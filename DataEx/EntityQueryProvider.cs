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
    public class EntityQueryProvider<T> : IEntityQueryProvider
    {

        public EntityQueryProvider()
        {
            TableDefinition = new TableDefinition(typeof(T));
            ObjectAccessor = new FastReflectionObjectAccessor();
        }

        #region Variable Declaration

        protected const int BatchSize = 9216;
        protected readonly TableDefinition TableDefinition;
        protected readonly IObjectAccesor ObjectAccessor;

        #endregion

        #region Method Implementation

        protected virtual IEnumerable<string> GetUpsertStatement(IEnumerable<T> items)
        {
            return null;
        }

        protected string GetInsertHeader()
        {
            return "INSERT INTO {0} ({1}) VALUES".Fi(TableDefinition.TableName,
                                              string.Join(", ", TableDefinition.GetNonAutoIncrementColumns().Select(i => i.ColumnName)));
        }

        protected string GetInsertValues(T item)
        {
            var values =
                TableDefinition.GetNonAutoIncrementColumns()
                               .Select(i => ObjectAccessor.GetPropertyValue(item, i.FieldName).ToSql())
                               .ToList();
            return "({0})".Fi(string.Join(", ", values));
        }

        public string GetInsertStatement(T item)
        {
            return "{0} {1}".Fi(GetInsertHeader(), GetInsertValues(item));
        }

        public string GetSelectStatement(Expression<Func<T, bool>> expression, bool lazyLoading = true)
        {
            return @"
SELECT
    {0}
FROM
    {1}{3}
WHERE
    {2}".Fi(GetSelectColumnNames(lazyLoading), TableDefinition.TableName, GetWhereStatement(expression), GetInnerJoins(TableDefinition, lazyLoading));
        }

        private static string GetInnerJoins(TableDefinition table, bool lazyLoading)
        {
            if (table.RelatedTables.Count <= 0 || lazyLoading) return string.Empty;
            var sb = new StringBuilder();
            sb.AppendLine(" ");
            foreach (var relation in table.RelatedTables)
            {
                sb.AppendLine(" INNER JOIN {0} ON {1}.{2} = {3}.{4}".Fi(relation.TableName, table.TableName,
                                                                        relation.ForeignKeyColumn.ColumnName, relation.TableName,
                                                                        relation.PrimaryKeyColumn.ColumnName));
            }
            return sb.ToString();
        }

        private string GetSelectColumnNames()
        {
            return GetSelectColumnNames(true);
        }

        private string GetSelectColumnNames(bool lazyLoading)
        {
            return GetSelectColumnNames(TableDefinition.Columns, lazyLoading);
        }

        private string GetSelectColumnNames(IEnumerable<ColumnDefinition> columns, bool lazyLoading = true)
        {
            var result = new List<string>();
            var primaryColumns = string.Join(",",
                               columns.Select(
                                   i => string.Format("{0}.{1} As {2}", i.Table.TableName, i.ColumnName, i.FieldName)));
            result.Add(primaryColumns);
            if (!lazyLoading)
            {
                var table = columns.First().Table;
                result.AddRange(table.RelatedTables.Select(GetSelectColumnsForFk));
            }
            return string.Join(",", result);
        }

        private string GetSelectColumnsForFk(TableRelationDefinition relation)
        {
            return string.Join(",",
                               relation.Columns.Select(i => string.Format("{0}.{1} As {2}{3}{4}{5}{6}{7}", relation.TableName, i.ColumnName, Database.ExtendedFieldPrefix, relation.PropertyName, Database.ExtendedFieldSeparator, relation.TableType.Name, Database.ExtendedFieldSeparator, i.FieldName)));
        }

        public string GetSelectStatement(bool lazyLoading = true)
        {
            return @"
SELECT
    {0}
FROM
    {1}{2}".Fi(GetSelectColumnNames(), TableDefinition.TableName, GetInnerJoins(TableDefinition, lazyLoading));
        }

        public string GetSelectStatement(T item, bool lazyLoading = true)
        {
            return @"
SELECT
    {0}
FROM
    {1}{3}
WHERE
    {2}".Fi(GetSelectColumnNames(), TableDefinition.TableName, GetPrimaryKeyValueAssigment(item), GetInnerJoins(TableDefinition, lazyLoading));
        }

        public string GetUpdateStatement(T item)
        {
            return "UPDATE {0} SET {1} WHERE {2}".Fi(TableDefinition.TableName, GetFieldValueAssigment(item), GetPrimaryKeyValueAssigment(item));
        }

        public string GetUpdateStatement(T item, Expression<Func<T, bool>> expression)
        {
            return "UPDATE {0} SET {1} WHERE {2}".Fi(TableDefinition.TableName, GetFieldValueAssigment(item), GetWhereStatement(expression));
        }

        public string GetDeleteStatement(T item)
        {
            return "DELETE FROM {0} WHERE {1}".Fi(TableDefinition.TableName, GetPrimaryKeyValueAssigment(item));
        }

        public string GetDeleteStatement(Expression<Func<T, bool>> expression)
        {
            return "DELETE FROM {0} WHERE {1}".Fi(TableDefinition.TableName, GetWhereStatement(expression));
        }

        public string GetWhereStatement(Expression<Func<T, bool>> expression)
        {
            return ResolveExpression(expression.Body);
        }

        private string GetPrimaryKeyValueAssigment(T item)
        {
            return GetFieldAssigment(item, TableDefinition.GetPrimaryKeys(), " And ");
        }

        private string GetFieldValueAssigment(T item)
        {
            return GetFieldAssigment(item, TableDefinition.GetNonKeyColumns(), ", ");
        }

        private string GetFieldAssigment(T item, IEnumerable<ColumnDefinition> columns, string separator)
        {
            var fields = new List<string>();
            foreach (var column in columns)
            {
                fields.Add("{0}.{1} = {2}".Fi(column.Table.TableName, column.ColumnName, ObjectAccessor.GetPropertyValue(item, column.FieldName).ToSql()));
            }
            return string.Join(separator, fields);
        }

        private string ResolveBinaryExpression(Expression expression)
        {
            var localExpression = (BinaryExpression)expression;
            var left = ResolveExpression(localExpression.Left);
            var right = ResolveExpression(localExpression.Right);
            var oper = ResolveExpressionNodeType(localExpression.NodeType);
            return string.Format("({0} {1} {2})", left, oper, right);
        }

        private static bool IsConstantExpression(Type type)
        {
            return typeof(ConstantExpression) == type || type.IsSubclassOf(typeof(ConstantExpression));
        }

        private string ResolveExpression(Expression expression)
        {
            var type = expression.GetType();
            if (typeof(MemberExpression) == type || type.IsSubclassOf(typeof(MemberExpression)))
                return ResolveMemberExpression((MemberExpression)expression);
            if (IsConstantExpression(type))
                return ResolveConstantExpression(expression);
            if (typeof(MethodCallExpression) == type || type.IsSubclassOf(typeof(MethodCallExpression)))
                return ResolveMethodExpression(expression);
            if (typeof(BinaryExpression) == type || type.IsSubclassOf(typeof(BinaryExpression)))
                return ResolveBinaryExpression(expression);
            throw new ArgumentException(string.Format("Expression type {0} is not supported", type));
        }

        private string ResolveMethodExpression(Expression expression)
        {
            var localExpression = (MethodCallExpression)expression;
            var memberExpression = (MemberExpression)localExpression.Object;
            var value = GetConstantValueFromExpression(memberExpression.Expression, memberExpression);
            return Convert.ToString(localExpression.Method.Invoke(value, null)).ToSql();
        }

        private string ResolveMemberExpression(MemberExpression expression)
        {
            if (expression.Expression != null && expression.Expression.NodeType == ExpressionType.Parameter ||
                expression.Expression.NodeType == ExpressionType.Convert)
            {
                var propertyInfo = (PropertyInfo)expression.Member;
                return "{0}.{1}".Fi(TableDefinition.TableName, TableDefinition.Columns.Single(i => i.FieldName == propertyInfo.Name).ColumnName);
            }
            var member = Expression.Convert(expression, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter().ToSql();
        }

        private string ResolveConstantExpression(Expression expression, MemberExpression memberExpression = null)
        {
            return GetConstantValueFromExpression(expression, memberExpression).ToSql();
        }

        private object GetConstantValueFromExpression(Expression expression, MemberExpression memberExpression = null)
        {
            var localExpression = (ConstantExpression)expression;
            var member = Expression.Convert(localExpression, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter();
            //var value = localExpression.Value;
            //if (Type.GetTypeCode(value.GetType()) == TypeCode.Object)
            //{
            //    FieldInfo field;
            //    if (memberExpression != null)
            //    {
            //        var member = memberExpression.Member.Name;
            //        field = value.GetType().GetField(member);
            //    }
            //    else
            //    {
            //        field = value.GetType().GetFields().FirstOrDefault();
            //    }
            //    value = field != null ? field.GetValue(value) : null;
            //}
            //return value;
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

        public virtual string GetInsertStatement(object item)
        {
            return GetInsertStatement((T)item);
        }

        public virtual IEnumerable<string> GetBulkInsertStatement(IEnumerable<object> items)
        {
            return items.Select(GetInsertStatement);
        }

        public virtual string GetUpdateStatement(object item)
        {
            return GetUpdateStatement((T)item);
        }

        public virtual string GetDeleteStatement(object item)
        {
            return GetDeleteStatement((T)item);
        }
    }

    public interface IEntityQueryProvider
    {
        string GetInsertStatement(object item);
        IEnumerable<string> GetBulkInsertStatement(IEnumerable<object> items);
        string GetUpdateStatement(object item);
        string GetDeleteStatement(object item);

    }
}
