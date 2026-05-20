using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AStarTests
{
    [TestClass]
    public class GeometryUtilsTests
    {
        [TestMethod]
        public void TestGetDistanceToLineSegment()
        {
            ////////////// PROJECTION OF POINT IS WITHIN THE LINE SEGMENT
            
            // Horizontal line
            Assert.AreEqual(10.0f, GeometryUtils.GetDistanceToLineSegment((0, 0), (1, 0), (0.5f, 10)), 1e-6f);
            Assert.AreEqual(10.0f, GeometryUtils.GetDistanceToLineSegment((0, 0), (1, 0), (0.5f, -10)), 1e-6f);

            // Vertical line
            Assert.AreEqual(10.0f, GeometryUtils.GetDistanceToLineSegment((0, 0), (0, 1), (10, 0.5f)), 1e-6f);
            Assert.AreEqual(10.0f, GeometryUtils.GetDistanceToLineSegment((0, 0), (0, 1), (-10, 0.5f)), 1e-6f);

            // 45 degree line
            // Point (0, 1) should be at distance 1/sqrt(2)
            Assert.AreEqual(1/MathF.Sqrt(2.0f), GeometryUtils.GetDistanceToLineSegment((0, 0), (1, 1), (0, 1)), 1e-6f);

            // Line points are same
            Assert.AreEqual(5.0f, GeometryUtils.GetDistanceToLineSegment((0, 0), (0, 0), (3, 4)), 1e-6f);
            
            ////////////// PROJECTION OF POINT IS BEYOND THE LINE SEGMENT
            Assert.AreEqual(MathF.Sqrt(2.0f), GeometryUtils.GetDistanceToLineSegment((0, 0), (1, 0), (2, 1)), 1e-6f);
            Assert.AreEqual(MathF.Sqrt(2.0f), GeometryUtils.GetDistanceToLineSegment((0, 0), (1, 0), (-1, 1)), 1e-6f);
        }

        [TestMethod]
        public void TestCircleFitsOnBoundary()
        {
            // Grid layout (3x3):
            // (0,2) (1,2) (2,2)
            // (0,1) (1,1) (2,1)
            // (0,0) (1,0) (2,0)
            bool[,] blockages = new bool[3, 3];

            // 1. Orthogonal fit (Horizontal)
            // Move (0,1) -> (1,1). Pathfinder size 0.9. No blockages.
            Assert.IsTrue(GeometryUtils.CircleFitsOnBoundary(0, 0, 1, 1, 1, 0.9f, blockages));

            // 2. Orthogonal blocked (Horizontal)
            // Move (0,1) -> (1,1). Pathfinder size 1.1. Block (0,0) or (1,0). 
            // Boundary midpoint is (0.5, 1). Checks vertices (0.5, 1 +/- 0.5) -> (0.5, 1.5) and (0.5, 0.5).
            // Vertex (0.5, 0.5) touches cells (0,0), (1,0), (0,1), (1,1).
            blockages[0, 0] = true;
            Assert.IsFalse(GeometryUtils.CircleFitsOnBoundary(0, 0, 1, 1, 1, 1.1f, blockages));
            blockages[0, 0] = false;

            // 3. Diagonal fit
            // Move (0,0) -> (1,1). Pathfinder size 0.9.
            Assert.IsTrue(GeometryUtils.CircleFitsOnBoundary(1, 0, 0, 1, 1, 0.9f, blockages));

            // 4. Diagonal blocked
            // Move (0,0) -> (1,1). Pathfinder size 1.5. 
            // Midpoint (0.5, 0.5). Direction (1,1). Perpendicular (1, -1).
            // Vertex check at (0.5, 0.5) + SQRT2 * (1, -1) etc.
            // Vertex (1, 0) or (0, 1) are reached if halfSize >= 0.5 * SQRT2 approx 0.707.
            // So pathfinderSize >= 1.414 should fail if (1,0) is blocked.
            blockages[1, 0] = true;
            Assert.IsFalse(GeometryUtils.CircleFitsOnBoundary(1, 0, 0, 1, 1, 1.5f, blockages));
            blockages[1, 0] = false;

            // 5. Boundary condition (Out of bounds should not block)
            // Move (0,0) -> (1,0). Size 2.0.
            // Midpoint (0.5, 0). Checks vertices (0.5, 0.5) and (0.5, -0.5).
            // (0.5, -0.5) touches cells (0,-1), (1,-1) which are out of bounds.
            Assert.IsTrue(GeometryUtils.CircleFitsOnBoundary(0, 0, 0, 1, 0, 2.0f, blockages));
        }
    }
}