using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public interface ITransactionResolver
    {
        DbTransaction GetTransactionOrNull();
        DbConnection GetConnectionOrNull();
    }
}
