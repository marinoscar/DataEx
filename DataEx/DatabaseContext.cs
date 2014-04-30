﻿using System;
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
    public class DatabaseContext : IDataContext
    {

        #region Variable Declaration

        private static Dictionary<string, IDataContext> _dataContexts;

        #endregion

        #region Constructors

        public DatabaseContext()
            : this(GetDefaultConnString())
        {

        }

        public DatabaseContext(string connectionString)
        {
            Items = new Dictionary<Type, IDataListItems>();
            Database = new Database(connectionString);
        }
        #endregion

        #region Property Implementation

        protected Dictionary<Type, IDataListItems> Items { get; private set; }
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

        public void Update<T>(T item)
        {
            GetDataList<T>().Add(item, true);
        }

        public void Remove<T>(T item)
        {
            GetDataList<T>().Remove(item);
        }

        public IQueryable<T> Select<T>(Expression<Func<T, bool>> expression)
        {
            return Database.Select<T>(expression).AsQueryable();
        }

        public IQueryable<T> Select<T>(Expression<Func<T, bool>> expression, bool lazyLoading)
        {
            return Database.Select<T>(expression, lazyLoading).AsQueryable();
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
                var list = modelType.Value.GetItems();
                var deleted = list.Where(i => i.Status == DataListItemStatus.Deleted);
                var inserted = list.Where(i => i.Status == DataListItemStatus.Added);
                var updated = list.Where(i => i.Status == DataListItemStatus.Updated);
                foreach (var item in deleted)
                {
                    OnDeletingRecord(item.Value);
                    Database.Delete(item.Value);
                    OnDeletedRecord(item.Value);
                }
                foreach (var item in inserted)
                {
                    OnInsertingRecord(item.Value);
                    Database.Insert(item.Value);
                    OnInsertedRecord(item.Value);
                }
                foreach (var item in updated)
                {
                    OnUpdatingRecord(item.Value);
                    Database.Update(item.Value);
                    OnUpdatedRecord(item.Value);
                }
                count = count + (deleted.Count() + inserted.Count() + updated.Count());
                modelType.Value.ClearItems();
            }
            return count;
        }

        protected virtual void OnInsertingRecord(object entity) { }
        protected virtual void OnUpdatingRecord(object entity) { }
        protected virtual void OnDeletingRecord(object entity) { }
        protected virtual void OnInsertedRecord(object entity) { }
        protected virtual void OnUpdatedRecord(object entity) { }
        protected virtual void OnDeletedRecord(object entity) { }

        #endregion
    }
}