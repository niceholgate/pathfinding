using System;
using System.IO;
using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;
using NicUtils.ExtensionMethods;
// using NUnit.Framework;

namespace AStarTests {

[TestClass]
public class PathfinderObstacleIntersectorTests
{
    private double[,] gridTerrainCosts =
    { // y, x
        {1, 0, 0, 0, 1, 1, 1},
        {1, 1, 1, 0, 1, 1, 1},
        {1, 1, 1, 0, 1, 1, 1},
        {1, 1, 1, 0, 1, 1, 1},
        {1, 1, 1, 0, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1, 1}
    };
    
    [TestMethod]
    public void TestCoordinateWherePathfinderDoesNotIntersectAnyObstacles_Happy()
    {
        gridTerrainCosts = gridTerrainCosts.Transpose();
        PathfinderObstacleIntersector sut = new()
        {
            GridTerrainCosts = gridTerrainCosts
        };

        // Inside a size 1 square                            x, y
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(0, 0, 0.9).Occupiable());
        Assert.IsFalse(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(0, 0, 1.1).Occupiable());
            
        // Inside a size 3 square
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(1, 2, 2.9).Occupiable());
        Assert.IsFalse(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(1, 2, 3.1).Occupiable());
            
        // Intersect with a corner
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 0.9).Occupiable());
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) - 0.01).Occupiable());
        Assert.IsFalse(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) + 0.01).Occupiable());
        
        // Respond to changes in the gridTerrainCosts
        gridTerrainCosts[3, 4] = 1;
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 0.9).Occupiable());
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) - 0.01).Occupiable());
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) + 0.01).Occupiable());
        gridTerrainCosts[3, 4] = 0;
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 0.9).Occupiable());
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) - 0.01).Occupiable());
        Assert.IsFalse(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) + 0.01).Occupiable());
    }
    
    [TestMethod]
    public void TestLargePathfinderPrefersCellCornerFarthestFromCornerObstacle()
    {
        gridTerrainCosts = gridTerrainCosts.Transpose();
        PathfinderObstacleIntersector sut = new()
        {
            GridTerrainCosts = gridTerrainCosts
        };
        OccupiableCellCoordinates occ = sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) - 0.01);
        Assert.IsTrue(occ.Occupiable());
        Assert.AreEqual(1, occ.CornersFarthestFromBlockages.Count);
        Assert.AreEqual((1.5, 5.5), occ.CornersFarthestFromBlockages[0]);
    }
    
    [TestMethod]
    public void TestLargePathfinderPrefersCellTwoCornersFarthestFromInLineObstacle()
    {
        gridTerrainCosts = gridTerrainCosts.Transpose();
        PathfinderObstacleIntersector sut = new()
        {
            GridTerrainCosts = gridTerrainCosts
        };
        OccupiableCellCoordinates occ = sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(3, 6, 1.9);
        Assert.IsTrue(occ.Occupiable());
        Assert.AreEqual(2, occ.CornersFarthestFromBlockages.Count);
        Assert.Contains((2.5, 6.5), occ.CornersFarthestFromBlockages);
        Assert.Contains((3.5, 6.5), occ.CornersFarthestFromBlockages);
        Assert.AreEqual(2, occ.OtherCorners.Count);
        Assert.Contains((2.5, 5.5), occ.OtherCorners);
        Assert.Contains((3.5, 5.5), occ.OtherCorners);
    }
    
    [TestMethod]
    public void TestCoordinateWherePathfinderDoesNotIntersectAnyObstacles_UninitialisedThrowsException()
    {
        PathfinderObstacleIntersector sut = new();
        TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
            () => sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(0, 0, 0.9),
            "GridTerrainCosts not yet initialised!");
    }
}
}