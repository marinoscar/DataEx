using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataEx;
using Microsoft.AspNet.Identity;
using SecurityEx.Model;
using UtilEx;

namespace SecurityEx
{
    public class UserStoreProvider : StoreProviderBase, IUserPasswordStore<User, string>, IUserLoginStore<User, string>, IUserClaimStore<User, string>, IRoleStore<Role, string>, IUserRoleStore<User, string>
    {
        #region Constructors

        public UserStoreProvider()
            : base(new Database())
        {

        }

        public UserStoreProvider(string connString)
            : base(new Database(connString))
        {

        }

        public UserStoreProvider(Database database)
            : base(database)
        {
        }

        #endregion

        #region Method Implementation

        #region User Manager Methods

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

        #region Password Manager Methods

        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            return new Task(() => SetPasswordHash(user, passwordHash));
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
            var dbUser = FindById(user.Id);
            if (dbUser == null) throw new ArgumentException("Invalid user information");
            return dbUser.PasswordHash;
        }

        public Task<bool> HasPasswordAsync(User user)
        {
            return new Task<bool>(() => HasPassword(user));
        }

        public bool HasPassword(User user)
        {
            var dbUser = FindById(user.Id);
            return !string.IsNullOrWhiteSpace(dbUser.PasswordHash);
        }


        #endregion

        #region Password Manager

        public Task SetPasswordAsync(User user, string password)
        {
            return new Task(() => SetPassword(user, password));
        }

        public void SetPassword(User user, string password)
        {
            var provider = new PasswordProvider();
            var passwordData = provider.CreatePassword(password);
            user.PasswordHash = passwordData.PasswordHash;
            user.PasswordSalt = passwordData.Salt;
            user.TemporaryPasswordHash = passwordData.PasswordHash;
            user.TemporaryPasswordSalt = passwordData.Salt;
            Database.Update(user);
        } 

        #endregion

        #region Login Manager Methods

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

        public Task AddLoginAsync(User user, UserLoginInfo login)
        {
            return new Task(() => AddLogin(user, login));
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

        #endregion

        #region Claim Manager Methods

        private static UserClaim ConvertClaim(IUser<string> user, Claim claim)
        {
            return new UserClaim()
                {
                    UserId = user.Id,
                    Type = claim.Type,
                    Properties = JsonSerializer.ToJson(claim.Properties),
                    Provider = claim.Issuer,
                    OriginalProvider = claim.OriginalIssuer,
                    ValueType = claim.ValueType,
                    Value = claim.Value
                };
        }

        public Task<IList<Claim>> GetClaimsAsync(User user)
        {
            return new Task<IList<Claim>>(() => GetClaims(user));
        }

        public IList<Claim> GetClaims(User user)
        {
            return Database.Select<UserClaim>(i => i.UserId == user.Id).Select(i => i.ToClaim()).ToList();
        }

        public Task AddClaimAsync(User user, Claim claim)
        {
            return new Task(() => AddClaim(user, claim));
        }

        public void AddClaim(User user, Claim claim)
        {
            AddClaim(ConvertClaim(user, claim));
        }

        public void AddClaim(UserClaim userClaim)
        {
            Database.Insert(userClaim);
        }

        public Task RemoveClaimAsync(User user, Claim claim)
        {
            return new Task(() => RemoveClaimAsync(user, claim));
        }

        public void RemoveClaim(User user, Claim claim)
        {
            RemoveClaim(ConvertClaim(user, claim));
        }

        public void RemoveClaim(UserClaim claim)
        {
            var sql = "DELETE FROM UserClaim WHERE UserId = {0} Type = {1} And Provider = {2}".FormatSql(claim.UserId,
                                                                                                         claim.Type,
                                                                                                         claim.Provider);
            Database.ExecuteNonQuery(sql);
        }

        #endregion

        #region Role Manager

        public Task CreateAsync(Role role)
        {
            return new Task(() => Create(role));
        }

        public void Create(Role role)
        {
            Database.Insert(role);
        }

        public Task UpdateAsync(Role role)
        {
            return new Task(() => Update(role));
        }

        public void Update(Role role)
        {
            Database.Update(role);
        }

        public Task DeleteAsync(Role role)
        {
            return new Task(() => Delete(role));
        }

        public void Delete(Role role)
        {
            Database.Delete(role);
        }

        Task<Role> IRoleStore<Role, string>.FindByIdAsync(string roleId)
        {
            return new Task<Role>(() => FindRoleById(roleId));
        }

        Role FindRoleById(string roleId)
        {
            return Database.Select<Role>(i => i.Id == roleId).SingleOrDefault();
        }

        Task<Role> IRoleStore<Role, string>.FindByNameAsync(string roleName)
        {
            return new Task<Role>(() => FindRoleByName(roleName));
        }

        Role FindRoleByName(string roleName)
        {
            return Database.Select<Role>(i => i.Name == roleName).SingleOrDefault();
        }

        #endregion

        #region User Role Manager Methods

        public Task AddToRoleAsync(User user, string roleName)
        {
            return new Task(() => AddToRole(user, roleName));
        }

        public void AddToRole(User user, string roleName)
        {
            var role = FindRoleByName(roleName);
            AddToRole(new UserRole()
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
        }

        public void AddToRole(UserRole userRole)
        {
            Database.Insert(userRole);
        }

        public Task RemoveFromRoleAsync(User user, string roleName)
        {
            return new Task(() => RemoveFromRole(user, roleName));
        }

        public void RemoveFromRole(User user, string roleName)
        {
            var role = FindRoleByName(roleName);
            RemoveFromRole(new UserRole()
            {
                UserId = user.Id,
                RoleId = role.Id
            });
        }

        public void RemoveFromRole(UserRole userRole)
        {
            Database.Delete(userRole);
        }

        public Task<IList<string>> GetRolesAsync(User user)
        {
            return new Task<IList<string>>(() => GetRoles(user));
        }

        public IList<string> GetRoles(User user)
        {
            var sql = "SELECT Name FROM UserRole WHERE UserId = {0}".FormatSql(user.Id);
            return Database.ExecuteToList<string>(sql);
        }

        public Task<bool> IsInRoleAsync(User user, string roleName)
        {
            return new Task<bool>(() => IsInRole(user,roleName));
        }

        public bool IsInRole(User user, string roleName)
        {
            var sql = @"SELECT COUNT(UserRole.*) 
                        FROM UserRole
                        INNER JOIN Role ON UserRole.RoleId = Role.Id 
                        WHERE UserRole.UserId = {0} And Role.Name = {1}".FormatSql(user.Id, roleName);
            return Database.ExecuteScalar<int>(sql) > 0;
        }

        #endregion

        #endregion
    }
}
