using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public class DataList<T> : IList<T>, IList, IDataList, IDataListItems, IQueryable<T>
    {

        #region Constructors

        public DataList()
        {
            InnerList = new List<DataListItem<T>>();
            _innerQueryable = InnerList.Select(i => i.Item).AsQueryable<T>();
        }

        #endregion

        private readonly IQueryable<T> _innerQueryable;

        #region Properties

        protected internal List<DataListItem<T>> InnerList { get; private set; }

        public Expression Expression
        {
            get
            {
                return _innerQueryable.Expression;
            }
        }
        public Type ElementType
        {
            get
            {
                return typeof(T);
            }
        }
        public IQueryProvider Provider { get { return _innerQueryable.Provider; } }

        #endregion

        #region Methods

        protected List<T> GetItemsList()
        {
            return InnerList.Where(i => i.Status != DataListItemStatus.Deleted).Select(i => i.Item).ToList();
        }

        public int GetItemsChangedCount()
        {
            return InnerList.Count(i => i.Status != DataListItemStatus.Unchanged);
        }

        protected List<DataListItem<T>> GetDeleted()
        {
            return InnerList.Where(i => i.Status == DataListItemStatus.Deleted).ToList();
        }

        public void UpdateItems()
        {
            var deleted = GetDeleted();
            var deletedCount = deleted.Count();
            for (var i = 0; i < deletedCount; i++)
            {
                InnerList.Remove(deleted[i]);
            }
            InnerList.ForEach(i => i.MarkAsUnChanged());
        }

        public List<T> ToList()
        {
            return GetItemsList();
        }

        #endregion

        #region Overriden Methods

        public IEnumerator<T> GetEnumerator()
        {
            return GetItemsList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            Add(item, false);
        }

        public void Update(T item)
        {
            Add(item, true);
        }

        private void Add(T item, bool isUpdate)
        {
            var newItem = new DataListItem<T>(item);
            if (isUpdate)
                newItem.MarkAsUpdated();
            InnerList.Add(newItem);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public int Add(object value)
        {
            Add((T)value);
            return 1;
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public void Clear()
        {
            InnerList.Clear();
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        public bool Contains(T item)
        {
            return GetItemsList().Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            var innerItem = InnerList.Where(i => i.Status != DataListItemStatus.Deleted).SingleOrDefault(i => i.Item.Equals(item));
            if (innerItem == null)
            {
                innerItem = new DataListItem<T>(item);
                InnerList.Add(innerItem);
            }
            innerItem.MarkAsDeleted();
            return true;
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return GetItemsList().Count; }
        }

        public object SyncRoot { get { return GetItemsList(); } }
        public bool IsSynchronized
        {
            get { return true; }
        }
        public bool IsReadOnly { get { return false; } }
        public bool IsFixedSize
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            return GetItemsList().IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            InnerList.Insert(index, new DataListItem<T>(item));
        }

        public void RemoveAt(int index)
        {
            var innerItem = InnerList[index];
            innerItem.MarkAsDeleted();
        }

        object IList.this[int index]
        {
            get { return InnerList[index].Item; }
            set { InnerList[index].Item = (T)value; }
        }

        public T this[int index]
        {
            get { return InnerList[index].Item; }
            set { InnerList[index].Item = value; }
        }

        #endregion

        public IEnumerable<IDataListItem> GetItems()
        {
            return InnerList;
        }

        public void ClearItems()
        {
            InnerList.Clear();
        }
    }

    public class DataListItem<T> : IDataListItem
    {

        private readonly int _hash;

        public DataListItem(T item)
        {
            Item = item;
            _hash = item.GetHashCode();

        }
        public T Item { get; set; }
        public object Value { get { return Item; } }
        public DataListItemStatus Status { get; private set; }

        protected internal void UpdateStatus()
        {
            var newHash = Item.GetHashCode();
            if (newHash != _hash && Status == DataListItemStatus.Unchanged) Status = DataListItemStatus.Updated;
        }

        protected internal void MarkAsDeleted()
        {
            Status = DataListItemStatus.Deleted;
        }

        protected internal void MarkAsUpdated()
        {
            Status = DataListItemStatus.Updated;
        }

        protected internal void MarkAsUnChanged()
        {
            Status = DataListItemStatus.Unchanged;
        }

    }

    public enum DataListItemStatus
    {
        Added, Updated, Deleted, Unchanged
    }

    public interface IDataList
    {
        void UpdateItems();
        int GetItemsChangedCount();
    }

    public interface IDataListItems
    {
        IEnumerable<IDataListItem> GetItems();
        void ClearItems();
    }

    public interface IDataListItem
    {
        object Value { get; }
        DataListItemStatus Status { get; }
    }
}
