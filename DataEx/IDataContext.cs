using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public interface IDataContext
    {
        int SaveChanges();
        void Add<T>(T item);
        void Update<T>(T item);
        void Remove<T>(T item);
        IEnumerable<T> Select<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderBy, bool orderByDescending, uint skip, uint take, bool lazyLoading);
    }
}
