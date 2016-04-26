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
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void CreationNotConfigured()
        {
            ICore core = Core.Instance;
        }

        [TestMethod]
        public void CreationConfigured()
        {
            ICoreConfigurator conf = Core.Configurator;
            conf.RootFolder = @"D:\Games\HOI\mod-CORE";

            ICore core = Core.Instance;
            core.Test();
        }
    }
}
