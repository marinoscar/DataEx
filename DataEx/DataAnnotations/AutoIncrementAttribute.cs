using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AutoIncrementAttribute : Attribute
    {
    }
}
