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
    public class UserPasswordStore : IUserPasswordStore<User, string>
    {

        #region Variables
        
        private UserStore _userStore; 

        #endregion

        #region Constructors

        public UserPasswordStore()
            : this(new UserStore())
        {

        }

        public UserPasswordStore(string connString)
            : this(new UserStore(connString))
        {

        }

        public UserPasswordStore(Database database)
            : this(new UserStore(database))
        {

        }

        public UserPasswordStore(UserStore userStore)
        {
            _userStore = userStore;
            Database = _userStore.Database;
        } 

        #endregion

        #region Property Implementation

        internal Database Database { get; private set; }

        #endregion

        public void Dispose()
        {
            _userStore.Dispose();
            _userStore = null;
        }

        public Task CreateAsync(User user)
        {
            return _userStore.CreateAsync(user);
        }

        public Task UpdateAsync(User user)
        {
            return _userStore.UpdateAsync(user);
        }

        public Task DeleteAsync(User user)
        {
            return _userStore.DeleteAsync(user);
        }

        public Task<User> FindByIdAsync(string userId)
        {
            return _userStore.FindByIdAsync(userId);
        }

        public Task<User> FindByNameAsync(string userName)
        {
            return _userStore.FindByNameAsync(userName);
        }

        public Task SetPasswordHashAsync(User user, string passwordHash)
        {

        }

        public void SetPasswordHash(User user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            Database.Update(user);
        }

        public Task<string> GetPasswordHashAsync(User user)
        {
            return new Task<string>(() => GetPasswordHash(user));
        }

        public string GetPasswordHash(User user)
        {
            var dbUser = _userStore.FindById(user.Id);
            if(dbUser == null) throw new ArgumentException("Invalid user information");
            return dbUser.PasswordHash;
        }

        public Task<bool> HasPasswordAsync(User user)
        {
            return new Task<bool>(() => HasPassword(user));
        }

        public bool HasPassword(User user)
        {
            var dbUser = _userStore.FindById(user.Id);
            return !string.IsNullOrWhiteSpace(dbUser.PasswordHash);
        }
    }
}
