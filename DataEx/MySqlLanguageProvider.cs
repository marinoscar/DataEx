using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilEx;

namespace DataEx
{
    public class MySqlLanguageProvider : AnsiSqlLanguageProvider
    {

        private SqlLanguageProviderHelper _helper;

        #region Constructor

        public MySqlLanguageProvider()
            : this(DbConfiguration.Get<ISqlExpressionProvider>(), DbConfiguration.Get<IObjectAccesor>())
        {

        }

        public MySqlLanguageProvider(IObjectAccesor objectAccesor)
            : this(new SqlExpressionProvider(new MySqlDialectProvider()), objectAccesor)
        {

        }

        public MySqlLanguageProvider(ISqlExpressionProvider expressionProvider, IObjectAccesor objectAccesor)
            : base(expressionProvider, new MySqlDialectProvider(), objectAccesor)
        {
            _helper = new SqlLanguageProviderHelper(objectAccesor, DialectProvider);
        }

        #endregion

        public override string Select<T>(System.Linq.Expressions.Expression<Func<T, bool>> expression, System.Linq.Expressions.Expression<Func<T, object>> orderBy, bool orderByDescending, uint skip, uint take, bool lazyLoading)
        {
            var limit = string.Empty;
            var baseSql = base.Select<T>(expression, orderBy, orderByDescending, skip, take, lazyLoading);
            if (skip > take) throw new ArgumentException("skip canot be greater or equal than take");
            if (take > 0 && skip > 0)
                limit = "LIMIT {0}, {1}".Fi(skip, take);
            else if (skip <= 0 && take > 0)
                limit = "LIMIT {0}".Fi(take);
            return baseSql
                .Replace(QueryBeginComment, string.Empty)
                .Replace(SelectBeginComment, string.Empty)
                .Replace(QueryEndComment, limit);
        }

        public override string Upsert<T>(IEnumerable<T> items)
        {
            var tableDef = _helper.GetTableDefinition(typeof (T));
            var columns = tableDef.Columns.Where(i => !i.IsAutoIncrement);
            var sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO {0} ({1}) VALUES\n", _helper.GetQualifiedTableName(tableDef),
                            string.Join(",",columns.Select(i => _helper.GetQualifiedColumnName(i))));
            foreach (var item in items)
            {
                sb.AppendFormat("({0}),", string.Join(",", columns.Select(i => _helper.GetColumnValue(item, i))));
            }
            sb.Length--; /*remove last , */
            sb.AppendLine();
            sb.AppendFormat("ON DUPLICATE KEY UPDATE {0}",
                            string.Join(",",
                                        columns.Select(
                                            i => string.Format("{0}=VALUES({0})", _helper.GetQualifiedColumnName(i)))));
            return sb.ToString();

        }
    }
}
