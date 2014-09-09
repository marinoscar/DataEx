using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataEx.DataAnnotations;
using Microsoft.AspNet.Identity;

namespace SecurityEx.Model
{
    public class User : AuditModelBasic, IUser<string> 
    {

        #region Constructors

        public User() : this(Guid.NewGuid().ToString())
        {
        } 

        public User(string id)
        {
            Id = id;
        } 

        #endregion

        [Key]
        public string Id { get; private set; }
        public string UserName { get; set; }
        public string LoweredUserName { get; set; }
        public string PrimaryEmail { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string CountryCode { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string TimeZone { get; set; }
        public DateTime Birthday { get; set; }
        public string JsonSettings { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string TemporaryPasswordHash { get; set; }
        public string TemporaryPasswordSalt { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public bool RequirePasswordChange { get; set; }
        public int FailedPasswordAttemptCount { get; set; }
        public DateTime UtcFailedPasswordAnswerAttemptWindowStart { get; set; }
        public DateTime UtcLastLoginDate { get; set; }
        public DateTime UtcLastLockedOutDate { get; set; }
        public DateTime UtcLastFailedAttempt { get; set; }

    }
}
