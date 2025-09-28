using System;
using System.IO;

namespace AStarNickNS;

public class PathfinderObstacleIntersector : IPathfinderObstacleIntersector
{
    public double[,] GridTerrainCosts { get; set; } = null;

    public bool PathfinderIntersectsWithObstacles(int x, int y, double pathfinderSize)
    {
        if (GridTerrainCosts == null || GridTerrainCosts.Length == 0)
        {
            throw new IOException("GridTerrainCosts not yet initialised!");
        }
        
        if (GetTerrainCost(x, y) <= 0) return true;
            
        double halfWidth = pathfinderSize / 2;
        double radiusSq = halfWidth * halfWidth;
        (double cx, double cy) = (x, y);
        int radius = (int)Math.Ceiling(halfWidth);

        for (int cellX = x - radius; cellX <= x + radius; cellX++)
        {
            for (int cellY = y - radius; cellY <= y + radius; cellY++)
            {
                if (GetTerrainCost(cellX, cellY) <= 0)
                {
                    // This cell is an obstacle. Check for intersection with the pathfinder's circle.

                    // Find the closest point on the cell's square to the circle's center.
                    double closestX = Math.Max(cellX - 0.5, Math.Min(cx, cellX + 0.5));
                    double closestY = Math.Max(cellY - 0.5, Math.Min(cy, cellY + 0.5));

                    // Calculate the distance squared from the circle's center to this closest point.
                    double deltaX = cx - closestX;
                    double deltaY = cy - closestY;
                    double distanceSq = (deltaX * deltaX) + (deltaY * deltaY);

                    // If the distance is less than the circle's radius squared, there is an intersection.
                    if (distanceSq < radiusSq) return true; // Collision detected.
                }
            }
        }

        return false; // No collisions found.
    }
    
    private double GetTerrainCost(int x, int y)
    {
        if (CoordinateOutOfBounds(x, y)) return 0;
        return GridTerrainCosts[x, y];
    }
    
    private bool CoordinateOutOfBounds(int x, int y)
    {
        return x < 0 || x >= GridTerrainCosts.GetLength(0)
                     || y < 0 || y >= GridTerrainCosts.GetLength(1);
    }
}