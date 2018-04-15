using Microsoft.VisualStudio.TestTools.UnitTesting;

using PersistentLayer;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PersistentLayerTests
{
    public class TraceCounter : TraceListener
    {
        public override void Write(string message)
        {
            _lines += message.Aggregate(0, (acc, chr) => { if (chr == '\n') acc++; return acc; });
        }

        public override void WriteLine(string message)
        {
            _lines += message.Aggregate(0, (acc, chr) => { if (chr == '\n') acc++; return acc; });
            _lines++;
        }

        public int Lines { get => _lines; }
        public void Reset() { _lines = 0; }

        private int _lines = 0;
    }

    [TestClass]
    public class CoreTests
    {
        [TestMethod]
//        [ExpectedException(typeof(System.InvalidOperationException))]
        public void CreationNotConfigured()
        {
            IUnitTypes models = Core.UnitTypes;
            Assert.IsNotNull(models);
        }

        [TestMethod]
        public void CreationConfigured()
        {
            Core.RootFolder = @"D:\Games\HOI\mod-CORE";

            IUnitTypes models = Core.UnitTypes;
            Assert.IsNotNull(models);
        }

        [TestMethod]
        public void TextDataParsed()
        {
            int lines = File.ReadLines(@"D:\Games\HOI\mod-CORE\config\text.csv").Count(s => { return s.StartsWith("#") || s.Contains(";;;;;;;;;;;") ? false : true; });
            TraceCounter tc = new TraceCounter();

            string root = @"D:\Games\HOI\mod-CORE";
            PrivateObject prCore = new PrivateObject(typeof(Core));

            Trace.Listeners.Add(tc);
            prCore.Invoke("ParseTextData", root);

            int textCount = ((Dictionary<string, string>) prCore.GetField("_textData")).Count;
            Assert.AreEqual(lines, textCount + tc.Lines, "Text count ({0}) is wrong (should be {1} - {2} = {3})", textCount, lines, tc.Lines, lines - tc.Lines);
            Assert.AreEqual(tc.Lines, 0);
        }

        [TestMethod]
        public void ModelNamesParsed()
        {
            int lines = File.ReadLines(@"D:\Games\HOI\mod-CORE\config\models.csv").Count();
            TraceCounter tc = new TraceCounter();

            string root = @"D:\Games\HOI\mod-CORE";
            PrivateObject prCore = new PrivateObject(typeof(Core));

            Trace.Listeners.Add(tc);
            prCore.Invoke("ParseModelNames", root);

            int modelsCount = 0;
            PrivateObject types = new PrivateObject(prCore.GetField("_unitTypes"));
            foreach (var e1 in (Dictionary<UnitTypeName, UnitType>) types.GetField("_unitTypes"))
            {
                PrivateObject models = new PrivateObject(e1.Value);
                foreach (var e2 in (Dictionary<int, Model>) models.GetField("_models"))
                {
                    PrivateObject model = new PrivateObject(e2.Value);
                    modelsCount += ((Dictionary<string, string>) model.GetField("_namesByCountry")).Count;
                }
            }

            Assert.AreEqual(lines - 1, modelsCount + tc.Lines, "Model count ({0}) is wrong (should be {1} - {2} = {3})", modelsCount, lines - 2, tc.Lines, lines - 2 - tc.Lines);
        }

        [TestMethod]
        public void CountriesParsed()
        {
            int lines = File.ReadLines(@"D:\Games\HOI\mod-CORE\config\models.csv").Count();
            TraceCounter tc = new TraceCounter();

            string root = @"D:\Games\HOI\mod-CORE";
            PrivateObject prCore = new PrivateObject(typeof(Core));
            prCore.Invoke("ParseTextData", root);

            Trace.Listeners.Add(tc);
            prCore.Invoke("ParseCountries", root);

            var countries = (Dictionary<string, string>) prCore.GetField("_countries");
            int countryTags = ((Dictionary<string, string>)prCore.GetField("_countryTags")).Count;
            Assert.AreEqual(countries.Count, countryTags, "Country count ({0}) isn't equal to tags count ({1})", countries.Count, countryTags);

            prCore.Invoke("ParseModelNames", root);
            PrivateObject types = new PrivateObject(prCore.GetField("_unitTypes"));
            foreach (var e1 in (Dictionary<UnitTypeName, UnitType>)types.GetField("_unitTypes"))
            {
                PrivateObject models = new PrivateObject(e1.Value);
                foreach (var e2 in (Dictionary<int, Model>)models.GetField("_models"))
                {
                    PrivateObject model = new PrivateObject(e2.Value);
                    foreach (string countryTag in ((Dictionary<string, string>)model.GetField("_namesByCountry")).Keys)
                        if (!countries.ContainsKey(countryTag)) Assert.Fail("Model for absent country found ({0})", countryTag);
                }
            }
        }
    }
}
