using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public interface ISqlLanguageProvider
    {
        #region Properties
        
        DatabaseProviderType ProviderType { get; }
        bool IsUpsertSupported { get; } 

        #endregion

        #region CRUD

        string Select<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderBy, bool orderByDescending, uint skip, uint take, bool lazyLoading);
        string Insert<T>(T model);
        string InsertBulk<T>(IEnumerable<T> models);
        string Update<T>(T model);
        string Delete<T>(T model);
        string Upsert<T>(IEnumerable<T> items); 

        #endregion
    }
}
