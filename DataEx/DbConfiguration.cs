using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public static class DbConfiguration
    {

        #region Variable Declaration
        
        private static DatabaseProviderType _providerType;
        private static int _commandTimeout;
        private static string _connecitonString;

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
    }
}
