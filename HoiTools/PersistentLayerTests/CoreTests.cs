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
//        [ExpectedException(typeof(System.InvalidOperationException))]
        public void CreationNotConfigured()
        {
            IModels models = Core.Models;
            Assert.IsNotNull(models);
        }

        [TestMethod]
        public void CreationConfigured()
        {
            Core.RootFolder = @"D:\Games\HOI\mod-CORE";

            IModels models = Core.Models;
            Assert.IsNotNull(models);
        }
    }
}
