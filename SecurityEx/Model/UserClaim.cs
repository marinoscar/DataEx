using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataEx.DataAnnotations;

namespace SecurityEx.Model
{
    public class UserClaim : AuditModelBasic
    {
        [Key, AutoIncrement]
        public int Id { get; set; }

        public string UserId { get; set; }
        public int ProviderId { get; set; }
        public string OriginalIssuer { get; set; }
        public string Properties { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
    }
}
