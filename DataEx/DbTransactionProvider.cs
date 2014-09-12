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
        private IDbTransaction _transaction;

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
            if (_connection == null)
                _connection = GetConnection(_providerType);
            if(_connection.State == ConnectionState.Closed)
                _connection.Open();
            if (_transaction == null)
                _transaction = _connection.BeginTransaction(isolationLevel);
            return _transaction;
        }

        public IDbConnection GetConnection(DatabaseProviderType providerType)
        {
            _connectionProvider.ConnectionString = ConnectionString;
            return _connectionProvider.GetConnection(providerType);
        }

        public string ConnectionString { get; set; }

        public void Dispose()
        {
            _connection = null;
            _transaction = null;
        }

    }
}
