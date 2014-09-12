using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UtilEx;

namespace DataEx
{
    public class AnsiSqlLanguageProvider : ISqlLanguageProvider
    {

        #region Variable Declaration

        private ISqlExpressionProvider _expressionProvider;
        private ISqlDialectProvider _dialectProvider;
        private IObjectAccesor _objectAccesor;
        private SqlLanguageProviderHelper _helper;

        internal const string QueryBeginComment = "/* QUERY-BEGIN */";
        internal const string QueryEndComment = "/* QUERY-END */";
        internal const string SelectBeginComment = "/* SELECT-BEGIN */";

        #endregion

        #region Constructor

        public AnsiSqlLanguageProvider(ISqlExpressionProvider expressionProvider, ISqlDialectProvider dialectProvider, IObjectAccesor objectAccesor)
        {
            _expressionProvider = expressionProvider;
            _dialectProvider = dialectProvider;
            _objectAccesor = objectAccesor;
            _helper = new SqlLanguageProviderHelper(_objectAccesor, _dialectProvider);
        }

        #endregion

        #region Property Implementation

        public DatabaseProviderType ProviderType { get { return DatabaseProviderType.None; } }
        public virtual bool IsUpsertSupported { get { return false; } }

        protected virtual ISqlExpressionProvider ExpressionProvider
        {
            get { return _expressionProvider; }
            set { _expressionProvider = value; }
        }

        protected virtual ISqlDialectProvider DialectProvider
        {
            get { return _dialectProvider; }
            set { _dialectProvider = value; }
        }

        protected virtual IObjectAccesor ObjectAccesor
        {
            get { return _objectAccesor; }
            set { _objectAccesor = value; }
        }

        #endregion

        #region Interface Methods

        public virtual string Select<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderBy, bool orderByDescending, uint skip, uint take, bool lazyLoading)
        {
            var modelType = typeof (T);
            var tableDef = _helper.GetTableDefinition(modelType);
            var sb = new StringBuilder();
            sb.AppendFormat("{0}\n",QueryBeginComment);
            sb.AppendFormat("SELECT {0}\n",SelectBeginComment);
            sb.AppendFormat("{0}\n", _helper.GetColumnNames(modelType, i => true, lazyLoading));
            sb.AppendFormat("FROM {0}\n", _helper.GetQualifiedTableName(tableDef));
            sb.AppendFormat("{0}\n", _helper.GetInnerJoins(modelType, lazyLoading));
            sb.AppendFormat("WHERE\n");
            sb.AppendFormat("{0}\n", ExpressionProvider.ResolveWhere(expression));
            sb.AppendFormat("{0}\n", ExpressionProvider.ResolveOrderBy(orderBy, orderByDescending));
            sb.AppendFormat("{0}\n",QueryEndComment);
            return sb.ToString();
        }

        public virtual string Insert<T>(T model)
        {
            var modelType = typeof(T);
            var tableDef = _helper.GetTableDefinition(modelType);
            var columns = tableDef.Columns.Where(i => !i.IsAutoIncrement);
            var columnNames = columns.Select(i => _helper.GetQualifiedColumnName(i));
            return Insert<T>(model, _helper.GetQualifiedTableName(tableDef), string.Join(",", columnNames), columns);
        }

        protected virtual string Insert<T>(T model, string tableName, string qualifiedColumnNames, IEnumerable<ColumnDefinition> columnValues)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}\n", QueryBeginComment);
            sb.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2})",
                            tableName,
                            qualifiedColumnNames,
                            string.Join(",", columnValues.Select(i => _helper.GetColumnValue(model, i))));
            sb.AppendFormat("{0}/\n", QueryEndComment);
            return sb.ToString();
        }

        public virtual string InsertBulk<T>(IEnumerable<T> models)
        {
            var modelType = typeof(T);
            var tableDef = _helper.GetTableDefinition(modelType);
            var columns = tableDef.Columns.Where(i => !i.IsAutoIncrement);
            var columnNames = columns.Select(i => _helper.GetQualifiedColumnName(i));
            var sb = new StringBuilder();
            foreach (var model in models)
            {
                sb.AppendLine(Insert<T>(model, _helper.GetQualifiedTableName(tableDef), string.Join(",", columnNames),
                                        columns));
            }
            return sb.ToString();
        }

        public virtual string Update<T>(T model)
        {
            var modelType = typeof(T);
            var tableDef = _helper.GetTableDefinition(modelType);
            var primaryKeys = tableDef.Columns.Where(i => i.IsKey);
            var columns = tableDef.Columns.Where(i => !primaryKeys.Contains(i));
            var updateValues = new List<string>(columns.Count());
            var updateKeys = new List<string>(primaryKeys.Count());
            updateValues.AddRange(columns.Select(column => _helper.GetQualifiedColumnAndValue(column, model)));
            updateKeys.AddRange(primaryKeys.Select(column => _helper.GetQualifiedColumnAndValue(column, model)));
            var sb = new StringBuilder();
            sb.AppendFormat("{0}\n", QueryBeginComment);
            sb.AppendFormat("UPDATE {0} SET\n", _helper.GetQualifiedTableName(tableDef));
            sb.AppendFormat("{0}\n", string.Join(", ", updateValues));
            sb.AppendFormat("WHERE\n");
            sb.AppendFormat("{0}\n", string.Join(" AND ", updateKeys));
            sb.AppendFormat("{0}/\n", QueryEndComment);
            return sb.ToString();
        }

        public virtual string Delete<T>(T model)
        {
            var modelType = typeof(T);
            var tableDef = _helper.GetTableDefinition(modelType);
            var primaryKeys = tableDef.Columns.Where(i => i.IsKey);
            var updateKeys = new List<string>(primaryKeys.Count());
            updateKeys.AddRange(primaryKeys.Select(column => _helper.GetQualifiedColumnAndValue(column, model)));
            var sb = new StringBuilder();
            sb.AppendFormat("{0}\n", QueryBeginComment);
            sb.AppendFormat("DELETE FROM {0} ", _helper.GetQualifiedTableName(tableDef));
            sb.AppendFormat("WHERE {0}", string.Join(" AND ", updateKeys));
            sb.AppendFormat("{0}/\n", QueryEndComment);
            return sb.ToString();
        }

        public virtual string Upsert<T>(IEnumerable<T> items)
        {
            throw  new InvalidOperationException("Upsert is not supported for provider {0}".Fi(GetType().FullName));
        }

        #endregion
    }
}
