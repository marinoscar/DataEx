using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UtilEx;

namespace DataEx
{
    public enum DatabaseProviderType { None, SqlServer, MySql, Postgresql, Oracle, Db2, SqLite }

    public class Database
    {
        #region Variable Declaration

        private readonly string _userName;
        private readonly string _serverName;
        private readonly string _databaseName;
        private readonly string _connectionString;
        private static Dictionary<string, List<string>> _typePrimaryFieldNames;
        private readonly IObjectAccesor _objectAccesor;
        private ISqlLanguageProvider _languageProvider;

        #endregion

        #region Constructors

        public Database()
            : this(DbConfiguration.DefaultConnectionString)
        {

        }

        public Database(string connectionString) : this(connectionString, DatabaseProviderType.None, DbConfiguration.Get<IDbTransactionProvider>(), DbConfiguration.Get<IObjectAccesor>(), DbConfiguration.Get<ISqlLanguageProvider>()) { }

        public Database(string connectionString, DatabaseProviderType providerType) : this(connectionString, providerType, DbConfiguration.Get<IDbTransactionProvider>(), DbConfiguration.Get<IObjectAccesor>(), DbConfiguration.Get<ISqlLanguageProvider>()) { }

        public Database(string connectionString, IDbTransactionProvider transactionProvider) : this(connectionString, DatabaseProviderType.None, transactionProvider, DbConfiguration.Get<IObjectAccesor>(), DbConfiguration.Get<ISqlLanguageProvider>()) { }

        public Database(string connectionString, DatabaseProviderType providerType, IDbTransactionProvider transactionProvider)
            : this(connectionString, providerType, transactionProvider, DbConfiguration.Get<IObjectAccesor>(), DbConfiguration.Get<ISqlLanguageProvider>())
        {
        }

        public Database(string connectionString, DatabaseProviderType providerType, IDbTransactionProvider transactionProvider, IObjectAccesor objectAccesor, ISqlLanguageProvider sqlLanguageProvider)
        {
            if (providerType == DatabaseProviderType.None)
                providerType = DbConfiguration.DefaultProviderType;

            ProviderType = providerType;

            var connString = new DbConnectionStringBuilder { ConnectionString = connectionString };

            _userName = GetUserIdFromConnStringObject(connString);
            _serverName = GetServerFromConnStringObject(connString);
            _databaseName = GetDatabaseFromConnStringObject(connString);
            _connectionString = connectionString;

            CommandTimeoutInSeconds = DbConfiguration.DatabaseCommandTimeout;
            TransactionProvider = transactionProvider;
            _objectAccesor = objectAccesor;
            _languageProvider = sqlLanguageProvider;
        }

        #endregion

        #region Static Methods

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

        private static string GetDatabaseFromConnStringObject(DbConnectionStringBuilder connString)
        {
            if (connString.ContainsKey("Database")) return Convert.ToString(connString["Database"]);
            if (connString.ContainsKey("Initial Catalog")) return Convert.ToString(connString["Initial Catalog"]);
            if (connString.ContainsKey("Host")) return Convert.ToString(connString["Host"]);
            return string.Empty;
        }

        #endregion

        #region Property Implementation

        public IDbTransactionProvider TransactionProvider { get; set; }
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

        #region Public Methods

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

        public void WhileReading(string query, Action<IDataReader> doSomething)
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

        public object WithDataReader(string query, Func<IDataReader, object> doSomething)
        {
            return WithDataReader(query, CommandBehavior.Default, doSomething);
        }

        public object WithDataReader(string query, CommandBehavior behavior, Func<IDataReader, object> doSomething)
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
                if (CanModelLoadFromDictionary(type))
                    list.Add((T)LoadEntityFromDictionary(type, r.ToDictionary()));
                else
                    list.Add((T)LoadEntityFromDataRecord(type, r.ToDictionary()));
            });
            return list;
        }

        public List<Dictionary<string, object>> ExecuteToDictionaryList(string query)
        {
            var list = new List<Dictionary<string, object>>();
            WhileReading(query, r => list.Add(r.ToDictionary()));
            return list;
        }

        public object WithConnection(Func<IDbConnection, object> doSomething)
        {
            if (TransactionProvider.ProvideTransaction)
            {
                return doSomething(OpenConnection(TransactionProvider.GetConnection(ProviderType)));
            }

            using (var conn = OpenConnection())
            {
                return doSomething(conn);
            }
        }

        public object WithCommand(string sqlStatement, Func<IDbCommand, object> doSomething)
        {
            return WithConnection(conn =>
            {

                var cmd = conn.CreateCommand();
                cmd.CommandText = sqlStatement;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                cmd.Transaction = TransactionProvider.BeginTransaction();
                cmd.CommandTimeout = CommandTimeoutInSeconds;
                try
                {
                    return doSomething(cmd);
                }
                catch (Exception ex)
                {
                    if (cmd.Transaction != null)
                        cmd.Transaction.Rollback();
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
            OpenConnection();
        }

        #endregion

        #region Private Methods

        private static List<string> GetTypePrimaryFieldNames(Type type)
        {
            var typename = type.FullName;
            if (!TypePrimaryFieldNames.ContainsKey(typename))
            {
                TypePrimaryFieldNames[typename] = new List<string>();
                return new List<string>();
            }
            return TypePrimaryFieldNames[typename];
        }

        private static bool CanModelLoadFromDictionary(Type modelType)
        {
            var cache = ObjectCacheProvider.GetProvider<Type, bool>("canLoadFromDictionary");
            return cache.GetCacheItem(modelType, t => typeof(IDictionaryLoader).IsAssignableFrom(modelType));
        }

        private object LoadEntityFromDictionary(Type type, Dictionary<string, object> d)
        {
            if (d == null) throw new ArgumentNullException("d");
            var item = (IDictionaryLoader)_objectAccesor.Create(type);
            item.LoadFromDictionary(d);
            return item;
        }

        private object LoadEntityFromDataRecord(Type type, Dictionary<string, object> d)
        {
            return LoadEntityFromDataRecord(type, d, _objectAccesor.Create(type));
        }

        private object LoadEntityFromDataRecord(Type type, Dictionary<string, object> d, object item)
        {
            var fields = d.Keys;
            var primary = GetTypePrimaryFieldNames(type);
            var extended = GetExtendedPropertyNames(fields);
            if (primary.Count <= 0)
            {
                primary = (from f in fields where !f.StartsWith(DbConfiguration.ExtendedFieldPrefix) select f).ToList();
                TypePrimaryFieldNames[type.FullName] = primary;
            }
            foreach (var field in primary)
            {
                var value = d[field];
                if (DBNull.Value.Equals(value)) value = null;
                _objectAccesor.TrySetPropertyValue(item, field, value);
            }
            foreach (var extendedProperty in extended)
            {
                var colPrefix = "{0}{1}{2}{3}{4}".Fi(DbConfiguration.ExtendedFieldPrefix, extendedProperty, DbConfiguration.ExtendedFieldSeparator, extendedProperty, DbConfiguration.ExtendedFieldSeparator);
                var newDic = d.Where(i => i.Key.StartsWith(colPrefix)).ToDictionary(i => i.Key.Replace(colPrefix, ""), v => v.Value);
                var propertyItem = _objectAccesor.TryGetPropertyValue<object>(item, extendedProperty);
                if (propertyItem == null)
                {
                    //If the property is not initialized on the model constructor then we need
                    //get the property info to create the object
                    var propertyInfo = GetPropertyInfo(item, extendedProperty);
                    propertyItem = _objectAccesor.Create(propertyInfo.PropertyType);
                }
                var value = LoadEntityFromDataRecord(propertyItem.GetType(), newDic, propertyItem);
                _objectAccesor.SetPropertyValue(item, extendedProperty, value);
            }
            return item;
        }

        private static PropertyInfo GetPropertyInfo(object target, string propertyName)
        {
            var provider =
                ObjectCacheProvider.GetProvider<Tuple<Type, string>, PropertyInfo>("ExtendedQueryPropertiesInfo");
            var key = new Tuple<Type, string>(target.GetType(), propertyName);
            return provider.GetCacheItem(key, i => i.Item1.GetProperty(i.Item2));
        }

        private static IEnumerable<string> GetExtendedPropertyNames(IEnumerable<string> names)
        {
            return names.Where(i => i.StartsWith(DbConfiguration.ExtendedFieldPrefix)).Select(GetPropertyNameFromExtendedColumnName).Distinct().ToList();
        }

        private static string GetPropertyNameFromExtendedColumnName(string name)
        {
            var cleanName = name.Replace(DbConfiguration.ExtendedFieldPrefix, "");
            var parts = cleanName.Split(DbConfiguration.ExtendedFieldSeparator.ToCharArray());
            return parts[0];
        }


        private IDbConnection OpenConnection()
        {
            return OpenConnection(TransactionProvider.GetConnection(ProviderType));
        }

        private IDbConnection OpenConnection(IDbConnection conn)
        {
            try
            {
                conn.ConnectionString = ConnectionString;
                if(conn.State == ConnectionState.Closed)
                    conn.Open();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to establish a connection with {0} for user {1}".Fi(ServerName, UserName), ex);
            }
            return conn;
        }

        #endregion

        #region CRUD

        public void Insert<T>(T item)
        {
            ExecuteNonQuery(_languageProvider.Insert(item));
        }

        public void Update<T>(T item)
        {
            ExecuteNonQuery(_languageProvider.Update(item));
        }

        public void Delete<T>(T item)
        {
            ExecuteNonQuery(_languageProvider.Delete(item));
        }

        public void Upsert<T>(IEnumerable<T> items)
        {
            ExecuteNonQuery(_languageProvider.Upsert(items));
        }

        #region Select Methods

        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> expression)
        {
            return ExecuteToList<T>(_languageProvider.Select(expression, null, false, 0, 0, false));
        }

        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderBy, bool orderByDescending)
        {
            return ExecuteToList<T>(_languageProvider.Select(expression, orderBy, orderByDescending, 0, 0, false));
        }

        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderBy, bool orderByDescending, bool lazyLoading)
        {
            return ExecuteToList<T>(_languageProvider.Select(expression, orderBy, orderByDescending, 0, 0, lazyLoading));
        }

        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderBy, bool orderByDescending, uint take, bool lazyLoading)
        {
            return ExecuteToList<T>(_languageProvider.Select(expression, orderBy, orderByDescending, 0, take, lazyLoading));
        }

        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderBy, bool orderByDescending, uint skip, uint take, bool lazyLoading)
        {
            return ExecuteToList<T>(_languageProvider.Select(expression, orderBy, orderByDescending, skip, take, lazyLoading));
        } 

        #endregion

        #endregion
    }
}
