using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx.UnitTest
{
    public class DataModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedOn { get; set; }
        public double Balance { get; set; }
    }
}
