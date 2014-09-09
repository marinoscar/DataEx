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
    public class EntityCreationTimes
    {
        private const int ExecutionTimes = 500;

        [Test]
        public void WhenUsingActivator()
        {
            var result = new List<DataModel>(ExecutionTimes);
            var sw = Stopwatch.StartNew();
            ExecutionTimes.Times((i) => result.Add(Activator.CreateInstance<DataModel>()));
            sw.Stop();
            Debug.WriteLine("Duration {0}", sw.Elapsed);
        }

        [Test]
        public void WhenUsingEntityCreator()
        {
            var result = new List<DataModel>(ExecutionTimes);
            var sw = Stopwatch.StartNew();
            ExecutionTimes.Times((i) => result.Add(EntityCreator.Create<DataModel>()));
            sw.Stop();
            Debug.WriteLine("Duration {0}", sw.Elapsed);
        }
    }
}
