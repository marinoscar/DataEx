using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public class DbTransactionProvider : IDbTransactionProvider
    {

        private IDbConnection _connection;
        private readonly IDbConnectionProvider _connectionProvider;
        private DatabaseProviderType _providerType;

        public bool ProvideTransaction
        {
            get { return true; }
        }

        public DbTransactionProvider(IDbConnectionProvider connectionProvider) : this(connectionProvider, DbConfiguration.DefaultProviderType)
        {
        }

        public DbTransactionProvider(IDbConnectionProvider connectionProvider, DatabaseProviderType providerType)
        {
            _connectionProvider = connectionProvider;
            _providerType = providerType;
        }

        public IDbTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            var connection = GetConnection(_providerType);
            if(_connection.State == ConnectionState.Closed)
                connection.Open();
            return connection.BeginTransaction(isolationLevel);
        }

        public IDbConnection GetConnection(DatabaseProviderType providerType)
        {
            _providerType = providerType;
            return _connection ?? (_connection = _connectionProvider.GetConnection(providerType));
        }
    }
}
