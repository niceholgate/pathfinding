using System;
using System.Collections.Generic;
using System.IO;
using NicUtils;

namespace AStarNickNS
{

    public class PathfinderObstacleIntersector : IPathfinderObstacleIntersector
    {
        public double[,] GridTerrainCosts { get; set; } = null;

        private readonly List<(double, double)> GRID_CORNER_DELTAS = new()
        {
            (0.0, 0.0), (0.5, 0.5), (-0.5, 0.5), (-0.5, -0.5), (0.5, -0.5)
        };

        public (double, double)? CoordinateWherePathfinderDoesNotIntersectAnyObstacles(int x, int y, double pathfinderSize)
        {
            if (GridTerrainCosts == null || GridTerrainCosts.Length == 0)
            {
                throw new IOException("GridTerrainCosts not yet initialised!");
            }
            
            if (GetTerrainCost(x, y) <= 0) return null;
                
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
            List<(double, double)> coordinatesWithoutIntersections = new();
            foreach ((double, double) cornerDelta in GRID_CORNER_DELTAS)
            {
                double circleCentreX = cx + cornerDelta.Item1;
                double circleCentreY = cy + cornerDelta.Item2;
                if (!CircleIntersectsWithAnyObstacle(cells, circleCentreX, circleCentreY, radiusSq))
                {
                    // We found a spot (circleCentreX, circleCentreY) in this cell (x, y) where the pathfinder fits
                    coordinatesWithoutIntersections.Add((circleCentreX, circleCentreY));
                }
            }

            if (coordinatesWithoutIntersections.Count == 1)
            {
                // If only one position can be occupied, return it.
                return coordinatesWithoutIntersections[0];
            } 
            if (coordinatesWithoutIntersections.Count > 1)
            {
                // If there are multiple positions to choose from, choose the one that is maximally distant
                // from the cell's nearest obstructed cell.
                (int, int) nearestObstructedCell = FindNearestedObstructedCell(x, y);
                double maxDistSq = double.MinValue;
                (double, double) bestCoordinate = coordinatesWithoutIntersections[0];
                foreach ((double, double) coord in coordinatesWithoutIntersections)
                {
                    double distSq = Distances2D.GetDistance(coord, nearestObstructedCell,
                        Distances2D.HeuristicType.EuclidianSquared);
                    if (distSq > maxDistSq)
                    {
                        maxDistSq = distSq;
                        bestCoordinate = coord;
                    }
                }
                return bestCoordinate;
            }
            
            // Intersections were found for all positions on the cell.
            return null;
        }

        private bool CircleIntersectsWithAnyObstacle(List<(int, int)> cells, double circleCentreX, double circleCentreY,
            double circleRadiusSquared)
        {
            foreach ((int cellCentreX, int cellCentreY) in cells)
            {
                // If this cell is an obstacle, check for intersection with the pathfinder's circle.
                if (GetTerrainCost(cellCentreX, cellCentreY) <= 0
                    && CircleIntersectsCell(cellCentreX, cellCentreY, circleCentreX, circleCentreY,
                        circleRadiusSquared))
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

        private (int, int) FindNearestedObstructedCell(int x, int y)
        {
            // This algo could exit early after the pathfinder radius is exceeded, but currently it does not
            // because we shouldn't even be here if there are no nearby obstructions
            // (because the center of the cell would be accessible and we wouldn't be checking corners).
            // Or maybe we shouldn't default to the center? Always go to an edge if it's further?
            // That would mean small pathfinders always go to edges even if the center is free and represents
            // a tighter path to follow.
            if (GetTerrainCost(x, y) <= 0)
            {
                return (x, y);
            }

            // Search in expanding square perimeters
            int maxDimension = Math.Max(GridTerrainCosts.GetLength(0), GridTerrainCosts.GetLength(1));

            for (int d = 1; d <= maxDimension; d++)
            {
                var obstructedCellsOnPerimeter = new List<(int, int)>();
                // Top and bottom edges of the square
                for (int i = -d; i <= d; i++)
                {
                    (int, int) topCell = (x + i, y - d);
                    if (GetTerrainCost(topCell.Item1, topCell.Item2) <= 0)
                    {
                        obstructedCellsOnPerimeter.Add(topCell);
                    }
                    
                    (int, int) bottomCell = (x + i, y + d);
                    if (GetTerrainCost(bottomCell.Item1, bottomCell.Item2) <= 0)
                    {
                        obstructedCellsOnPerimeter.Add(bottomCell);
                    }
                }

                // Left and right edges (excluding corners, which are already checked)
                for (int i = -d + 1; i < d; i++)
                {
                    (int, int) leftCell = (x - d, y + i);
                    if (GetTerrainCost(leftCell.Item1, leftCell.Item2) <= 0)
                    {
                        obstructedCellsOnPerimeter.Add(leftCell);
                    }
                    (int, int) rightCell = (x + d, y + i);
                    if (GetTerrainCost(rightCell.Item1, rightCell.Item2) <= 0)
                    {
                        obstructedCellsOnPerimeter.Add(rightCell);
                    }
                }

                if (obstructedCellsOnPerimeter.Count > 0)
                {
                    (int, int) closestCell = obstructedCellsOnPerimeter[0];
                    double minDistanceSq = Distances2D.GetDistance(closestCell, (x, y),
                        Distances2D.HeuristicType.EuclidianSquared);

                    foreach ((int, int) cell in obstructedCellsOnPerimeter)
                    {
                        double distSq = Distances2D.GetDistance(cell, (x, y),
                                Distances2D.HeuristicType.EuclidianSquared);
                        if (distSq < minDistanceSq)
                        {
                            minDistanceSq = distSq;
                            closestCell = cell;
                        }
                    }
                    return closestCell;
                }
            }
            throw new Exception("This should not be reached - could not find an obstructed cell");
        }
    }
}