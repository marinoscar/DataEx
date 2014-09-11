using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public interface IDbLogger
    {
        void Log(string message);
        bool IsEnabled { get; }
    }
}
