//using AStarNickNS;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Linq;

//namespace AStarTests {

//    [TestClass]
//    public class GridPlaceTests {
//        //Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => account.Debit(debitAmount));
//        private static GridPlaceGraph _gridPlaceGraph1 = new();
//        private static GridPlaceGraph _gridPlaceGraph2 = new();
//        readonly GridPlace gridPlaceBase1 = new((-3, 5), _gridPlaceGraph1);
//        readonly GridPlace gridPlaceDiag1 = new((-4, 6), _gridPlaceGraph1);
//        readonly GridPlace gridPlaceStra1 = new((-3, 6), _gridPlaceGraph1);
//        readonly GridPlace gridPlaceDistant1 = new((-60, 60), _gridPlaceGraph1);

//        readonly GridPlace gridPlaceBase2 = new((-3, 5), _gridPlaceGraph2);
//        readonly GridPlace gridPlaceDiag2 = new((-4, 6), _gridPlaceGraph2);
//        readonly GridPlace gridPlaceStra2 = new((-3, 6), _gridPlaceGraph2);


//        [TestMethod]
//        public void TestIsNeighbour() {
//            // Explicit neighbours (not automatically bidirectional)
//            gridPlaceDistant1.ExplicitNeighboursWithCosts.Add(gridPlaceBase1, 1.0);
//            Assert.IsTrue(gridPlaceDistant1.IsNeighbour(gridPlaceBase1));
//            Assert.IsFalse(gridPlaceBase1.IsNeighbour(gridPlaceDistant1));
//            gridPlaceBase1.ExplicitNeighboursWithCosts.Add(gridPlaceDistant1, 1.0);
//            Assert.IsTrue(gridPlaceBase1.IsNeighbour(gridPlaceDistant1));

//            // Grid neighbours (remove the above explicit neighbours first)
//            gridPlaceBase1.ExplicitNeighboursWithCosts.Remove(gridPlaceDistant1);
//            gridPlaceDistant1.ExplicitNeighboursWithCosts.Remove(gridPlaceBase1);
//            Assert.IsFalse(gridPlaceBase1.IsNeighbour(gridPlaceDistant1));

//            Assert.IsTrue(gridPlaceBase1.IsNeighbour(gridPlaceDiag1));
//            Assert.IsTrue(gridPlaceBase1.IsNeighbour(gridPlaceStra1));
//            Assert.IsTrue(gridPlaceDiag1.IsNeighbour(gridPlaceBase1));
//            Assert.IsTrue(gridPlaceStra1.IsNeighbour(gridPlaceBase1));

//            // Grid neighbours on different graphs are not neighbours
//            Assert.IsFalse(gridPlaceBase1.IsNeighbour(gridPlaceDiag2));
//            Assert.IsFalse(gridPlaceBase1.IsNeighbour(gridPlaceStra2));
//            Assert.IsFalse(gridPlaceDiag1.IsNeighbour(gridPlaceBase2));
//            Assert.IsFalse(gridPlaceStra1.IsNeighbour(gridPlaceBase2));
//        }

//        [TestMethod]
//        public void TestDeltaFrom() {
//            int[] expectedDelta = new int[2] { (-60) - (-3), 60 - 5 };
//            Assert.IsTrue(gridPlaceDistant1.DeltaFrom(gridPlaceBase1).SequenceEqual(expectedDelta));
//        }

//        [TestMethod]
//        public void TestToString() {
//            Assert.AreEqual("(-3, 5)", gridPlaceBase1.ToString());
//        }
//    }
//}
