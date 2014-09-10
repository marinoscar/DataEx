using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public interface IDbConnectionProvider
    {
        IDbConnection GetConnection(DatabaseProviderType providerType);
    }
}
