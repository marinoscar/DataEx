using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public interface IDbTransactionProvider : IDbConnectionProvider, IDisposable
    {
        IDbTransaction BeginTransaction();
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel);
        bool ProvideTransaction { get; }
    }
}
