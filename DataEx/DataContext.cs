using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public class DataContext : IDataContext
    {

        #region Variable Declaration

        private static Dictionary<string, IDataContext> _dataContexts;

        #endregion

        #region Constructors

        public DataContext()
            : this(GetDefaultConnString())
        {

        }

        public DataContext(string connectionString)
        {
            Items = new Dictionary<Type, object>();
            Database = new Database(connectionString);
        }
        #endregion

        #region Property Implementation

        protected Dictionary<Type, object> Items { get; private set; }
        public Database Database { get; private set; }

        public static Dictionary<string, IDataContext> DataContexts
        {
            get
            {
                if(_dataContexts == null)
                    return new Dictionary<string, IDataContext>();
                return _dataContexts;
            }
        }

        #endregion

        #region Private Methods

        private static string GetDefaultConnStringName()
        {
            var connStringName = ConfigurationManager.AppSettings["defaultConnStringName"];
            return string.IsNullOrWhiteSpace(connStringName) ? "" : connStringName;
        }

        private static string GetDefaultConnString()
        {
            var connStringName = GetDefaultConnStringName();
            ConnectionStringSettings firstConnString = null;
            if (!string.IsNullOrWhiteSpace(connStringName))
                firstConnString = ConfigurationManager.ConnectionStrings[connStringName];
            if (firstConnString == null && ConfigurationManager.ConnectionStrings.Count > 0)
                firstConnString = ConfigurationManager.ConnectionStrings[0];
            if (firstConnString == null) return string.Empty;
            return firstConnString.ConnectionString;
        }

        private DataList<T> GetDataList<T>()
        {
            var type = typeof(T);
            if (!Items.ContainsKey(type))
                Items[type] = new DataList<T>();
            return (DataList<T>)Items[type];
        }

        #endregion

        #region Public Methods

        public static IDataContext GetContext<T>() where T : IDataContext
        {
            var type = typeof (T).FullName;
            return DataContexts.ContainsKey(type) ? DataContexts[type] : null;
        }

        public void Add<T>(T item)
        {
            GetDataList<T>().Add(item);
        }

        public void Remove<T>(T item)
        {
            GetDataList<T>().Remove(item);
        }

        public IQueryable<T> Select<T>(Expression<Func<T, bool>> expression)
        {
            return Database.Select<T>(expression).AsQueryable();
        }

        public IQueryable<T> Get<T>()
        {
            return Database.Select<T>().AsQueryable();
        }

        public virtual int SaveChanges()
        {
            var count = 0;
            foreach (var modelType in Items)
            {
                var data = (DataList<object>)modelType.Value;
                var list = data.InnerList;
                var deleted = list.Where(i => i.Status == DataListItemStatus.Deleted);
                var inserted = list.Where(i => i.Status == DataListItemStatus.Added);
                var updated = list.Where(i => i.Status == DataListItemStatus.Updated);
                foreach (var item in deleted)
                {
                    Database.Delete(item);
                }
                foreach (var item in inserted)
                {
                    Database.Insert(item);
                }
                foreach (var item in updated)
                {
                    Database.Update(item);
                }
                count = count + (deleted.Count() + inserted.Count() + updated.Count());
            }
            return count;
        }

        #endregion
    }
}
