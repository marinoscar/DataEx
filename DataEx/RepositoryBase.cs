using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public abstract class RepositoryBase : IUnitOfWork
    {
        protected RepositoryBase(IDataContext context)
        {
            Context = context;
        }

        public IDataContext Context { get; private set; }

        public virtual int SaveChanges()
        {
            return Context.SaveChanges();
        }
    }
}
