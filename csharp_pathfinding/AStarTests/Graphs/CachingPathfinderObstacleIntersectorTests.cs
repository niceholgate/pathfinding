using System;
using System.Collections.Generic;
using System.IO;
using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;
using NicUtils.ExtensionMethods;
// using NUnit.Framework;

namespace AStarTests {

[TestClass]
public class CachingPathfinderObstacleIntersectorTests
{
    private bool[,] blockages =
    { // y, x
        {false, true, true, true, false, false, false},
        {false, false, false, true, false, false, false},
        {false, false, false, true, false, false, false},
        {false, false, false, true, false, false, false},
        {false, false, false, true, false, false, false},
        {false, false, false, false, false, false, false},
        {false, false, false, false, false, false, false},
        {false, false, false, false, false, false, false},
        {false, false, false, false, false, false, false}
    };
    
    private float _sub2Sqrt2 = 2*GeometryUtils.SQRT2 - 0.01f;
    private float _sup2Sqrt2 = 2*GeometryUtils.SQRT2 + 0.01f;
    
    [TestMethod]
    public void TestCoordinateWherePathfinderDoesNotIntersectAnyObstacles_Happy()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{0.9f, 1.1f, _sub2Sqrt2, _sup2Sqrt2, 2.9f, 3.1f});

        // Inside a size 1 square                            x, y
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(0, 0, 0.9f, blockages).Occupiable());
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(0, 0, 1.1f, blockages).Occupiable());
            
        // Inside a size 3 square
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(1, 2, 2.9f, blockages).Occupiable());
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(1, 2, 3.1f, blockages).Occupiable());
            
        // Intersect with a corner
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, 0.9f, blockages).Occupiable());
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, _sub2Sqrt2, blockages).Occupiable());
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(2, 5, _sup2Sqrt2, blockages).Occupiable());
        
        // Respond to changes in the blockages
        blockages[3, 4] = false;
        sut.Invalidate(2, 5);
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, 0.9f, blockages).Occupiable());
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, _sub2Sqrt2, blockages).Occupiable());
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, _sup2Sqrt2, blockages).Occupiable());
        blockages[3, 4] = true;
        sut.Invalidate(2, 5);
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, 0.9f, blockages).Occupiable());
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, _sub2Sqrt2, blockages).Occupiable());
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(2, 5, _sup2Sqrt2, blockages).Occupiable());
    }
    
    [TestMethod]
    public void TestSmallerPathfinderSkipsComputationIfNextLargestPathfinderFitsOnAllCoords()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{1.2f, 1.9f});

        OccupiableCellCoordinates bigger =
            sut.GetOccupiableCellCoordinates(1, 2, 1.9f, blockages);
        Assert.IsTrue(bigger.Occupiable());
        Assert.IsTrue(bigger.AllCoordsOccupiable);
        
        OccupiableCellCoordinates smaller =
            sut.GetOccupiableCellCoordinates(1, 2, 1.2f, blockages);
        Assert.IsTrue(smaller.Occupiable());
        Assert.IsTrue(smaller.AllCoordsOccupiable);
    }
    
    [TestMethod]
    public void TestLargePathfinderPrefersCellCornerFarthestFromCornerObstacle()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{_sub2Sqrt2});
        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(2, 5, _sub2Sqrt2, blockages);
        Assert.IsTrue(occ.Occupiable());
        Assert.AreEqual(1, occ.CornersFarthestFromBlockages.Count);
        Assert.AreEqual((1.5f, 5.5f), occ.CornersFarthestFromBlockages[0]);
    }
    
    [TestMethod]
    public void TestNoCornersFarthestFromBlockagesAndNoNearestBlockedCornersWhenFarFromBlockages()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{0.8f});
        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(1, 7, 0.8f, blockages);
        Assert.IsTrue(occ.Occupiable());
        Assert.AreEqual(0, occ.CornersFarthestFromBlockages.Count);
        Assert.AreEqual(0, occ.NearestBlockedCorners.Count);
    }
    
    [TestMethod]
    public void TestNearestBlockedCornersFourCorners()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{0.8f});
        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(0, 8, 0.8f, blockages);
        Assert.IsTrue(occ.Occupiable());
        Assert.AreEqual(4, occ.NearestBlockedCorners.Count);
    }
    
    [TestMethod]
    public void TestNearestBlockedCornersOneCorner()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{0.8f});
        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(2, 5, 0.8f, blockages);
        Assert.IsTrue(occ.Occupiable());
        Assert.AreEqual(1, occ.NearestBlockedCorners.Count);
    }
    
    [TestMethod]
    public void TestLargePathfinderPrefersCellTwoCornersFarthestFromInLineObstacle()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{1.9f});
        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(3, 6, 1.9f, blockages);
        Assert.IsTrue(occ.Occupiable());
        Assert.AreEqual(2, occ.CornersFarthestFromBlockages.Count);
        Assert.Contains((2.5f, 6.5f), occ.CornersFarthestFromBlockages);
        Assert.Contains((3.5f, 6.5f), occ.CornersFarthestFromBlockages);
        Assert.AreEqual(2, occ.OtherCorners.Count);
        Assert.Contains((2.5f, 5.5f), occ.OtherCorners);
        Assert.Contains((3.5f, 5.5f), occ.OtherCorners);
    }
    
    [TestMethod]
    public void TestCoordinateWherePathfinderDoesNotIntersectAnyObstacles_NullBlockageMapThrowsException()
    {
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{0.9f});
        TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
            () => sut.GetOccupiableCellCoordinates(0, 0, 0.9f, null),
            "blockages empty!");
    }
}
}