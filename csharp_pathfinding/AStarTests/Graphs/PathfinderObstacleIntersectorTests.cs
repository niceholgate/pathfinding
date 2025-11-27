using System;
using System.IO;
using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;
using NicUtils.ExtensionMethods;

namespace AStarTests;

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
        Assert.IsNotNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(0, 0, 0.9));
        Assert.IsNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(0, 0, 1.1));
            
        // Inside a size 3 square
        Assert.IsNotNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(1, 2, 2.9));
        Assert.IsNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(1, 2, 3.1));
            
        // Intersect with a corner
        Assert.IsNotNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 0.9));
        Assert.IsNotNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) - 0.01));
        Assert.IsNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) + 0.01));
        
        // Respond to changes in the gridTerrainCosts
        gridTerrainCosts[3, 4] = 1;
        Assert.IsNotNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 0.9));
        Assert.IsNotNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) - 0.01));
        Assert.IsNotNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) + 0.01));
        gridTerrainCosts[3, 4] = 0;
        Assert.IsNotNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 0.9));
        Assert.IsNotNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) - 0.01));
        Assert.IsNull(sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(2, 5, 2*Math.Sqrt(2) + 0.01));
    }
    
    [TestMethod]
    public void TestCoordinateWherePathfinderDoesNotIntersectAnyObstacles_UninitialisedThrowsException()
    {
        PathfinderObstacleIntersector sut = new();
        TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
            () => sut.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(0, 0, 0.9),
            "GridTerrainCosts not yet initialised!");
    }
}