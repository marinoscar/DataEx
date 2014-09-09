using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataEx.DataAnnotations;

namespace SecurityEx.Model
{
    public class UserRole : AuditModel
    {
        [Key, AutoIncrement]
        public int Id { get; set; }
        public string UserId { get; set; }
        [Relation("UserId", "Id")]
        public User User { get; set; }
        public string RoleId { get; set; }
        [Relation("RoleId", "Id")]
        public Role Role { get; set; }
    }
}
