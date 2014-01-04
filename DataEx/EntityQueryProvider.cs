using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UtilEx;

namespace DataEx
{
    public class EntityQueryProvider<T>
    {

        public EntityQueryProvider()
        {
            TableDefinition = new TableDefinition<T>();
            ClassHelper = new ClassHelper<T>();
        }

        #region Variable Declaration

        protected const int BatchSize = 9216;
        protected readonly TableDefinition<T> TableDefinition;
        protected readonly ClassHelper<T> ClassHelper;

        #endregion

        #region Method Implementation

        protected virtual IEnumerable<string> GetBatchInsertStatement(IEnumerable<T> items)
        {
            return null;
        }
        protected virtual IEnumerable<string> GetUpsertStatement(IEnumerable<T> items)
        {
            return null;
        }

        protected string GetInsertHeader()
        {
            return "INSERT INTO {0} ({1}) VALUES".Fi(TableDefinition.TableName,
                                              string.Join(", ", TableDefinition.GetNonAutoIncrementColumns()));
        }

        protected string GetInsertValues(T item)
        {
            return "({0})".Fi(TableDefinition.GetNonAutoIncrementColumns().Select(i => ClassHelper.GetPropertyValue<object>(item, i.FieldName).ToSql()));
        }

        public string GetInsertStatement(T item)
        {
            return "{0} {1}".Fi(GetInsertHeader(), GetInsertValues(item));
        }

        public string GetSelectStatement(Expression<Func<T, bool>> expression)
        {
            return "SELECT {0} FROM {1} WHERE {2}".Fi(string.Join(",", TableDefinition.Columns.Select(i => i.ColumnName)), TableDefinition.TableName,
                                                  GetWhereStatement(expression));
        }

        public string GetSelectStatement()
        {
            return "SELECT {0} FROM {1}".Fi(string.Join(",", TableDefinition.Columns.Select(i => i.ColumnName)), TableDefinition.TableName);
        }

        public string GetSelectStatement(T item)
        {
            return "SELECT {0} FROM {1} WHERE {2}".Fi(string.Join(",", TableDefinition.Columns.Select(i => i.ColumnName)), TableDefinition.TableName, GetPrimaryKeyValueAssigment(item));
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
                fields.Add("{0} = {1}".Fi(column.ColumnName, ClassHelper.GetPropertyValue<object>(item, column.FieldName).ToSql()));
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
