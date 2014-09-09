using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataEx.DataAnnotations;
using UtilEx;
using ColumnAttribute = System.Data.Linq.Mapping.ColumnAttribute;
using TableAttribute = System.Data.Linq.Mapping.TableAttribute;

namespace DataEx
{
    public class TableDefinition
    {
        #region Constructor

        public TableDefinition(Type type)
        {
            RelatedTables = new List<TableRelationDefinition>();
            TableType = type;
            Columns = new List<ColumnDefinition>();
            LoadMetaData();
        }

        #endregion

        #region Property Implementation

        public Type TableType { get; private set; }
        public List<TableRelationDefinition> RelatedTables { get; private set; }
        public IEnumerable<ColumnDefinition> Columns { get; private set; }
        public string TableName { get; private set; }
        public IEnumerable<ColumnDefinition> Keys
        {
            get
            {
                return Columns == null ? new ColumnDefinition[] { } : Columns.Where(i => i.IsKey);
            }
        }

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
            var type = TableType;
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            TableName = tableAttr == null ? type.Name : tableAttr.Name;
            var properties =
                type.GetProperties();
            foreach (var property in properties)
            {
                var notMapped = property.GetCustomAttribute<NotMappedAttribute>();
                if (notMapped != null)
                    continue;
                var relation = property.GetCustomAttribute<RelationAttribute>();
                if (relation != null)
                {
                    AddForeignKey(relation, property);
                    continue;
                }
                var column = GetColumnFromProperty(property);
                column.Table = this;
                AddColumn(column);
            }
            if(!Keys.Any())
                throw new ArgumentException("The entity {0} does not have a key defined".Fi(TableType.Name));
        }

        private ColumnDefinition GetColumnFromProperty(PropertyInfo property)
        {
            var key = property.GetCustomAttribute<KeyAttribute>();
            var columnInfo = property.GetCustomAttribute<ColumnAttribute>();
            var autoNumeric = property.GetCustomAttribute<AutoIncrementAttribute>();
            var columnName = columnInfo != null ? columnInfo.Name : property.Name;
            var column = new ColumnDefinition()
            {
                ColumnName = columnName,
                FieldName = property.Name,
                IsKey = (key != null || (property.Name.Equals("Id") && property.PropertyType == typeof(int))),
                IsAutoIncrement = (autoNumeric != null) || (property.Name.Equals("Id") && property.PropertyType == typeof(int))

            };
            return column;
        }

        private void AddForeignKey(RelationAttribute relation, PropertyInfo property)
        {
            var column = Columns.SingleOrDefault(i => i.ColumnName == relation.ForeignKey) ??
                         GetColumnFromProperty(TableType.GetProperty(relation.ForeignKey));
            var tableRelation = new TableRelationDefinition(property.PropertyType, column, relation.PrimaryKey, property.Name);
            RelatedTables.Add(tableRelation);
        }

        public void AddColumn(ColumnDefinition column)
        {
            ((List<ColumnDefinition>)Columns).Add(column);
        }


        #endregion
    }

    public class TableRelationDefinition : TableDefinition
    {
        public TableRelationDefinition(Type type, ColumnDefinition foreignKey, string primaryKey, string propertyName)
            : base(type)
        {
            ForeignKeyColumn = foreignKey;
            PrimaryKeyColumn = Columns.Single(i => i.ColumnName == primaryKey);
            PropertyName = propertyName;
        }

        public ColumnDefinition ForeignKeyColumn { get; private set; }
        public ColumnDefinition PrimaryKeyColumn { get; private set; }
        public string PropertyName { get; private set; }
    }

    public class ColumnDefinition
    {
        public TableDefinition Table { get; set; }
        public string FieldName { get; set; }
        public string ColumnName { get; set; }
        public int Ordinal { get; set; }
        public bool IsKey { get; set; }
        public bool IsUnique { get; set; }
        public bool AllowNulls { get; set; }
        public bool IsAutoIncrement { get; set; }
    }
}
