using System;
using System.Collections.Generic;
using System.Linq;
using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;

namespace AStarTests.Graphs
{
    [TestClass]
    public class GridCellIntersectionsTest
    {
        [TestMethod]
        public void TestGetCellIntersectionsWithLineSegment_Horizontal()
        {
            var start = (0, 0);
            var end = (3, 0);
            var expectedCells = new List<(int, int)> { (0, 0), (1, 0), (2, 0), (3, 0) };
            var expectedIntersectonDistances = new List<float> { 0.5f, 1.0f, 1.0f, 0.5f };
            
            var result = GridCellIntersections.GetCellIntersectionsWithLineSegment(start, end);
            var actualCells = result.Select(r => (r.x, r.y)).ToList();
            var actualIntersectionDistances = result.Select(r => r.IntersectedDistance).ToList();

            CollectionAssert.AreEqual(expectedCells, actualCells);
            TestHelpers.AssertSequencesAreEqualWithinTolerance(expectedIntersectonDistances, actualIntersectionDistances, 0.0001f);
        }

        [TestMethod]
        public void TestGetCellIntersectionsWithLineSegment_Vertical()
        {
            var start = (0, 0);
            var end = (0, 3);
            var expectedCells = new List<(int, int)> { (0, 0), (0, 1), (0, 2), (0, 3) };
            var expectedIntersectonDistances = new List<float> { 0.5f, 1.0f, 1.0f, 0.5f };
            
            var result = GridCellIntersections.GetCellIntersectionsWithLineSegment(start, end);
            var actualCells = result.Select(r => (r.x, r.y)).ToList();
            var actualIntersectionDistances = result.Select(r => r.IntersectedDistance).ToList();

            CollectionAssert.AreEqual(expectedCells, actualCells);
            TestHelpers.AssertSequencesAreEqualWithinTolerance(expectedIntersectonDistances, actualIntersectionDistances, 0.0001f);
        }

        [TestMethod]
        public void TestGetCellIntersectionsWithLineSegment_Diagonal()
        {
            var start = (0, 0);
            var end = (3, 3);
            var expectedCells = new List<(int, int)> { (0, 0), (0, 1), (1, 0), (1, 1), (1, 2), (2, 1), (2, 2), (2, 3), (3, 2), (3, 3) };
            var expectedIntersectonDistances = new List<float> { 
                MathF.Sqrt(2.0f)/2, 0.0f, 0.0f,
                MathF.Sqrt(2.0f), 0.0f, 0.0f, 
                MathF.Sqrt(2.0f), 0.0f, 0.0f,
                MathF.Sqrt(2.0f)/2 };
            
            var result = GridCellIntersections.GetCellIntersectionsWithLineSegment(start, end);
            var actualCells = result.Select(r => (r.x, r.y)).ToList();
            var actualIntersectionDistances = result.Select(r => r.IntersectedDistance).ToList();

            CollectionAssert.AreEqual(expectedCells, actualCells);
            TestHelpers.AssertSequencesAreEqualWithinTolerance(expectedIntersectonDistances, actualIntersectionDistances, 0.0001f);
        }

        [TestMethod]
        public void TestGetCellIntersectionsWithLineSegment_SameCell()
        {
            var start = (0, 0);
            var end = (0, 0);
            var expectedCells = new List<(int, int)> { (0, 0) };
            var expectedIntersectonDistances = new List<float> { 0.0f };
            
            var result = GridCellIntersections.GetCellIntersectionsWithLineSegment(start, end);
            var actualCells = result.Select(r => (r.x, r.y)).ToList();
            var actualIntersectionDistances = result.Select(r => r.IntersectedDistance).ToList();
            
            CollectionAssert.AreEqual(expectedCells, actualCells);
            TestHelpers.AssertSequencesAreEqualWithinTolerance(expectedIntersectonDistances, actualIntersectionDistances, 0.0001f);
        }

        [TestMethod]
        public void TestGetCellIntersectionsWithLineSegment_Complex()
        {
            var start = (0, 0);
            var end = (2, 1);
            var result = GridCellIntersections.GetCellIntersectionsWithLineSegment(start, end);
            
            var expectedCells = new List<(int, int)> { (0, 0), (1, 0), (1, 1), (2, 1) };
            var actualCells = result.Select(r => (r.x, r.y)).ToList();

            CollectionAssert.AreEqual(expectedCells, actualCells);
            Assert.AreEqual(MathF.Sqrt(2*2+1*1), result.Sum(r => r.IntersectedDistance), 0.001f);
        }
    }
}