using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace AStarTests
{
    [TestClass]
    public class GenericPlaceGraphTests
    {
        private GenericPlaceGraph sut = new();

        //[TestInitialize]
        //public void Initialize() {
        //    sut = new();
        //}

        [TestMethod]
        public void TestBuild_SucceedsForGoodGraph()
        {
            sut.BuildFromFile("../../../Resources/mermaid_networks/net1.mmd");

            Assert.Contains("G", sut.Places.Keys);
            Assert.DoesNotContain("H", sut.Places.Keys);
            Assert.IsTrue(TestHelpers.AllEqual(sut.CostToLeave("A", "B"), sut.CostToLeave("B", "A"), 6.0));
        }

        [TestMethod]
        public void TestBuild_ExceptionOnBadFileType()
        {
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.BuildFromFile("../../../Resources/mermaid_networks/netwithnegative.txt"),
                "GenericPlaceGraph only supports building from .mmd (Mermaid) files");
        }
        
        [TestMethod]
        public void TestBuild_ExceptionOnNegativeCost()
        {
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.BuildFromFile("../../../Resources/mermaid_networks/netwithnegative.mmd"),
                "Cannot have a negative cost: -2 for (C, D)");
        }

        [TestMethod]
        public void TestBuild_ExceptionOnDuplicatePairs()
        {
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.BuildFromFile("../../../Resources/mermaid_networks/netwithduplicate.mmd"),
                "Cannot specify the same pair of places more than once: (A, B)");
        }

        [TestMethod]
        public void TestBuild_ExceptionOnDisjointGraph()
        {
            TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
                () => sut.BuildFromFile("../../../Resources/mermaid_networks/netdisjoint.mmd"),
                "Cannot support a disjoint Graph!");
        }

        ///*              E -3- F -3- G 
        //*               |           |
        //*               2           2
        //*               |           |
        //*         A -2- B -5- C -2- D 
        //*/
        //[TestMethod]
        //public void TestBuildFromTables() {

        //    List<(string, string, double)> neighbourPairs = new List<(string, string, double)> {
        //        ("A", "B", 2.0),
        //        ("B", "C", 5.0),
        //        ("C", "D", 2.0),
        //        ("D", "G", 2.0),
        //        ("G", "F", 3.0),
        //        ("F", "E", 3.0),
        //        ("E", "B", 2.0)
        //    };

        //    GenericPlaceGraph sut = new();
        //    Assert.IsTrue(sut.BuildFromTables(neighbourPairs));
        //    Assert.AreEqual(sut.Places["A"].GetCostToLeave(sut.Places["B"]), 2.0);
        //}
    }
}