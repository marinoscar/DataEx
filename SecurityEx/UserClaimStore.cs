using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataEx;
using Microsoft.AspNet.Identity;
using SecurityEx.Model;

namespace SecurityEx
{
    public class UserClaimStore : IUserClaimStore<User, string>
    {
        #region Variables

        private UserStore _userStore;

        #endregion

        #region Constructors

        public UserClaimStore()
            : this(new UserStore())
        {

        }

        public UserClaimStore(string connString)
            : this(new UserStore(connString))
        {

        }

        public UserClaimStore(Database database)
            : this(new UserStore(database))
        {

        }

        public UserClaimStore(UserStore userStore)
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

        public Task<IList<Claim>> GetClaimsAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task AddClaimAsync(User user, Claim claim)
        {
            
            throw new NotImplementedException();
        }

        public Task RemoveClaimAsync(User user, Claim claim)
        {
            throw new NotImplementedException();
        }
    }
}
