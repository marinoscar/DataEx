using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataEx;

namespace SecurityEx
{
    public class StoreProviderBase : IDisposable
    {
        #region Property Implementation
        
        protected Database Database { get; private set; } 

        #endregion

        #region Constructors

        public StoreProviderBase()
            : this(new Database())
        {

        }

        public StoreProviderBase(string connString)
            : this(new Database(connString))
        {

        }

        public StoreProviderBase(Database database)
        {
            Database = database;
        } 

        #endregion

        public void Dispose()
        {
            Database = null;
        }
    }
}
