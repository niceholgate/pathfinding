using System;
using System.IO;
using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;

namespace AStarTests;

[TestClass]
public class PathfinderObstacleIntersectorTests
{
    private readonly double[,] gridTerrainCosts =
    { // y, x
        {1, 0, 0, 0, 1},
        {1, 1, 1, 0, 1},
        {1, 1, 1, 0, 1},
        {1, 1, 1, 0, 1},
        {1, 1, 1, 0, 1},
        {1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1}
    };
    
    [TestMethod]
    public void TestPathfinderIntersectsWithObstacles_Happy()
    {
        PathfinderObstacleIntersector sut = new()
        {
            GridTerrainCosts = gridTerrainCosts
        };

        // Inside a size 1 square                            x, y
        Assert.IsFalse(sut.PathfinderIntersectsWithObstacles(0, 0, 0.9));
        Assert.IsTrue(sut.PathfinderIntersectsWithObstacles(0, 0, 1.1));
            
        // Inside a size 3 square
        Assert.IsFalse(sut.PathfinderIntersectsWithObstacles(2, 1, 2.9));
        Assert.IsTrue(sut.PathfinderIntersectsWithObstacles(2, 1, 3.1));
            
        // Intersect with a corner
        Assert.IsFalse(sut.PathfinderIntersectsWithObstacles(5, 2, 0.9));
        Assert.IsFalse(sut.PathfinderIntersectsWithObstacles(5, 2, Math.Sqrt(2) - 0.01));
        Assert.IsTrue(sut.PathfinderIntersectsWithObstacles(5, 2, Math.Sqrt(2) + 0.01));
        
        // Respond to changes in the gridTerrainCosts
        gridTerrainCosts[4, 3] = 1;
        Assert.IsFalse(sut.PathfinderIntersectsWithObstacles(5, 2, 0.9));
        Assert.IsFalse(sut.PathfinderIntersectsWithObstacles(5, 2, Math.Sqrt(2) - 0.01));
        Assert.IsFalse(sut.PathfinderIntersectsWithObstacles(5, 2, Math.Sqrt(2) + 0.01));
        gridTerrainCosts[4, 3] = 0;
        Assert.IsFalse(sut.PathfinderIntersectsWithObstacles(5, 2, 0.9));
        Assert.IsFalse(sut.PathfinderIntersectsWithObstacles(5, 2, Math.Sqrt(2) - 0.01));
        Assert.IsTrue(sut.PathfinderIntersectsWithObstacles(5, 2, Math.Sqrt(2) + 0.01));
    }
    
    [TestMethod]
    public void TestPathfinderIntersectsWithObstacles_UninitialisedThrowsException()
    {
        PathfinderObstacleIntersector sut = new();
        TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
            () => sut.PathfinderIntersectsWithObstacles(0, 0, 0.9),
            "GridTerrainCosts not yet initialised!");
    }
}