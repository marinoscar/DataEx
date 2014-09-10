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
        #region Constructor

        public MySqlLanguageProvider()
            : this(new SqlExpressionProvider(new MySqlDialectProvider()), new FastReflectionObjectAccessor())
        {

        }

        public MySqlLanguageProvider(IObjectAccesor objectAccesor)
            : this(new SqlExpressionProvider(new MySqlDialectProvider()), objectAccesor)
        {
            
        }

        public MySqlLanguageProvider(ISqlExpressionProvider expressionProvider, IObjectAccesor objectAccesor)
            : base(expressionProvider, new MySqlDialectProvider(), objectAccesor)
        {
        }

        #endregion

        public override string Select<T>(System.Linq.Expressions.Expression<Func<T, bool>> expression, System.Linq.Expressions.Expression<Func<T, object>> orderBy, bool orderByDescending, uint skip, uint take, bool lazyLoading)
        {
            var limit = string.Empty;
            var baseSql = base.Select<T>(expression, orderBy, orderByDescending, skip, take, lazyLoading);
            if (skip > take) throw new ArgumentException("skip canot be greater or equal than take");
            if (take > 0 && skip > 0)
                limit = "LIMIT {0}, {1}".Fi(skip, take);
            else if(skip <=0 && take > 0)
                limit = "LIMIT {0}".Fi(take);
            return baseSql
                .Replace(QueryBeginComment, string.Empty)
                .Replace(SelectBeginComment, string.Empty)
                .Replace(QueryEndComment, limit);
        }
    }
}
