using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilEx;

namespace DataEx
{
    public static class DbConfiguration
    {

        #region Variable Declaration
        
        private static DatabaseProviderType _providerType;
        private static int _commandTimeout;
        private static string _connecitonString;
        private static Dictionary<DatabaseProviderType, Action> _initalizers;
        private static bool _isInitialized;

        public const string ExtendedFieldSeparator = "___";
        public const string ExtendedFieldPrefix = "extended" + ExtendedFieldSeparator;

        #endregion

        #region Property Implementation
        
        public static DatabaseProviderType DefaultProviderType
        {
            get
            {
                if (_providerType == DatabaseProviderType.None)
                    _providerType = GetProviderTypeFromConfigFile();
                return _providerType;
            }
            set { _providerType = value; }
        } 

        public static int DatabaseCommandTimeout
        {
            get { return GetCommandTimeoutFromConfigFile(); }
        }

        public static string DefaultConnectionString
        {
            get { return GetDefaultConnectionStringFromConfigFile(); }
        }

        #endregion

        #region Helper Methods
        
        private static string GetDefaultConnectionStringFromConfigFile()
        {
            if (!String.IsNullOrWhiteSpace(_connecitonString)) return _connecitonString;
            var defaultConnStringName = ConfigurationManager.AppSettings["defaultConnStringName"];
            if (String.IsNullOrWhiteSpace(defaultConnStringName) && ConfigurationManager.ConnectionStrings.Count > 0)
                return ConfigurationManager.ConnectionStrings[0].ConnectionString;
            var connStringObj = ConfigurationManager.ConnectionStrings[defaultConnStringName];
            if (connStringObj == null)
                throw new ArgumentException("No connection string specified");
            _connecitonString = connStringObj.ConnectionString;
            return _connecitonString;
        }

        private static DatabaseProviderType GetProviderTypeFromConfigFile()
        {
            var providerTypeName = ConfigurationManager.AppSettings["DefaultProviderType"];
            if (String.IsNullOrWhiteSpace(providerTypeName)) return DatabaseProviderType.None;
            DatabaseProviderType providerType;
            var didItWork = Enum.TryParse(providerTypeName, true, out providerType);
            return didItWork ? providerType : DatabaseProviderType.MySql;
        } 

        private static int GetCommandTimeoutFromConfigFile()
        {
            if (_commandTimeout > 0) return _commandTimeout;
            var commandTimeOut = ConfigurationManager.AppSettings["DatabaseCommandTimeout"];
            _commandTimeout = String.IsNullOrWhiteSpace(commandTimeOut) ? 15 : Convert.ToInt32(commandTimeOut);
            return _commandTimeout;
        }

        #endregion

        #region Methods

        public static void RegisterInitializer(DatabaseProviderType providerType, Action initializer)
        {
            if(_initalizers == null) _initalizers = new Dictionary<DatabaseProviderType, Action>();
            _initalizers[providerType] = initializer;
        }

        public static void Initialize()
        {
            if (_initalizers == null) LoadInitializers();
            _initalizers[DefaultProviderType]();
            _isInitialized = true;
        }

        private static void Initialize(DatabaseProviderType providerType)
        {
            if (_initalizers == null) LoadInitializers();
            _initalizers[providerType]();
        }

        private static void LoadInitializers()
        {
            if (_initalizers != null) return;
            if (_initalizers == null) _initalizers = new Dictionary<DatabaseProviderType, Action>();
            RegisterInitializer(DatabaseProviderType.MySql, InitializeMySql);
            RegisterInitializer(DatabaseProviderType.SqlServer, InitializeSqlServer);
            RegisterInitializer(DatabaseProviderType.Oracle, InitializeOracle);
            RegisterInitializer(DatabaseProviderType.Postgresql, InitializePostgresql);
            RegisterInitializer(DatabaseProviderType.Db2, InitializeDb2);
        }

        private static void InitializeMySql()
        {
            DefaultProviderType = DatabaseProviderType.MySql;
            ObjectContainer.Register<IDbLogger>(new EmptyDbLogger());
            ObjectContainer.Register<IObjectAccesor>(new FastReflectionObjectAccessor());
            ObjectContainer.Register<ISqlDialectProvider>(new MySqlDialectProvider());
            ObjectContainer.Register<ISqlExpressionProvider>(new SqlExpressionProvider(ObjectContainer.Get<ISqlDialectProvider>()));
            ObjectContainer.Register<ISqlLanguageProvider>(new MySqlLanguageProvider(ObjectContainer.Get<ISqlExpressionProvider>(), ObjectContainer.Get<IObjectAccesor>()));
            ObjectContainer.Register<IDbTransactionProvider>(() => new NullDbTransactionProvider(new DbConnectionProvider()));
        }

        private static void InitializeSqlServer()
        {
            DefaultProviderType = DatabaseProviderType.MySql;
            ObjectContainer.Register<IDbLogger>(new EmptyDbLogger());
            ObjectContainer.Register<IObjectAccesor>(new FastReflectionObjectAccessor());
            ObjectContainer.Register<IDbTransactionProvider>(() => new NullDbTransactionProvider(new DbConnectionProvider()));
            ObjectContainer.Register<ISqlDialectProvider>(new SqlServerDialectProvider());
            ObjectContainer.Register<ISqlExpressionProvider>(new SqlExpressionProvider(ObjectContainer.Get<ISqlDialectProvider>()));
            ObjectContainer.Register<ISqlLanguageProvider>(new SqlServerLanguageProvider(ObjectContainer.Get<ISqlExpressionProvider>(), ObjectContainer.Get<IObjectAccesor>()));
        }

        private static void InitializeOracle()
        {
            InitializeAnsi(DatabaseProviderType.Oracle);
        }

        private static void InitializePostgresql()
        {
            InitializeAnsi(DatabaseProviderType.Postgresql);
        }

        private static void InitializeDb2()
        {
            InitializeAnsi(DatabaseProviderType.Db2);
        }

        private static void InitializeAnsi(DatabaseProviderType providerType)
        {
            DefaultProviderType = providerType;
            ObjectContainer.Register<IDbLogger>(new EmptyDbLogger());
            ObjectContainer.Register<IObjectAccesor>(new FastReflectionObjectAccessor());
            ObjectContainer.Register<IDbTransactionProvider>(() => new NullDbTransactionProvider(new DbConnectionProvider()));
            ObjectContainer.Register<ISqlDialectProvider>(new AnsiSqlDialectProvider());
            ObjectContainer.Register<ISqlExpressionProvider>(new SqlExpressionProvider(ObjectContainer.Get<ISqlDialectProvider>()));
            ObjectContainer.Register<ISqlLanguageProvider>(new AnsiSqlLanguageProvider(ObjectContainer.Get<ISqlExpressionProvider>(), ObjectContainer.Get<ISqlDialectProvider>(), ObjectContainer.Get<IObjectAccesor>()));
        }

        public static void Initialize(IObjectAccesor objectAccesor, ISqlDialectProvider dialectProvider, ISqlExpressionProvider expressionProvider, ISqlLanguageProvider languageProvider, Func<IDbTransactionProvider> transactionProvider, DatabaseProviderType providerType)
        {
            DefaultProviderType = providerType;
            ObjectContainer.Register<ISqlDialectProvider>(dialectProvider);
            ObjectContainer.Register<ISqlExpressionProvider>(expressionProvider);
            ObjectContainer.Register<ISqlLanguageProvider>(languageProvider);
            ObjectContainer.Register<IObjectAccesor>(objectAccesor);
            ObjectContainer.Register<IDbTransactionProvider>(transactionProvider);
        } 

        internal static T Get<T>()
        {
            var isRegistered = ObjectContainer.IsRegistered<T>();
            if(!isRegistered && !_isInitialized)
                Initialize();
            if (!isRegistered)
                return default(T);
            return ObjectContainer.Get<T>();
        }

        public static Database CreateDatabase()
        {
            return CreateDatabase(DefaultConnectionString);
        }

        public static Database CreateDatabase(string connectionString)
        {
            return new Database(connectionString, DefaultProviderType, 
                Get<IDbTransactionProvider>(),
                Get<IObjectAccesor>(),
                Get<ISqlLanguageProvider>());
        }

        #endregion
    }
}
