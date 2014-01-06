using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RelationAttribute : Attribute
    {
        public RelationAttribute(string foreignKey, string primaryKey)
        {
            ForeignKey = foreignKey;
            PrimaryKey = primaryKey;
        }

        public RelationAttribute(string foreignKey)
            : this(foreignKey, "Id")
        {
        }

        public string ForeignKey { get; private set; }
        public string PrimaryKey { get; private set; }
    }
}
