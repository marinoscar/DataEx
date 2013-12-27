using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilEx;

namespace DataEx
{
    public class MySqlEntityQueryProvider<T> : EntityQueryProvider<T> where T : class
    {
        public MySqlEntityQueryProvider()
        {

        }

        protected override IEnumerable<string> GetBatchInsertStatement(IEnumerable<T> items)
        {
            var batches = new List<string>();
            var sb = new StringBuilder(BatchSize);
            sb.AppendFormat("{0}", GetInsertHeader());
            foreach (var item in items)
            {
                sb.AppendFormat("({0}),", GetInsertValues(item));
                if (sb.Length > BatchSize)
                {
                    sb.Length--;
                    batches.Add(sb.ToString());
                    sb.Clear();
                    sb.AppendFormat("{0}", GetInsertHeader());
                }
            }
            sb.Length--;
            batches.Add(sb.ToString());
            return batches;
        }


        private string GetOnDuplicaKeyUpdateStatement()
        {
            var sb = new StringBuilder();
            sb.Append("\nON DUPLICATE KEY UPDATE");
            foreach (var column in TableDefinition.GetNonKeyColumns())
            {
                sb.AppendFormat(" {0} = VALUES({0}),", column.ColumnName);
            }
            sb.Length--;
            return sb.ToString();
        }

        protected override IEnumerable<string> GetUpsertStatement(IEnumerable<T> items)
        {
            var batch = new List<string>();
            batch.AddRange(GetBatchInsertStatement(items));
            for (var i = 0; i < batch.Count; i++)
            {
                batch[i] = "{0}{1}".Fi(batch[i], GetOnDuplicaKeyUpdateStatement());
            }
            return batch;
        }
    }
}
