using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace DataEx
{
    public class LocalContextTransactionResolver : ITransactionResolver
    {


        private readonly IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;

        public LocalContextTransactionResolver(string connectionString)
            : this(connectionString, Database.DefaultProvider)
        {
        }

        public LocalContextTransactionResolver(string connectionString, DatabaseProviderType providerType)
            : this(GetConnection(connectionString, providerType))
        {
            ProviderType = providerType;
        }

        public LocalContextTransactionResolver(DbConnection connection, IsolationLevel isolationLevel)
        {
            _isolationLevel = isolationLevel;
            Connection = connection;
        }

        public LocalContextTransactionResolver(DbConnection connection)
            : this(connection, IsolationLevel.ReadCommitted)
        {
        }

        public DbTransaction GetTransactionOrNull()
        {
            if (Transaction == null) Transaction = Connection.BeginTransaction(_isolationLevel);
            return Transaction;
        }

        private static DbConnection GetConnection(string connectionString, DatabaseProviderType providerType)
        {
            DbProviderFactory factory;
            factory = providerType == DatabaseProviderType.None ? Database.GetDefaultFactory() : Database.GetFactoryFromProvider(providerType);
            var conn = factory.CreateConnection();
            conn.ConnectionString = connectionString;
            return conn;
        }

        public DbConnection GetConnectionOrNull()
        {
            return Connection;
        }

        public DbConnection Connection { get; private set; }
        public DbTransaction Transaction { get; private set; }
        public DatabaseProviderType ProviderType { get; private set; }


    }
}
