using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{

    public static class SqlDialectProviderFactory
    {
        public static ISqlDialectProvider GetProvider(DatabaseProviderType providerType)
        {
            switch (providerType)
            {
                case DatabaseProviderType.SqlServer:
                    return new SqlServerDialectProvider();
                case DatabaseProviderType.MySql:
                    return new MySqlDialectProvider();
                case DatabaseProviderType.Postgresql:
                    return new AnsiSqlDialectProvider();
                default:
                    return new AnsiSqlDialectProvider();
            }
        }
    }

    public class AnsiSqlDialectProvider : ISqlDialectProvider
    {
        public string SystemNameStartCharacter { get { return string.Empty; } }
        public string SystemNameEndCharacter { get { return SystemNameStartCharacter; } }
    }

    public class MySqlDialectProvider : ISqlDialectProvider
    {
        public string SystemNameStartCharacter { get { return "`"; } }
        public string SystemNameEndCharacter { get { return SystemNameStartCharacter; } }
    }

    public class SqlServerDialectProvider : ISqlDialectProvider
    {
        public string SystemNameStartCharacter { get { return "["; } }
        public string SystemNameEndCharacter { get { return "]"; } }
    }
}
