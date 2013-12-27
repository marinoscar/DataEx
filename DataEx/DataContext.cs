using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public class DataContext
    {

        public DataContext()
        {
            Items = new Dictionary<Type, object>();
        }

        protected Dictionary<Type, object> Items { get; private set; }

        private DataList<T> GetDataList<T>()
        {
            var type = typeof (T);
            if (!Items.ContainsKey(type))
                Items[type] = new DataList<T>();
            return (DataList<T>)Items[type];
        } 

        public void Set<T>(T item)
        {
            GetDataList<T>().Add(item);
        } 
        public IQueryable<T> Get<T>()
        {
            return GetDataList<T>();
        } 

        public void Remove<T>(T item)
        {
            GetDataList<T>().Remove(item);
        }

        public virtual int SaveChanges()
        {
            return 0;
        }
    }
}
