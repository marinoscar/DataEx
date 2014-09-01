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
    public class User : IUser<string>
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
        public string PrimaryEmail { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string CountryCode { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public DateTime Birthday { get; set; }
        public string PasswordHash { get; set; }
    }
}
