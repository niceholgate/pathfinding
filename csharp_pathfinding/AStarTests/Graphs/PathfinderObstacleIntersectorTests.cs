using System;
using System.IO;
using System.Linq;
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
        {1, 0, 0, 0, 1},
        {1, 1, 1, 0, 1},
        {1, 1, 1, 0, 1},
        {1, 1, 1, 0, 1},
        {1, 1, 1, 0, 1},
        {1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1}
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
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(0, 0, 0.9).Any());
        Assert.IsFalse(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(0, 0, 1.1).Any());
            
        // Inside a size 3 square
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(1, 2, 2.9).Any());
        Assert.IsFalse(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(1, 2, 3.1).Any());
            
        // Intersect with a corner
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 0.9).Any());
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) - 0.01).Any());
        Assert.IsFalse(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) + 0.01).Any());
        
        // Respond to changes in the gridTerrainCosts
        gridTerrainCosts[3, 4] = 1;
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 0.9).Any());
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) - 0.01).Any());
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) + 0.01).Any());
        gridTerrainCosts[3, 4] = 0;
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 0.9).Any());
        Assert.IsTrue(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) - 0.01).Any());
        Assert.IsFalse(sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) + 0.01).Any());
    }
    
    [TestMethod]
    public void TestLargePathfinderPrefersCellCornerFarthestFromCornerObstacle()
    {
        gridTerrainCosts = gridTerrainCosts.Transpose();
        PathfinderObstacleIntersector sut = new()
        {
            GridTerrainCosts = gridTerrainCosts
        };
        Assert.AreEqual((1.5, 5.5), sut.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) - 0.01)[0]);
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