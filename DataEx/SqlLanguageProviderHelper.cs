using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilEx;

namespace DataEx
{
    public class SqlLanguageProviderHelper
    {
        private readonly IObjectAccesor _objectAccesor;
        private readonly ISqlDialectProvider _dialectProvider;

        public SqlLanguageProviderHelper(IObjectAccesor objectAccesor, ISqlDialectProvider dialectProvider)
        {
            _objectAccesor = objectAccesor;
            _dialectProvider = dialectProvider;
        }

        public string GetQualifiedColumnAndValue(ColumnDefinition column, object model)
        {
            return "{0} = {1}".Fi(GetQualifiedColumnName(column), GetColumnValue(model, column));
        }

        public string GetColumnValue(object model, ColumnDefinition column)
        {
            return _objectAccesor.GetPropertyValue(model, column.FieldName).ToSql();
        }

        public string GetQualifiedTableName(TableDefinition table)
        {
            return "{0}{2}{1}".Fi(_dialectProvider.SystemNameStartCharacter, _dialectProvider.SystemNameEndCharacter, table.TableName);
        }

        public string GetQualifiedColumnName(ColumnDefinition column)
        {
            return "{0}{2}{1}.{0}{3}{1}".Fi(_dialectProvider.SystemNameStartCharacter,
                                            _dialectProvider.SystemNameEndCharacter, column.Table.TableName,
                                            column.ColumnName);
        }

        public string GetColumnNames(Type modelType, Func<ColumnDefinition, bool> where, bool lazyLoading)
        {
            var returnValue = new List<string>();
            var tableDef = GetTableDefinition(modelType);
            var columns = tableDef.Columns.Where(where);
            var standardColumns = string.Join(",",
                               columns.Select(GetQualifiedColumnName));
            returnValue.Add(standardColumns);
            if (!lazyLoading && tableDef.RelatedTables.Any())
            {
                foreach (var relatedTable in tableDef.RelatedTables)
                {
                    returnValue.Add(string.Join(",", relatedTable.Columns.Select(i => string.Format("{0}{2}{1}.{0}{3}{1} As {0}{4}{5}{6}{7}{8}{9}{1}",
                        _dialectProvider.SystemNameStartCharacter, _dialectProvider.SystemNameEndCharacter, relatedTable.TableName, i.ColumnName,
                        DbConfiguration.ExtendedFieldPrefix, relatedTable.PropertyName, DbConfiguration.ExtendedFieldSeparator, relatedTable.TableType.Name,
                        DbConfiguration.ExtendedFieldSeparator, i.FieldName))));
                }
            }
            return string.Join(",", returnValue);
        }

        public string GetInnerJoins(Type modelType, bool lazyLoading)
        {
            var tableDef = GetTableDefinition(modelType);
            if (tableDef.RelatedTables.Count <= 0 || lazyLoading) return string.Empty;
            var sb = new StringBuilder();
            sb.AppendLine(" ");
            foreach (var relation in tableDef.RelatedTables)
            {
                sb.AppendLine("INNER JOIN {0} ON {1} = {2}".Fi(
                    GetQualifiedTableName(relation.ForeignKeyColumn.Table),
                    GetQualifiedColumnName(relation.PrimaryKeyColumn),
                    GetQualifiedColumnName(relation.ForeignKeyColumn)));
            }
            return sb.ToString();
        }

        public TableDefinition GetTableDefinition(Type modelType)
        {
            var cacheProvider =
                ObjectCacheProvider.GetProvider<Type, TableDefinition>(TableDefinition.TableDefinitionCacheKey);
            return cacheProvider.GetCacheItem(modelType, i => new TableDefinition(modelType));
        }
    }
}
