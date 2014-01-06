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
        IQueryable<T> Select<T>(Expression<Func<T, bool>> expression);
        IQueryable<T> Select<T>(Expression<Func<T, bool>> expression, bool lazyLoading);
        IQueryable<T> Get<T>();
    }
}
