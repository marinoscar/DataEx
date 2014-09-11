using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public class EmptyDbLogger : IDbLogger
    {
        public void Log(string message)
        {}

        public bool IsEnabled
        {
            get { return false; }
        }
    }

    public class DebugDbLogger : IDbLogger
    {
        public DebugDbLogger()
        {
            IsEnabled = true;
        }

        public void Log(string message)
        {
            Debug.WriteLine(message);
        }

        public bool IsEnabled { get; set; }
    }
}
