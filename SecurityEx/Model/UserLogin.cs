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
    public class UserLogin : AuditModelBasic
    {
        [Key, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// The user id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Gets the provider i.e. Google, Facebook, Twitter, Application
        /// </summary>
        public string Provider { get; set; }
        /// <summary>
        /// Gets the provider type i.e Application, External
        /// </summary>
        public string ProviderType { get; set; }


        public UserLoginInfo ToUserLoginInfo()
        {
            return new UserLoginInfo(Provider, UserId);
        }
    }
}
