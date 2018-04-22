using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PersistentLayer.Tests
{
    [TestClass]
    public class ClausewitzParserTests
    {
        [TestMethod]
        public void ParseTest()
        {
            const string expected = "block1begin\n" +
                                    "name1 =val1\n" +
                                    "block2begin\n" +
                                    "name2 =val2\n" +
                                    "BLOCKend\n" +
                                    "name3 =val3\n" +
                                    "BLOCKend\n";

            const string expected3 = "block1begin\n" +
                                    "name1 =val1\n" +
                                    "block2begin\n" +
                                    " val2 " + " val " + "BLOCKend\n" +
                                    "name3 =val3\n" +
                                    "block3begin\n" +
                                    "BLOCKend\n" +
                                    "BLOCKend\n";

            string res = "";
            ClausewitzParser parser =
                new ClausewitzParser(block => { res += block + "begin\n"; }, () => { res += "BLOCKend\n"; }, (name, val) => { res += name + " =" + val + "\n"; }, val => { res += " " + val + " "; });
            parser.Parse("TestData\\ClausewitzTest1.txt");
            Assert.AreEqual(expected, res);

            res = "";
            parser.Parse("TestData\\ClausewitzTest2.txt");
            Assert.AreEqual(expected, res);

            res = "";
            parser.Parse("TestData\\ClausewitzTest3.txt");
            Assert.AreEqual(expected3, res);
        }
    }
}