using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using AStarNickNS;
using System.Collections.Generic;
using System.Linq;

namespace AStarTests
{

    [TestClass]
    public class GenericPlaceTests
    {
        GenericPlace gpA = new GenericPlace("A", new HashSet<IPlace<string>> { });
        GenericPlace gpB = new GenericPlace("B", new HashSet<IPlace<string>> { });
        GenericPlace gpC = new GenericPlace("C", new HashSet<IPlace<string>> { });

        [TestMethod]
        public void TestGenericPlace_IsNeighbour()
        {
            // A and B are neighbours, C is disjoint
            gpA.Neighbours.Add(gpB);
            gpB.Neighbours.Add(gpA);

            Assert.IsTrue(gpA.IsNeighbour(gpB));
            Assert.IsTrue(gpB.IsNeighbour(gpA));
            Assert.IsFalse(gpC.IsNeighbour(gpA));
            Assert.IsFalse(gpC.IsNeighbour(gpB));
            Assert.IsFalse(gpA.IsNeighbour(gpC));
            Assert.IsFalse(gpB.IsNeighbour(gpC));

            Assert.AreEqual(0, gpC.Neighbours.Count);
            Assert.AreEqual(1, gpB.Neighbours.Count);

            // Check you cannot add a neighbour multiple times to the Neighbours list (HashSet)
            gpA.Neighbours.Add(gpB);
            gpA.Neighbours.Add(gpB);
            gpA.Neighbours.Add(gpB);
            Assert.AreEqual(1, gpA.Neighbours.Count);

            // Check you can remove a neighbour in the HashSet
            gpA.Neighbours.Remove(gpB);
            Assert.AreEqual(0, gpA.Neighbours.Count);

            // Check you cannot remove a non-existent neighbour
            gpC.Neighbours.Remove(gpB);
            Assert.AreEqual(0, gpC.Neighbours.Count);
        }

        [TestMethod]
        public void TestGenericPlace_ToString()
        {
            // Check ToString() method returns the label
            Assert.AreEqual(gpA.Label, "A");
            Assert.AreEqual(gpA.Label, gpA.ToString());
        }


    }

    [TestClass]
    public class GridCoords2DTests
    {
        //Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => account.Debit(debitAmount));
        GridCoords2D gridCoords2DBase = new GridCoords2D(-3, 5);
        GridCoords2D gridCoords2DDiag = new GridCoords2D(-4, 6);
        GridCoords2D gridCoords2DStra = new GridCoords2D(-3, 6);
        GridCoords2D gridCoords2DDistant = new GridCoords2D(-60, 60);

        [TestMethod]
        public void TestGridCoords2D_GetLabel()
        {
            Assert.IsTrue(gridCoords2DBase.Label.Equals((-3, 5)));
        }

        [TestMethod]
        public void TestGridCoords2D_IsDiagonalNeighbour()
        {
            Assert.AreEqual(true, gridCoords2DBase.IsDiagonalNeighbour(gridCoords2DDiag));
            Assert.AreEqual(false, gridCoords2DBase.IsDiagonalNeighbour(gridCoords2DStra));
            Assert.AreEqual(true, gridCoords2DDiag.IsDiagonalNeighbour(gridCoords2DBase));
            Assert.AreEqual(false, gridCoords2DStra.IsDiagonalNeighbour(gridCoords2DBase));

            Assert.AreEqual(false, gridCoords2DBase.IsDiagonalNeighbour(gridCoords2DDistant));
        }

        [TestMethod]
        public void TestGridCoords2D_IsStraightNeighbour()
        {
            Assert.AreEqual(false, gridCoords2DBase.IsStraightNeighbour(gridCoords2DDiag));
            Assert.AreEqual(true, gridCoords2DBase.IsStraightNeighbour(gridCoords2DStra));
            Assert.AreEqual(false, gridCoords2DDiag.IsStraightNeighbour(gridCoords2DBase));
            Assert.AreEqual(true, gridCoords2DStra.IsStraightNeighbour(gridCoords2DBase));
            
            Assert.AreEqual(false, gridCoords2DBase.IsStraightNeighbour(gridCoords2DDistant));
        }

        [TestMethod]
        public void TestGridCoords2D_IsNeighbour()
        {
            Assert.AreEqual(true, gridCoords2DBase.IsNeighbour(gridCoords2DDiag));
            Assert.AreEqual(true, gridCoords2DBase.IsNeighbour(gridCoords2DStra));
            Assert.AreEqual(true, gridCoords2DDiag.IsNeighbour(gridCoords2DBase));
            Assert.AreEqual(true, gridCoords2DStra.IsNeighbour(gridCoords2DBase));
            
            Assert.AreEqual(false, gridCoords2DBase.IsNeighbour(gridCoords2DDistant));
        }

        [TestMethod]
        public void TestGridCoords2D_DeltaFrom()
        {
            int[] expectedDelta = new int[2] { (-60)-(-3), 60-5 };
            Assert.IsTrue(gridCoords2DDistant.DeltaFrom(gridCoords2DBase).SequenceEqual(expectedDelta));
        }

        [TestMethod]
        public void TestGridCoords2D_ToString() {
            Assert.AreEqual("(-3, 5)", gridCoords2DBase.ToString());
        }
    }

    [TestClass]
    public class NodeTests
    {
        static GenericPlace gpA = new GenericPlace("A", new HashSet<IPlace<string>> { });
        static GenericPlace gpB = new GenericPlace("B", new HashSet<IPlace<string>> { });
        static GenericPlace gpC = new GenericPlace("C", new HashSet<IPlace<string>> { });

        Node<GenericPlace> nodeA = new Node<GenericPlace>(gpA);
        Node<GenericPlace> nodeB = new Node<GenericPlace>(gpB);
        Node<GenericPlace> nodeC = new Node<GenericPlace>(gpC);

        public void SetupNodes()
        {

            Dictionary<Node<GenericPlace>, double> costDictA = new Dictionary<Node<GenericPlace>, double> { { nodeB, 4.0 } };
            foreach (Node<GenericPlace> node in costDictA.Keys)
            {
                nodeA.Coord.Neighbours.Add(node.Coord);
            }

            Dictionary<Node<GenericPlace>, double> costDictB = new Dictionary<Node<GenericPlace>, double> { { nodeA, 4.0 } };
            foreach (Node<GenericPlace> node in costDictB.Keys)
            {
                nodeB.Coord.Neighbours.Add(node.Coord);
            }

        }

        [TestMethod]
        public void TestNode_GetLabel()
        {
            SetupNodes();
            Console.WriteLine(gpA.Neighbours);
            int a = 2;
            //Assert.IsTrue(gridCoords2DBase.GetLabel().SequenceEqual(new int[2] { -3, 5 }));
        }


    }
}
