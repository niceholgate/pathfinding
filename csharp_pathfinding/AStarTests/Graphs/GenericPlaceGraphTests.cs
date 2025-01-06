using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;
using System;
using System.Collections.Generic;

namespace AStarTests {

    [TestClass]
    public class GenericPlaceGraphTests {

        private GenericPlaceGraph sut;

        [TestInitialize]
        public void Initialize() {
            sut = new();
        }

        [TestMethod]
        public void TestBuild_SucceedsForGoodGraph() {
            sut.Build("../../../Resources/mermaid_networks/net1.mmd");

            Assert.IsTrue(sut.Places.ContainsKey("G"));
            Assert.IsFalse(sut.Places.ContainsKey("H"));
            Assert.IsTrue(TestHelpers.AllEqual(sut.GetCost("A", "B"), sut.GetCost("B", "A"), 6.0));
        }

        [TestMethod]
        public void TestBuild_ExceptionOnNegativeCost() {
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.Build("../../../Resources/mermaid_networks/netwithnegative.mmd"),
                "Cannot have a negative cost: -2 for (C, D)");
        }

        [TestMethod]
        public void TestBuild_ExceptionOnDuplicatePairs() {
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.Build("../../../Resources/mermaid_networks/netwithduplicate.mmd"),
                "Cannot specify the same pair of places more than once: (A, B)");
        }

        //[TestMethod]
        //public void TestBuild_FailsDisjointGraph() {
        //    GenericPlaceGraph sut = new();
        //    sut.Build("../../../Resources/mermaid_networks/net1.mmd");

        //    Assert.IsTrue(sut.Places.ContainsKey("G"));
        //    Assert.IsFalse(sut.Places.ContainsKey("H"));
        //    Assert.IsTrue(TestHelpers.AllEqual(sut.GetCost("A", "B"), sut.GetCost("B", "A"), 6.0));
        //}

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