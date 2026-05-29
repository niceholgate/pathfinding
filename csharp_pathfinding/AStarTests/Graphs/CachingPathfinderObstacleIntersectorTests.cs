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
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(0.9f, "default"), blockages).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(0, 0, new PathfinderAttributes(0.9f, "default"), blockages));
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(1.1f, "default"), blockages).Occupiable());
        Assert.IsFalse(sut.IsOccupiable(0, 0, new PathfinderAttributes(1.1f, "default"), blockages));
            
        // Inside a size 3 square
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(2.9f, "default"), blockages).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(1, 2, new PathfinderAttributes(2.9f, "default"), blockages));
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(3.1f, "default"), blockages).Occupiable());
        Assert.IsFalse(sut.IsOccupiable(1, 2, new PathfinderAttributes(3.1f, "default"), blockages));
            
        // Intersect with a corner
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(0.9f, "default"), blockages).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(0.9f, "default"), blockages));
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default"), blockages).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default"), blockages));
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default"), blockages).Occupiable());
        Assert.IsFalse(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default"), blockages));
        
        // Respond to changes in the blockages
        blockages[3, 4] = false;
        sut.Invalidate(2, 5);
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(0.9f, "default"), blockages).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(0.9f, "default"), blockages));
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default"), blockages).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default"), blockages));
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default"), blockages).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default"), blockages));
        blockages[3, 4] = true;
        sut.Invalidate(2, 5);
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(0.9f, "default"), blockages).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(0.9f, "default"), blockages));
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default"), blockages).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default"), blockages));
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default"), blockages).Occupiable());
        Assert.IsFalse(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default"), blockages));
    }
    
    [TestMethod]
    public void TestSmallerPathfinderSkipsComputationIfNextLargestPathfinderFitsOnAllCoords()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{1.2f, 1.9f});

        OccupiableCellCoordinates bigger =
            sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(1.9f, "default"), blockages);
        Assert.IsTrue(bigger.Occupiable());
        Assert.IsTrue(bigger.AllCoordsOccupiable);
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Miss, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(1, 2, new PathfinderAttributes(1.9f, "default"), blockages));
        
        OccupiableCellCoordinates smaller =
            sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(1.2f, "default"), blockages);
        Assert.IsTrue(smaller.Occupiable());
        Assert.IsTrue(smaller.AllCoordsOccupiable);
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Implied, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(1, 2, new PathfinderAttributes(1.2f, "default"), blockages));
    }
    
    [TestMethod]
    public void TestSkipsComputationIfRepeated()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{1.9f});

        OccupiableCellCoordinates one =
            sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(1.9f, "default"), blockages);
        Assert.IsTrue(one.Occupiable());
        Assert.IsTrue(one.AllCoordsOccupiable);
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Miss, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(1, 2, new PathfinderAttributes(1.9f, "default"), blockages));
        
        OccupiableCellCoordinates oneAgain =
            sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(1.9f, "default"), blockages);
        Assert.IsTrue(oneAgain.Occupiable());
        Assert.IsTrue(oneAgain.AllCoordsOccupiable);
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Hit, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(1, 2, new PathfinderAttributes(1.9f, "default"), blockages));
    }
    
    [TestMethod]
    public void TestLargePathfinderPrefersCellCornerFarthestFromCornerObstacle()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{_sub2Sqrt2});
        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default"), blockages);
        Assert.IsTrue(occ.Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default"), blockages));
        Assert.AreEqual(1, occ.CornersFarthestFromBlockages.Count);
        Assert.AreEqual((1.5f, 5.5f), occ.CornersFarthestFromBlockages[0]);
    }
    
    [TestMethod]
    public void TestNoCornersFarthestFromBlockagesAndNoNearestBlockedCornersWhenFarFromBlockages()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{0.8f});
        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(1, 7, new PathfinderAttributes(0.8f, "default"), blockages);
        Assert.IsTrue(occ.Occupiable());
        Assert.IsTrue(sut.IsOccupiable(1, 7, new PathfinderAttributes(0.8f, "default"), blockages));
        Assert.AreEqual(0, occ.CornersFarthestFromBlockages.Count);
        Assert.AreEqual(0, occ.NearestBlockedCorners.Count);
    }
    
    [TestMethod]
    public void TestNearestBlockedCornersFourCorners()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{0.8f});
        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(0, 8, new PathfinderAttributes(0.8f, "default"), blockages);
        Assert.IsTrue(occ.Occupiable());
        Assert.IsTrue(sut.IsOccupiable(0, 8, new PathfinderAttributes(0.8f, "default"), blockages));
        Assert.AreEqual(4, occ.NearestBlockedCorners.Count);
    }
    
    [TestMethod]
    public void TestNearestBlockedCornersOneCorner()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{0.8f});
        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(0.8f, "default"), blockages);
        Assert.IsTrue(occ.Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(0.8f, "default"), blockages));
        Assert.AreEqual(1, occ.NearestBlockedCorners.Count);
    }
    
    [TestMethod]
    public void TestLargePathfinderPrefersCellTwoCornersFarthestFromInLineObstacle()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float>{1.9f});
        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(3, 6, new PathfinderAttributes(1.9f, "default"), blockages);
        Assert.IsTrue(occ.Occupiable());
        Assert.IsTrue(sut.IsOccupiable(3, 6, new PathfinderAttributes(1.9f, "default"), blockages));
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
            () => sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(0.9f, "default"), null),
            "blockages empty!");
    }

    [TestMethod]
    public void TestCacheSeparationByLayer()
    {
        blockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(0), blockages.GetLength(1), new List<float> { 0.9f });

        // Layer 1
        OccupiableCellCoordinates res1 = sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(0.9f, "layer1"), blockages);
        Assert.IsTrue(res1.Occupiable());
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Miss, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(0, 0, new PathfinderAttributes(0.9f, "layer1"), blockages));

        // Layer 2 (same coords, same size)
        OccupiableCellCoordinates res2 = sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(0.9f, "layer2"), blockages);
        Assert.IsTrue(res2.Occupiable());
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Miss, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(0, 0, new PathfinderAttributes(0.9f, "layer2"), blockages));

        // Repeated Layer 1
        sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(0.9f, "layer1"), blockages);
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Hit, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(0, 0, new PathfinderAttributes(0.9f, "layer1"), blockages));

        // Repeated Layer 2
        sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(0.9f, "layer2"), blockages);
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Hit, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(0, 0, new PathfinderAttributes(0.9f, "layer2"), blockages));
    }
}
}