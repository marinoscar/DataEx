using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilEx;

namespace DataEx
{
    public static class SqlLanguageProviderFactory
    {
        public static ISqlLanguageProvider GetProvider(DatabaseProviderType providerType)
        {
            switch (providerType)
            {
                case DatabaseProviderType.None:
                    return new AnsiSqlLanguageProvider(new SqlExpressionProvider(),new SqlServerDialectProvider(), new FastReflectionObjectAccessor());
                case DatabaseProviderType.SqlServer:
                    return new AnsiSqlLanguageProvider(new SqlExpressionProvider(), new SqlServerDialectProvider(), new FastReflectionObjectAccessor());
                case DatabaseProviderType.MySql:
                    return new MySqlLanguageProvider(new SqlExpressionProvider(), new FastReflectionObjectAccessor());
                default:
                    return new AnsiSqlLanguageProvider(new SqlExpressionProvider(), new AnsiSqlDialectProvider(), new FastReflectionObjectAccessor());
            }
        }
    }
}
