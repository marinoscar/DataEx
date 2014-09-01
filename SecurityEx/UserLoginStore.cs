using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataEx;
using Microsoft.AspNet.Identity;
using SecurityEx.Model;
using UtilEx;

namespace SecurityEx
{
    public class UserLoginStore : IUserLoginStore<User, string>
    {

        #region Variables

        private UserStore _userStore;

        #endregion

        #region Constructors

        public UserLoginStore()
            : this(new UserStore())
        {

        }

        public UserLoginStore(string connString)
            : this(new UserStore(connString))
        {

        }

        public UserLoginStore(Database database)
            : this(new UserStore(database))
        {

        }

        public UserLoginStore(UserStore userStore)
        {
            _userStore = userStore;
            Database = _userStore.Database;
        }

        #endregion

        internal Database Database { get; private set; }

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

        public Task AddLoginAsync(User user, UserLoginInfo login)
        {
            return new Task(() => AddLogin(user, login));
        }

        private static UserLogin GetLogin(User user, UserLoginInfo login)
        {
            return new UserLogin()
            {
                Provider = login.LoginProvider,
                ProviderType = "External",
                UserId = user.Id
            };
        }

        public void AddLogin(User user, UserLoginInfo login)
        {
            Database.Insert(GetLogin(user, login));
        }

        public Task RemoveLoginAsync(User user, UserLoginInfo login)
        {
            return new Task(() => RemoveLogin(user, login));
        }

        public void RemoveLogin(User user, UserLoginInfo login)
        {
            var userLogin = GetLogin(user, login);
            Database.ExecuteNonQuery(
                "DELETE FROM UserLogin WHERE UserId = {0} And Provider = {1}".FormatSql(userLogin.UserId, userLogin.Provider));
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            return new Task<IList<UserLoginInfo>>(() => GetLogins(user));
        }

        public IList<UserLoginInfo> GetLogins(User user)
        {
            return Database.Select<UserLogin>(i => i.UserId == user.Id).Select(i => i.ToUserLoginInfo()).ToList();
        } 

        public Task<User> FindAsync(UserLoginInfo login)
        {
            return new Task<User>(() => Find(login));
        }

        public User Find(UserLoginInfo login)
        {
            return Database.Select<User>(i => i.Id == login.ProviderKey).SingleOrDefault();
        }
    }
}
