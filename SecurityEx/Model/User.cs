using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataEx.DataAnnotations;

namespace SecurityEx.Model
{
    public class User
    {
        [Key, AutoIncrement]
        public int Id { get; set; }

        public string Code { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
    }
}
