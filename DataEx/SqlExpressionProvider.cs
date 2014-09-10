using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UtilEx;

namespace DataEx
{
    public class SqlExpressionProvider : ISqlExpressionProvider
    {

        #region Variable Declaration
        
        private ISqlDialectProvider _sqlDialectProvider;
        private SqlLanguageProviderHelper _helper;

        #endregion

        #region Constructors

        public SqlExpressionProvider()
            : this(SqlDialectProviderFactory.GetProvider(DbConfiguration.DefaultProviderType))
        {
        }

        public SqlExpressionProvider(ISqlDialectProvider sqlDialectProvider)
        {
            _sqlDialectProvider = sqlDialectProvider;
            _helper = new SqlLanguageProviderHelper(new FastReflectionObjectAccessor(), _sqlDialectProvider);
        } 

        #endregion

        #region Interface Members

        public string ResolveWhere<T>(Expression<Func<T, bool>> expression)
        {
            return ResolveExpression(expression.Body, typeof(T));
        }

        public string ResolveOrderBy<T>(Expression<Func<T, object>> orderBy, bool descending)
        {
            if (orderBy == null) return string.Empty;
            var tableDef = GetTableDefinition(typeof (T));
            var expression = (UnaryExpression)orderBy.Body;
            var memberExpression = (MemberExpression) expression.Operand;
            var column = tableDef.Columns.Single(i => i.FieldName == memberExpression.Member.Name);
            return "ORDER BY {0} {1}".Fi(_helper.GetQualifiedColumnName(column), descending ? "DESC" : "ASC");
        }

        #endregion

        #region Expresion Methods

        private string ResolveBinaryExpression(Expression expression, Type modelType)
        {
            var localExpression = (BinaryExpression)expression;
            var left = ResolveExpression(localExpression.Left, modelType);
            var right = ResolveExpression(localExpression.Right, modelType);
            var oper = ResolveExpressionNodeType(localExpression.NodeType);
            return string.Format("({0} {1} {2})", left, oper, right);
        }

        private static bool IsConstantExpression(Type type)
        {
            return typeof(ConstantExpression) == type || type.IsSubclassOf(typeof(ConstantExpression));
        }

        private string ResolveExpression(Expression expression, Type modelType)
        {
            var type = expression.GetType();
            if (typeof(MemberExpression) == type || type.IsSubclassOf(typeof(MemberExpression)))
                return ResolveMemberExpression((MemberExpression)expression, modelType);
            if (IsConstantExpression(type))
                return ResolveConstantExpression(expression);
            if (typeof(MethodCallExpression) == type || type.IsSubclassOf(typeof(MethodCallExpression)))
                return ResolveMethodExpression(expression);
            if (typeof(BinaryExpression) == type || type.IsSubclassOf(typeof(BinaryExpression)))
                return ResolveBinaryExpression(expression, modelType);
            throw new ArgumentException(string.Format("Expression type {0} is not supported", type));
        }

        private string ResolveMethodExpression(Expression expression)
        {
            var localExpression = (MethodCallExpression)expression;
            var memberExpression = (MemberExpression)localExpression.Object;
            var value = GetConstantValueFromExpression(memberExpression.Expression);
            return Convert.ToString(localExpression.Method.Invoke(value, null)).ToSql();
        }

        private string ResolveMemberExpression(MemberExpression expression, Type modelType)
        {
            if (expression.Expression != null && expression.Expression.NodeType == ExpressionType.Parameter ||
                expression.Expression.NodeType == ExpressionType.Convert)
            {
                var table = GetTableDefinition(modelType);
                var propertyInfo = (PropertyInfo)expression.Member;
                var column = table.Columns.Single(i => i.FieldName == propertyInfo.Name);
                return _helper.GetQualifiedColumnName(column);
            }
            var member = Expression.Convert(expression, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter().ToSql();
        }

        private static TableDefinition GetTableDefinition(Type modelType)
        {
            var cacheProvider =
                    ObjectCacheProvider.GetProvider<Type, TableDefinition>(TableDefinition.TableDefinitionCacheKey);
            return cacheProvider.GetCacheItem(modelType, i => new TableDefinition(modelType));
        }

        private string ResolveConstantExpression(Expression expression)
        {
            return GetConstantValueFromExpression(expression).ToSql();
        }

        private object GetConstantValueFromExpression(Expression expression)
        {
            var localExpression = (ConstantExpression)expression;
            var member = Expression.Convert(localExpression, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter();
        }

        private string ResolveExpressionNodeType(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.AndAlso:
                    return "And";
                case ExpressionType.And:
                    return "And";
                case ExpressionType.OrElse:
                    return "Or";
                case ExpressionType.Or:
                    return "Or";
                case ExpressionType.NotEqual:
                    return "<>";
            }
            throw new ArgumentException(string.Format("ExpressionType {0} not supported", nodeType));
        }

        #endregion
    }
}
