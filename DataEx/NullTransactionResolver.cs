using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace DataEx
{
    public class NullTransactionResolver : ITransactionResolver
    {
        public DbTransaction GetTransactionOrNull()
        {
            return null;
        }

        public DbConnection GetConnectionOrNull()
        {
            return null;
        }
    }
}
