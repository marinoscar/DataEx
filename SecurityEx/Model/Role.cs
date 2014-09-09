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
    public class Role : AuditModel, IRole<string>
    {
        public Role()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
