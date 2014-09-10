using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public interface ISqlExpressionProvider
    {
        string ResolveWhere<T>(Expression<Func<T, bool>> expression);
        string ResolveOrderBy<T>(Expression<Func<T, object>> orderBy, bool @descending);
    }
}
