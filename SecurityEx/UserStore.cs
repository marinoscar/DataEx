using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataEx;
using Microsoft.AspNet.Identity;
using SecurityEx.Model;

namespace SecurityEx
{
    public class UserStore : IUserStore<User, string>
    {

        #region Variable Declaration

        #endregion

        #region Constructors

        public UserStore()
            : this(new Database())
        {

        }

        public UserStore(string connString)
            : this(new Database(connString))
        {
        }

        public UserStore(Database db)
        {
            Database = db;
        }

        #endregion

        #region Property Implementation

        internal Database Database { get; private set; } 

        #endregion

        #region Method Implemenation

        public void Dispose()
        {
            Database = null;
        }

        public Task CreateAsync(User user)
        {
            return new Task(() => Create(user));
        }

        public void Create(User user)
        {
            Database.Insert(user);
        }

        public Task UpdateAsync(User user)
        {
            return new Task(() => Update(user));
        }

        public void Update(User user)
        {
            Database.Update(user);
        }

        public Task DeleteAsync(User user)
        {
            return new Task(() => Delete(user));
        }

        public void Delete(User user)
        {
            Database.Delete(user);
        }

        public Task<User> FindByIdAsync(string userId)
        {
            return new Task<User>(() => FindById(userId));
        }

        public User FindById(string userId)
        {
            return Database.Select<User>(i => i.Id == userId).SingleOrDefault();
        }

        public Task<User> FindByNameAsync(string userName)
        {
            return new Task<User>(() => FindByName(userName));
        }

        public User FindByName(string userName)
        {
            return Database.Select<User>(i => i.UserName == userName).SingleOrDefault();
        } 

        #endregion

    }
}
