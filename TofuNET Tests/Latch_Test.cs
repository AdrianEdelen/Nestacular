using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TofuNET_Tests
{
    [TestClass]
    internal class Latch_Test
    {

        [TestMethod]
        public void SRLatchTest()
        {
            var latch = new SRLatch();
            Assert.IsTrue( new Func<bool>(() =>
            {
                bool q;
                bool notq;
                latch.R = false;
                latch.S = false;
                q = latch.Q;
                notq = latch.NotQ;
                return true; /* dummy, still building the test */
            })());


        }

    }
}
