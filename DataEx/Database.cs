using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UtilEx;

namespace DataEx
{
    public enum DatabaseProviderType { None, SqlServer, MySql, Postgresql, Oracle, Db2, SqLite }

    public class Database
    {

        #region Variable Declaration

        public const string ExtendedFieldSeparator = "___";
        public const string ExtendedFieldPrefix = "extended" + ExtendedFieldSeparator;
        private readonly string _userName;
        private readonly string _serverName;
        private readonly string _databaseName;
        private readonly string _connectionString;
        private DbProviderFactory _providerFactory;
        private static Dictionary<string, Dictionary<string, PropertyInfo>> _propertyInfos;
        private static Dictionary<string, List<string>> _typePrimaryFieldNames; 


        #endregion

        #region Constructors

        public Database()
            : this(GetDefaultConnectionString())
        {
            
        }

        public Database(string connectionString) : this(connectionString, DatabaseProviderType.None, null, new NullTransactionResolver()) { }

        public Database(string connectionString, DatabaseProviderType providerType) : this(connectionString, providerType, null, new NullTransactionResolver()) { }

        public Database(string connectionString, string appNameIfNotSpecified) : this(connectionString, DatabaseProviderType.None, appNameIfNotSpecified, new NullTransactionResolver()) { }

        public Database(string connectionString, string defaultAppName, ITransactionResolver transactionResolver) : this(connectionString, DatabaseProviderType.None, defaultAppName, transactionResolver) { }

        public Database(LocalContextTransactionResolver transactionResolver)
            : this(
                transactionResolver.Connection.ConnectionString, transactionResolver.ProviderType, "",
                transactionResolver)
        {
        }

        public Database(string connectionString, DatabaseProviderType providerType, string defaultAppName, ITransactionResolver transactionResolver)
        {
            if (providerType == DatabaseProviderType.None && DefaultProvider == DatabaseProviderType.None)
                providerType = DatabaseProviderType.SqlServer;
            if (providerType == DatabaseProviderType.None && DefaultProvider != DatabaseProviderType.None)
                providerType = DefaultProvider;
            ProviderType = providerType;

            var connString = new DbConnectionStringBuilder { ConnectionString = connectionString };

            _userName = GetUserIdFromConnStringObject(connString);
            _serverName = GetServerFromConnStringObject(connString);
            _databaseName = GetDatabaseFromConnStringObject(connString);
            _connectionString = connectionString;

            CommandTimeoutInSeconds = GetCommandTimeoutFromConfiguration();
            TransactionResolver = transactionResolver;
        }

        #endregion

        #region Static Methods

        private static string GetDefaultConnectionString()
        {
            var defaultConnStringName = ConfigurationManager.AppSettings["defaultConnStringName"];
            if (string.IsNullOrWhiteSpace(defaultConnStringName) && ConfigurationManager.ConnectionStrings.Count > 0)
                return ConfigurationManager.ConnectionStrings[0].ConnectionString;
            var connStringObj = ConfigurationManager.ConnectionStrings[defaultConnStringName];
            if(connStringObj == null)
                throw new ArgumentException("No connection string specified");
            return connStringObj.ConnectionString;
        }

        private static PropertyInfo GetPropertyInfoCache(Type type, string name)
        {
            var typeName = type.FullName;
            if (!PropertyInfos.ContainsKey(typeName))
                PropertyInfos[typeName] = new Dictionary<string, PropertyInfo>();
            if (PropertyInfos[typeName].ContainsKey(name))
                return PropertyInfos[typeName][name];
            var property = type.GetProperty(name);
            if (property == null) return null;
            PropertyInfos[typeName][name] = property;
            return property;
        }

        public static void SetDefaultDatabaseProviderType(DatabaseProviderType type)
        {
            _dbProvider = type;
        }

        private static DatabaseProviderType _dbProvider;

        public static DatabaseProviderType DefaultProvider
        {
            get
            {
                if (_dbProvider == DatabaseProviderType.None)
                    _dbProvider = DatabaseProviderType.MySql;
                return _dbProvider;
            }
            set { _dbProvider = value; }
        }

        private static string GetUserIdFromConnStringObject(DbConnectionStringBuilder connString)
        {
            if (connString.ContainsKey("User Id")) return Convert.ToString(connString["User Id"]);
            if (connString.ContainsKey("Uid")) return Convert.ToString(connString["Uid"]);
            return string.Empty;
        }

        private static string GetServerFromConnStringObject(DbConnectionStringBuilder connString)
        {
            if (connString.ContainsKey("Server")) return Convert.ToString(connString["Server"]);
            if (connString.ContainsKey("Data Source")) return Convert.ToString(connString["Data Source"]);
            if (connString.ContainsKey("Host")) return Convert.ToString(connString["Host"]);
            return string.Empty;
        }

        public static string GetDatabaseFromConnStringObject(DbConnectionStringBuilder connString)
        {
            if (connString.ContainsKey("Database")) return Convert.ToString(connString["Database"]);
            if (connString.ContainsKey("Initial Catalog")) return Convert.ToString(connString["Initial Catalog"]);
            if (connString.ContainsKey("Host")) return Convert.ToString(connString["Host"]);
            return string.Empty;
        }

        public static DbProviderFactory GetDefaultFactory()
        {
            return DbProviderFactories.GetFactory(GetDatabaseProviderName(DefaultProvider));
        }

        public static DbProviderFactory GetFactoryFromProvider(DatabaseProviderType provider)
        {
            return DbProviderFactories.GetFactory(GetDatabaseProviderName(provider));
        }

        private static string GetDatabaseProviderName(DatabaseProviderType providerType)
        {
            var result = "System.Data.SqlClient";
            switch (providerType)
            {
                case DatabaseProviderType.MySql:
                    result = "MySql.Data.MySqlClient";
                    break;
                case DatabaseProviderType.Postgresql:
                    result = "Npgsql";
                    break;
            }
            return result;
        }

        public static DatabaseProviderType GetProviderTypeFromName(string providerName)
        {
            var result = DatabaseProviderType.SqlServer;
            switch (providerName.ToLowerInvariant())
            {
                case "mysql.data.mysqlclient":
                    result = DatabaseProviderType.MySql;
                    break;
                case "Npgsql":
                    result = DatabaseProviderType.Postgresql;
                    break;
            }
            return result;
        }

        #endregion

        #region Property Implementation

        private static Dictionary<string, Dictionary<string, PropertyInfo>> PropertyInfos
        {
            get { return _propertyInfos ?? (_propertyInfos = new Dictionary<string, Dictionary<string, PropertyInfo>>()); }
        }

        public ITransactionResolver TransactionResolver { get; set; }

        public DatabaseProviderType ProviderType { get; private set; }

        public string Name
        {
            get { return _databaseName; }
        }

        public string ServerName
        {
            get { return _serverName; }
        }

        public string UserName
        {
            get { return _userName; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public int CommandTimeoutInSeconds { get; set; }

        private static Dictionary<string, List<string>> TypePrimaryFieldNames
        {
            get { return _typePrimaryFieldNames ?? (_typePrimaryFieldNames = new Dictionary<string, List<string>>()); }
        }

        #endregion

        #region Methods

        private static List<string> GetTypePrimaryFieldNames(Type type)
        {
            var typename= type.FullName;
            if (!TypePrimaryFieldNames.ContainsKey(typename))
            {
                TypePrimaryFieldNames[typename] = new List<string>();
                return new List<string>();
            }
            return TypePrimaryFieldNames[typename];
        } 

        private static int GetCommandTimeoutFromConfiguration()
        {
            var item = ConfigurationManager.AppSettings["databaseCommandTimeout"];
            if (string.IsNullOrWhiteSpace(item)) return 60;
            return Convert.ToInt32(item);
        }

        public T ExecuteScalar<T>(string query)
        {
            return (T)WithCommand(query, command =>
            {
                var result = command.ExecuteScalar();
                if (Convert.DBNull == result)
                {
                    throw new InvalidCastException(("Attempt to execute sql query '{0}' and obtain a scalar of type '{1}' "
                        + "returned an unexpected NULL from the database.").Fi(query, typeof(T).FullName));
                }

                if (null == result)
                {
                    throw new InvalidCastException(("Attempt to execute sql query '{0}' and obtain a scalar of type '{1}' "
                        + "returned zero rows.").Fi(query, typeof(T).FullName));
                }

                result = Convert.ChangeType(result, typeof(T));
                return result;
            });
        }

        public T TryExecuteScalar<T>(string query, out bool gotData)
        {
            var closureGotData = false;
            var result = (T)WithCommand(query, command =>
            {
                var returnedValue = command.ExecuteScalar();
                if (Convert.IsDBNull(returnedValue) || null == returnedValue)
                {
                    return default(T);
                }

                closureGotData = true;
                return returnedValue;
            });

            gotData = closureGotData;
            return result;
        }

        public T ExecuteScalarOr<T>(string query, T defaultOnFailure)
        {
            bool success;
            var result = TryExecuteScalar<T>(query, out success);
            return success ? result : defaultOnFailure;
        }

        public void WhileReading(string query, Action<DbDataReader> doSomething)
        {
            WithDataReader(query, CommandBehavior.Default, r =>
            {
                while (r.Read())
                {
                    doSomething(r);
                }

                return null;
            });
        }

        public object WithDataReader(string query, Func<DbDataReader, object> doSomething)
        {
            return WithDataReader(query, CommandBehavior.Default, doSomething);
        }

        public object WithDataReader(string query, CommandBehavior behavior, Func<DbDataReader, object> doSomething)
        {
            return WithCommand(query, command =>
            {
                using (var r = command.ExecuteReader(behavior))
                {
                    return doSomething(r);
                }
            });
        }

        public List<T> ExecuteToList<T>(string query)
        {
            var type = typeof(T);
            var list = new List<T>();
            IEnumerable<string> names = null;
            WhileReading(query, r =>
                {
                    if (names == null)
                        names = r.GetNames();
                    list.Add((T)FromDataRecord(type, r.ToDictionary()));
                });
            return list;
        }

        private object FromDataRecord(Type type, Dictionary<string, object> d)
        {
            var item = Activator.CreateInstance(type);
            var fields = d.Keys;
            var primary = GetTypePrimaryFieldNames(type);
            var extended = GetExtendedPropertyNames(fields);
            if (primary.Count <= 0)
            {
                primary = (from f in fields where !f.StartsWith(ExtendedFieldPrefix) select f).ToList();
                TypePrimaryFieldNames[type.FullName] = primary;
            }
            foreach (var field in primary)
            {
                var value = d[field];
                var propInfo = GetPropertyInfoCache(type, field);
                if (propInfo == null) continue;

                if (DBNull.Value.Equals(value)) value = null;
                else value = Convert.ChangeType(value, propInfo.PropertyType);
                propInfo.SetValue(item, value);
            }
            foreach (var extendedProperty in extended)
            {
                var prop = GetPropertyInfoCache(type, extendedProperty);
                var colPrefix = "{0}{1}{2}{3}{4}".Fi(ExtendedFieldPrefix, extendedProperty, ExtendedFieldSeparator, prop.PropertyType.Name, ExtendedFieldSeparator);
                var newDic = d.Where(i => i.Key.StartsWith(colPrefix)).ToDictionary(i => i.Key.Replace(colPrefix, ""), v => v.Value);
                var value = FromDataRecord(prop.PropertyType, newDic);
                prop.SetValue(item, value);
            }
            return item;
        }

        private IEnumerable<string> GetExtendedPropertyNames(IEnumerable<string> names)
        {
            return names.Where(i => i.StartsWith(ExtendedFieldPrefix)).Select(GetPropertyNameFromExtendedColumnName).Distinct().ToList();
        }

        private string GetPropertyNameFromExtendedColumnName(string name)
        {
            var cleanName = name.Replace(ExtendedFieldPrefix, "");
            var parts = cleanName.Split(ExtendedFieldSeparator.ToCharArray());
            return parts[0];
        }

        public List<Dictionary<string, object>> ExecuteToDictionaryList(string query)
        {
            var list = new List<Dictionary<string, object>>();
            WhileReading(query, r => list.Add(r.ToDictionary()));
            return list;
        }

        public object WithConnection(Func<DbConnection, object> doSomething)
        {
            var ambientConnection = TransactionResolver.GetConnectionOrNull();
            if (ambientConnection != null)
            {
                if (ambientConnection.State == ConnectionState.Closed) ambientConnection.Open();
                return doSomething(ambientConnection);
            }

            using (var conn = OpenConnection())
            {
                return doSomething(conn);
            }
        }

        public object WithCommand(string sqlStatement, Func<DbCommand, object> doSomething)
        {
            return WithConnection(conn =>
            {

                var cmd = conn.CreateCommand();
                cmd.CommandText = sqlStatement;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                cmd.Transaction = TransactionResolver.GetTransactionOrNull();
                cmd.CommandTimeout = CommandTimeoutInSeconds;
                try
                {
                    return doSomething(cmd);
                }
                catch (Exception ex)
                {
                    throw new DataException("Error running statement:\n{0}\n{1}\n\n with user {2}".Fi(sqlStatement, ex.Message, _userName), ex);
                }
            });
        }

        public int ExecuteNonQuery(string sqlStatement)
        {
            return (int)WithCommand(sqlStatement, command => command.ExecuteNonQuery());
        }

        public void TestConnection()
        {
            WithConnection(conn => conn);
        }


        private DbConnection OpenConnection()
        {
            var conn = GetConnection(ConnectionString);
            conn.Open();
            return conn;
        }


        private DbProviderFactory GetFactory()
        {
            return GetFactory(ProviderType);
        }

        private DbProviderFactory GetFactory(DatabaseProviderType provider)
        {
            if (_providerFactory == null) _providerFactory = DbProviderFactories.GetFactory(GetDatabaseProviderName(provider));
            return _providerFactory;
        }

        public DbConnection GetConnection()
        {
            return GetConnection(ProviderType);
        }

        public DbConnection GetConnection(DatabaseProviderType providerType)
        {
            var conn = GetFactoryFromProvider(providerType).CreateConnection();
            conn.ConnectionString = ConnectionString;
            return conn;
        }

        private DbConnection GetConnection(string connString)
        {
            var conn = GetFactory().CreateConnection();
            conn.ConnectionString = connString;
            return conn;
        }




        #endregion

        #region CRUD

        public void Insert<T>(T item)
        {
            var queryProvider = QueryProvider.GetQueryProvider(item.GetType());
            ExecuteNonQuery(queryProvider.GetInsertStatement(item));
        }

        public void Update<T>(T item)
        {
            var queryProvider = QueryProvider.GetQueryProvider(item.GetType());
            ExecuteNonQuery(queryProvider.GetUpdateStatement(item));
        }

        public void Delete<T>(T item)
        {
            var queryProvider = QueryProvider.GetQueryProvider(item.GetType());
            ExecuteNonQuery(queryProvider.GetDeleteStatement(item));
        }

        public IEnumerable<T> Select<T>()
        {
            var queryProvider = QueryProvider.GetQueryProvider<T>();
            return ExecuteToList<T>(queryProvider.GetSelectStatement());
        }

        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> expression, bool lazyLoading = true)
        {
            var queryProvider = QueryProvider.GetQueryProvider<T>();
            return ExecuteToList<T>(queryProvider.GetSelectStatement(expression, lazyLoading));
        }

        #endregion

    }
}
