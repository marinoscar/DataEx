using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public class NullDbTransactionProvider : IDbTransactionProvider
    {
        private readonly IDbConnectionProvider _connectionProvider;

        public NullDbTransactionProvider()
            : this(new DbConnectionProvider())
        {

        }


        public NullDbTransactionProvider(IDbConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public string ConnectionString { get; set; }

        public IDbConnection GetConnection(DatabaseProviderType providerType)
        {
            return _connectionProvider.GetConnection(providerType);
        }

        public virtual IDbTransaction BeginTransaction()
        {
            return null;
        }

        public virtual IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return null;
        }

        public bool ProvideTransaction
        {
            get { return false; }
        }

        public void Dispose()
        {
        }
    }
}
