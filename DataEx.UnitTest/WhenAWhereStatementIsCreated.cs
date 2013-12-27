using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DataEx.UnitTest
{
    [TestFixture]
    public class WhenAWhereStatementIsCreated
    {
        [Test]
        public void ItShouldProcessAEqualityStatement()
        {
            var queryProvider = new MySqlEntityQueryProvider<DataModel>();
            var result = queryProvider.GetWhereStatement(i => i.Id == 1);
            Assert.AreEqual("(Id = 1)", result);
        }

        [Test]
        public void ItShouldProcessAEqualityStatement1()
        {
            var queryProvider = new MySqlEntityQueryProvider<DataModel>();
            var result = queryProvider.GetWhereStatement(i => 1 == i.Id);
            Assert.AreEqual("(1 = Id)", result);
        }

        [Test]
        public void ItShouldResolveAndOperator()
        {
            var queryProvider = new MySqlEntityQueryProvider<DataModel>();
            var result = queryProvider.GetWhereStatement(i => i.LastName == "Marin" && i.Balance > 0d);
            Assert.AreEqual("((LastName = 'Marin') And (Balance > 0))", result);
        }

        [Test]
        public void ItShouldResolveOrOperator()
        {
            var queryProvider = new MySqlEntityQueryProvider<DataModel>();
            var result = queryProvider.GetWhereStatement(i => i.LastName == "Marin" || i.Balance > 0d);
            Assert.AreEqual("((LastName = 'Marin') Or (Balance > 0))", result);
        }
    }
}
