using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AStarTests {

    [TestClass]
    public class GenericPlaceGraphTests {

        /*              E -3- F -3- G 
        *               |           |
        *               2           2
        *               |           |
        *         A -2- B -5- C -2- D 
        */
        [TestMethod]
        public void TestBuildFromTables() {
            
            List<(string, string, double)> neighbourPairs = new List<(string, string, double)> {
                ("A", "B", 2.0),
                ("B", "C", 5.0),
                ("C", "D", 2.0),
                ("D", "G", 2.0),
                ("G", "F", 3.0),
                ("F", "E", 3.0),
                ("E", "B", 2.0)
            };

            GenericPlaceGraph sut = new();
            Assert.IsTrue(sut.BuildFromTables(neighbourPairs));
            Assert.AreEqual(sut.Places["A"].GetCostToLeave(sut.Places["B"]), 2.0);
        }

    }
}