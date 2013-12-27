using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataEx.DataAnnotations;

namespace DataEx
{
    public class TableDefinition<T> where T : class
    {

        private static TableDefinition<T> _instance;  

        #region Constructor
        
        public TableDefinition()
        {
            if (_instance != null) return;
            Columns = new List<ColumnDefinition>();
            LoadMetaData();
            _instance = this;
        } 

        #endregion

        #region Property Implementation

        public IEnumerable<ColumnDefinition> Columns { get; private set; }
        public string TableName { get; private set; }

        #endregion

        #region Method Implementation

        public IEnumerable<ColumnDefinition> GetNonAutoIncrementColumns()
        {
            return Columns.Where(i => !i.IsAutoIncrement);
        } 

        public IEnumerable<ColumnDefinition> GetNonKeyColumns()
        {
            return Columns.Where(i => !i.IsKey);
        } 

        public IEnumerable<ColumnDefinition> GetPrimaryKeys()
        {
            return Columns.Where(i => i.IsKey);
        }

        public IEnumerable<ColumnDefinition> GetUniqueKeys()
        {
            return Columns.Where(i => i.IsUnique);
        } 

        private void LoadMetaData()
        {
            var tableAttr = GetType().GetCustomAttribute<TableAttribute>();
            TableName = tableAttr == null ? GetType().Name : tableAttr.Name;
            var properties =
                GetType().GetProperties(BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public);
            foreach (var property in properties)
            {
                var key = property.GetCustomAttribute<KeyAttribute>();
                var columnInfo = property.GetCustomAttribute<ColumnAttribute>();
                var autoNumeric = property.GetCustomAttribute<AutoIncrementAttribute>();
                var columnName = columnInfo != null ? columnInfo.Name : property.Name;
                var column = new ColumnDefinition()
                    {
                        ColumnName = columnName,
                        FieldName = property.Name,
                        IsKey = key != null,
                        IsAutoIncrement = (autoNumeric != null) || (property.Name.Equals("Id") && property.PropertyType == typeof(int))
                       
                    };
                AddColumn(column);
            }
        }

        public void AddColumn(ColumnDefinition column)
        {
            ((List<ColumnDefinition>)Columns).Add(column);
        }

        
        #endregion
    }

    public class ColumnDefinition
    {
        public string FieldName { get; set; }
        public string ColumnName { get; set; }
        public int Ordinal { get; set; }
        public bool IsKey { get; set; }
        public bool IsUnique { get; set; }
        public bool AllowNulls { get; set; }
        public bool IsAutoIncrement { get; set; }
    }
}
