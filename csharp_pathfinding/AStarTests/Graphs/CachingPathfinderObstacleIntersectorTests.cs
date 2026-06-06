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
        bool[,] transposedBlockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(transposedBlockages.GetLength(0), transposedBlockages.GetLength(1), new List<float>{0.9f, 1.1f, _sub2Sqrt2, _sup2Sqrt2, 2.9f, 3.1f});
        sut.SetBlockageLayer("default", transposedBlockages);

        // Inside a size 1 square                            x, y
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(0.9f, "default")).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(0, 0, new PathfinderAttributes(0.9f, "default")));
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(1.1f, "default")).Occupiable());
        Assert.IsFalse(sut.IsOccupiable(0, 0, new PathfinderAttributes(1.1f, "default")));

        // Inside a size 3 square
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(2.9f, "default")).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(1, 2, new PathfinderAttributes(2.9f, "default")));
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(3.1f, "default")).Occupiable());
        Assert.IsFalse(sut.IsOccupiable(1, 2, new PathfinderAttributes(3.1f, "default")));

        // Intersect with a corner
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(0.9f, "default")).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(0.9f, "default")));
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default")).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default")));
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default")).Occupiable());
        Assert.IsFalse(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default")));

        // Respond to changes in the blockages
        sut.SetBlockage("default", 3, 4, false);
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(0.9f, "default")).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(0.9f, "default")));
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default")).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default")));
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default")).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default")));
        
        sut.SetBlockage("default", 3, 4, true);
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(0.9f, "default")).Occupiable());   
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(0.9f, "default")));
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default")).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default")));
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default")).Occupiable());
        Assert.IsFalse(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default")));

        // --- Un/setting blockage in one coord affects large pathfinders trying to fit in neighbour coords
        sut.SetBlockage("default", 3, 4, false);
        Assert.IsTrue(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default")).Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default")));
        
        sut.SetBlockage("default", 3, 4, true);
        Assert.IsFalse(sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default")).Occupiable());
        Assert.IsFalse(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sup2Sqrt2, "default")));
    }

    [TestMethod]
    public void TestSmallerPathfinderSkipsComputationIfNextLargestPathfinderFitsOnAllCoords()
    {
        bool[,] transposedBlockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(transposedBlockages.GetLength(0), transposedBlockages.GetLength(1), new List<float>{1.2f, 1.9f});
        sut.SetBlockageLayer("default", transposedBlockages);

        OccupiableCellCoordinates bigger =
            sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(1.9f, "default"));
        Assert.IsTrue(bigger.Occupiable());
        Assert.IsTrue(bigger.AllCoordsOccupiable);
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Miss, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(1, 2, new PathfinderAttributes(1.9f, "default")));

        OccupiableCellCoordinates smaller =
            sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(1.2f, "default"));
        Assert.IsTrue(smaller.Occupiable());
        Assert.IsTrue(smaller.AllCoordsOccupiable);
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Implied, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(1, 2, new PathfinderAttributes(1.2f, "default")));
    }

    [TestMethod]
    public void TestSkipsComputationIfRepeated()
    {
        bool[,] transposedBlockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(transposedBlockages.GetLength(0), transposedBlockages.GetLength(1), new List<float>{1.9f});      
        sut.SetBlockageLayer("default", transposedBlockages);

        OccupiableCellCoordinates one =
            sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(1.9f, "default"));
        Assert.IsTrue(one.Occupiable());
        Assert.IsTrue(one.AllCoordsOccupiable);
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Miss, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(1, 2, new PathfinderAttributes(1.9f, "default")));

        OccupiableCellCoordinates oneAgain =
            sut.GetOccupiableCellCoordinates(1, 2, new PathfinderAttributes(1.9f, "default"));
        Assert.IsTrue(oneAgain.Occupiable());
        Assert.IsTrue(oneAgain.AllCoordsOccupiable);
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Hit, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(1, 2, new PathfinderAttributes(1.9f, "default")));
    }

    [TestMethod]
    public void TestLargePathfinderPrefersCellCornerFarthestFromCornerObstacle()
    {
        bool[,] transposedBlockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(transposedBlockages.GetLength(0), transposedBlockages.GetLength(1), new List<float>{_sub2Sqrt2});
        sut.SetBlockageLayer("default", transposedBlockages);

        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default"));
        Assert.IsTrue(occ.Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(_sub2Sqrt2, "default")));
        Assert.AreEqual(1, occ.CornersFarthestFromBlockages.Count);
        Assert.AreEqual((1.5f, 5.5f), occ.CornersFarthestFromBlockages[0]);
    }

    [TestMethod]
    public void TestFourCornersFarthestFromBlockagesAndZeroNearestBlockedCornersWhenFarFromBlockages()
    {
        bool[,] transposedBlockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(transposedBlockages.GetLength(0), transposedBlockages.GetLength(1), new List<float>{0.8f});      
        sut.SetBlockageLayer("default", transposedBlockages);

        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(1, 7, new PathfinderAttributes(0.8f, "default"));
        Assert.IsTrue(occ.Occupiable());
        Assert.IsTrue(sut.IsOccupiable(1, 7, new PathfinderAttributes(0.8f, "default")));
        Assert.AreEqual(4, occ.CornersFarthestFromBlockages.Count);
        Assert.AreEqual(0, occ.NearestBlockedCorners.Count);      
    }

    [TestMethod]
    public void TestNearestBlockedCornersFourCorners()
    {
        bool[,] transposedBlockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(transposedBlockages.GetLength(0), transposedBlockages.GetLength(1), new List<float>{0.8f});      
        sut.SetBlockageLayer("default", transposedBlockages);

        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(0, 8, new PathfinderAttributes(0.8f, "default"));
        Assert.IsTrue(occ.Occupiable());
        Assert.IsTrue(sut.IsOccupiable(0, 8, new PathfinderAttributes(0.8f, "default")));
        Assert.AreEqual(4, occ.NearestBlockedCorners.Count);      
    }

    [TestMethod]
    public void TestNearestBlockedCornersOneCorner()
    {
        bool[,] transposedBlockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(transposedBlockages.GetLength(0), transposedBlockages.GetLength(1), new List<float>{0.8f});      
        sut.SetBlockageLayer("default", transposedBlockages);

        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(2, 5, new PathfinderAttributes(0.8f, "default"));
        Assert.IsTrue(occ.Occupiable());
        Assert.IsTrue(sut.IsOccupiable(2, 5, new PathfinderAttributes(0.8f, "default")));
        Assert.AreEqual(1, occ.NearestBlockedCorners.Count);      
    }

    [TestMethod]
    public void TestLargePathfinderPrefersCellTwoCornersFarthestFromInLineObstacle()
    {
        bool[,] transposedBlockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(transposedBlockages.GetLength(0), transposedBlockages.GetLength(1), new List<float>{1.9f});      
        sut.SetBlockageLayer("default", transposedBlockages);

        OccupiableCellCoordinates occ = sut.GetOccupiableCellCoordinates(3, 6, new PathfinderAttributes(1.9f, "default"));
        Assert.IsTrue(occ.Occupiable());
        Assert.IsTrue(sut.IsOccupiable(3, 6, new PathfinderAttributes(1.9f, "default")));
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
        CachingPathfinderObstacleIntersector sut = new(blockages.GetLength(1), blockages.GetLength(0), new List<float>{0.9f});      

        TestHelpers.AssertThrowsExceptionWithMessage<ArgumentNullException>(
            () => sut.SetBlockageLayer("default", null),
            "Value cannot be null. (Parameter 'blockageGrid')");
        
        TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
            () => sut.IsOccupiable(0, 0, new PathfinderAttributes(0.9f, "nonexistent")),
            "blockages empty!");
    }

    [TestMethod]
    public void TestCacheSeparationByLayer()
    {
        bool[,] transposedBlockages = blockages.Transpose();
        CachingPathfinderObstacleIntersector sut = new(transposedBlockages.GetLength(0), transposedBlockages.GetLength(1), new List<float> { 0.9f });   
        sut.SetBlockageLayer("layer1", transposedBlockages);
        sut.SetBlockageLayer("layer2", transposedBlockages);

        // Layer 1
        OccupiableCellCoordinates res1 = sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(0.9f, "layer1"));
        Assert.IsTrue(res1.Occupiable());
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Miss, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(0, 0, new PathfinderAttributes(0.9f, "layer1")));

        // Layer 2 (same coords, same size)
        OccupiableCellCoordinates res2 = sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(0.9f, "layer2"));
        Assert.IsTrue(res2.Occupiable());
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Miss, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(0, 0, new PathfinderAttributes(0.9f, "layer2")));

        // Repeated Layer 1
        sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(0.9f, "layer1"));
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Hit, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(0, 0, new PathfinderAttributes(0.9f, "layer1")));

        // Repeated Layer 2
        sut.GetOccupiableCellCoordinates(0, 0, new PathfinderAttributes(0.9f, "layer2"));
        Assert.AreEqual(CachingPathfinderObstacleIntersector.CacheCheckResult.Hit, sut.LastCacheCheckResult);
        Assert.IsTrue(sut.IsOccupiable(0, 0, new PathfinderAttributes(0.9f, "layer2")));
    }
}
}
