using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AStarNickNS;

public class PathfinderObstacleIntersector : IPathfinderObstacleIntersector
{
    public double[,] GridTerrainCosts { get; set; } = null;

    private List<(double, double)> GRID_CORNER_DELTAS = new List<(double, double)>
    {
        (0.0, 0.0), (0.5, 0.5), (-0.5, 0.5), (-0.5, -0.5), (0.5, -0.5)
    };

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
        List<(int, int)> cells = new();
        for (int cellX = x - radius; cellX <= x + radius; cellX++)
        {
            for (int cellY = y - radius; cellY <= y + radius; cellY++) cells.Add((cellX, cellY));
        }
        
        // The pathfinder fits in this cell if it can stand on any part of the cell with no intersections with obstacles.
        foreach ((double, double) cornerDelta in GRID_CORNER_DELTAS)
        {
            double circleCentreX = cx + cornerDelta.Item1;
            double circleCentreY = cy + cornerDelta.Item2;
            if (!CircleIntersectsWithAnyObstacle(cells, circleCentreX, circleCentreY, radiusSq))
            {
                return false;
            }
        }

        return true; // Intersections were found for all positions on the cell.
    }

    private bool CircleIntersectsWithAnyObstacle(List<(int, int)> cells, double circleCentreX, double circleCentreY, double circleRadiusSquared)
    {
        foreach ((int cellCentreX, int cellCentreY) in cells)
        {
            // If this cell is an obstacle, check for intersection with the pathfinder's circle.
            if (GetTerrainCost(cellCentreX, cellCentreY) <= 0
                && CircleIntersectsCell(cellCentreX, cellCentreY, circleCentreX, circleCentreY, circleRadiusSquared))
            {
                return true;
            }
        }

        return false;
    }

    private bool CircleIntersectsCell(int cellCentreX, int cellCentreY, double circleCentreX, double circleCentreY,
        double circleRadiusSquared)
    {
        // Find the closest point on the cell's square to the circle's center.
        double closestX = Math.Max(cellCentreX - 0.5, Math.Min(circleCentreX, cellCentreX + 0.5));
        double closestY = Math.Max(cellCentreY - 0.5, Math.Min(circleCentreY, cellCentreY + 0.5));

        // Calculate the distance squared from the circle's center to this closest point.
        double deltaX = circleCentreX - closestX;
        double deltaY = circleCentreY - closestY;
        double distanceSquared = deltaX * deltaX + deltaY * deltaY;

        return distanceSquared <= circleRadiusSquared;
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