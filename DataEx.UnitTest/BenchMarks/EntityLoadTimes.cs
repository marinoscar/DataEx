using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UtilEx;

namespace DataEx.UnitTest.BenchMarks
{
    [TestFixture]
    public class EntityLoadTimes
    {
        private const int TotalLoads = 50000;
        private Dictionary<string, object> _values = new Dictionary<string, object>()
            {
                {"Id", 1}, {"Code", "ABC-0001"}, {"Name", "Oscar"}, {"LastName", "Marin"},
                {"CreatedOn", DateTime.UtcNow}, {"Balance", 150d}
            };

        [Test]
        public void WhenUsingReflection()
        {
            var sw = Stopwatch.StartNew(); 
            for (var i = 0; i < TotalLoads; i++)
            {
                var newItem = NewItem();
                AssignValues(newItem, _values);
            }
            sw.Stop();
            Debug.WriteLine("Execution time for TotalLoads {0}", sw.Elapsed);
            Debug.WriteLine("finish the execution");
        }

        [Test]
        public void WhenUsingDelegates()
        {
            var helper = new ClassHelper<DataModel>();
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < TotalLoads; i++)
            {
                var newItem = helper.FromDictionary(_values);
            }
            sw.Stop();
            Debug.WriteLine("Execution time for TotalLoads {0}", sw.Elapsed);
            Debug.WriteLine("finish the execution");
        }

        public DataModel NewItem()
        {
            return Activator.CreateInstance<DataModel>();
        }

        public void AssignValues(DataModel target, Dictionary<string, object> values)
        {
            foreach (var key in values.Keys)
            {
                var prop = typeof(DataModel).GetProperty(key);
                prop.SetValue(target, values[prop.Name]);
            }
        }
    }
}
