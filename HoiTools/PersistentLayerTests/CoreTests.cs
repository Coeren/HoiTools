using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PersistentLayer;
using System.Configuration;

namespace PersistentLayerTests
{
    [TestClass]
    public class CoreTests
    {
        [TestMethod]
        public void Creation()
        {
            Core core = new Core();
        }
    }
}
