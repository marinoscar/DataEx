using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UtilEx;

namespace DataEx
{
    public class EntityQueryProvider<T> where T : class
    {

        private Type _targetType;

        public EntityQueryProvider()
        {
            _targetType = typeof(T);
            _tableDefinition = new TableDefinition<T>();
        }

        #region Variable Declaration

        private TableDefinition<T> _tableDefinition;

        #endregion

        #region Method Implementation

        public string GetSelectStatement(Expression<Func<T, bool>> expression)
        {
            return "SELECT {0} FROM {1} WHERE".Fi(string.Join(",", _tableDefinition.Columns.Select(i => i.ColumnName)), _tableDefinition.TableName,
                                                  GetWhereStatement(expression));
        }

        public string UpdateStatement(T item)
        {
            throw new NotImplementedException("");
        }

        public string GetWhereStatement(Expression<Func<T, bool>> expression)
        {
            return ResolveExpression(expression.Body);
        }

        private string ResolveBinaryExpression(Expression expression)
        {
            var localExpression = (BinaryExpression)expression;
            var left = ResolveExpression(localExpression.Left);
            var right = ResolveExpression(localExpression.Right);
            var oper = ResolveExpressionNodeType(localExpression.NodeType);
            return string.Format("({0} {1} {2})", left, oper, right);
        }

        private string ResolveExpression(Expression expression)
        {
            var type = expression.GetType();
            if (typeof(MemberExpression) == type || type.IsSubclassOf(typeof(MemberExpression)))
                return ResolveMemberExpression(expression);
            if (typeof(ConstantExpression) == type || type.IsSubclassOf(typeof(ConstantExpression)))
                return ResolveConstantExpression(expression);
            if (typeof(BinaryExpression) == type || type.IsSubclassOf(typeof(BinaryExpression)))
                return ResolveBinaryExpression(expression);
            throw new ArgumentException(string.Format("Expression type {0} is not supported", type));
        }

        private string ResolveMemberExpression(Expression expression)
        {
            var localExpression = (MemberExpression)expression;
            return localExpression.Member.Name;
        }

        private string ResolveConstantExpression(Expression expression)
        {
            var localExpression = (ConstantExpression)expression;
            return Convert.ToString(localExpression.Value.ToSql());
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
