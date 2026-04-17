using System;
using System.Collections.Generic;
using System.IO;
using NicUtils;

namespace AStarNickNS
{

    public class PathfinderObstacleIntersector : IPathfinderObstacleIntersector
    {
        public float[,] GridTerrainCosts { get; set; } = null;

        private readonly List<(float, float)> GRID_CORNER_DELTAS = new()
        {
            (0.5f, 0.5f), (-0.5f, 0.5f), (-0.5f, -0.5f), (0.5f, -0.5f)
        };

        public OccupiableCellCoordinates CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(int x, int y, float pathfinderSize)
        {
            if (GridTerrainCosts == null || GridTerrainCosts.Length == 0)
            {
                throw new IOException("GridTerrainCosts not yet initialised!");
            }
            
            OccupiableCellCoordinates occ = new OccupiableCellCoordinates {
                Centre = null,
                CornersFarthestFromBlockages = new List<(float, float)>(),
                NearestBlockedCorners = new List<(float, float)>(),
                OtherCorners = new List<(float, float)>(),
                AllCoordsOccupiable = false
            };
            
            if (GetTerrainCost(x, y) <= 0) return occ;
            
            List<(int, int)> nearestObstructedCells = FindNearestObstructedCells(x, y, pathfinderSize);
            occ.NearestBlockedCorners = FindNearestObstructedCorners(nearestObstructedCells, x, y);
            
            // Sub-cell pathfinders just go to the center
            if (pathfinderSize <= 1.0f)
            {
                occ.Centre = (x, y);
                return occ;
            }
                
            float halfWidth = pathfinderSize / 2;
            float radiusSq = halfWidth * halfWidth;
            (float cx, float cy) = (x, y);
            int radius = (int)MathF.Ceiling(halfWidth);
            List<(int, int)> cells = new();
            for (int cellX = x - radius; cellX <= x + radius; cellX++)
            {
                for (int cellY = y - radius; cellY <= y + radius; cellY++) cells.Add((cellX, cellY));
            }
            
            // The pathfinder fits in this cell if it can stand on any part of the cell with no intersections with obstacles.
            occ.Centre = CircleIntersectsWithAnyObstacle(cells, cx, cy, radiusSq)
                ? null : (cx, cy);

            List<(float, float)> cornersWithoutIntersections = new();
            foreach ((float, float) cornerDelta in GRID_CORNER_DELTAS)
            {
                float circleCentreX = cx + cornerDelta.Item1;
                float circleCentreY = cy + cornerDelta.Item2;
                if (!CircleIntersectsWithAnyObstacle(cells, circleCentreX, circleCentreY, radiusSq))
                {
                    // We found a corner (circleCentreX, circleCentreY) of this cell (x, y) where the pathfinder fits
                    cornersWithoutIntersections.Add((circleCentreX, circleCentreY));
                }
            }

            if (cornersWithoutIntersections.Count == 0) return occ;
            
            if (cornersWithoutIntersections.Count == 1)
            {
                // Only one corner to choose from.
                occ.CornersFarthestFromBlockages = cornersWithoutIntersections;
                return occ;
            }
            
            occ.AllCoordsOccupiable = cornersWithoutIntersections.Count == 4;

            // If there are multiple corners to choose from, find the one/s maximally distant
            // from the cell's nearest obstructed cell(s).
            List<float> minDistancesSq = new();
            float maxMinDistanceSq = float.MinValue;
            foreach ((float, float) corner in cornersWithoutIntersections)
            {
                float minCornerDistSq = float.MaxValue;
                foreach ((int, int) obs in nearestObstructedCells)
                {
                    float d2 = (float)Distances2D.GetDistance(corner, obs, Distances2D.HeuristicType.EuclidianSquared);
                    if (d2 < minCornerDistSq) minCornerDistSq = d2;
                }
                minDistancesSq.Add(minCornerDistSq);
                if (minCornerDistSq > maxMinDistanceSq) maxMinDistanceSq = minCornerDistSq;
            }
            for (int i = 0; i < cornersWithoutIntersections.Count; i++)
            {
                if (MathF.Abs(minDistancesSq[i] - maxMinDistanceSq) < 1e-6f)
                {
                    occ.CornersFarthestFromBlockages.Add(cornersWithoutIntersections[i]);
                }
                else
                {
                    occ.OtherCorners.Add(cornersWithoutIntersections[i]);
                }
            }

            return occ;
        }

        private bool CircleIntersectsWithAnyObstacle(List<(int, int)> cells, float circleCentreX, float circleCentreY,
            float circleRadiusSquared)
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

        private bool CircleIntersectsCell(int cellCentreX, int cellCentreY, float circleCentreX, float circleCentreY,
            float circleRadiusSquared)
        {
            // Find the closest point on the cell's square to the circle's center.
            float closestX = MathF.Max(cellCentreX - 0.5f, MathF.Min(circleCentreX, cellCentreX + 0.5f));
            float closestY = MathF.Max(cellCentreY - 0.5f, MathF.Min(circleCentreY, cellCentreY + 0.5f));

            // Calculate the distance squared from the circle's center to this closest point.
            float deltaX = circleCentreX - closestX;
            float deltaY = circleCentreY - closestY;
            float distanceSquared = deltaX * deltaX + deltaY * deltaY;

            return distanceSquared <= circleRadiusSquared;
        }

        private float GetTerrainCost(int x, int y)
        {
            if (CoordinateOutOfBounds(x, y)) return 0;
            return GridTerrainCosts[x, y];
        }

        private bool CoordinateOutOfBounds(int x, int y)
        {
            return x < 0 || x >= GridTerrainCosts.GetLength(0)
                         || y < 0 || y >= GridTerrainCosts.GetLength(1);
        }

        private List<(float, float)> FindNearestObstructedCorners(List<(int, int)> nearestObstructedCells, int x, int y)
        {
            List<(float, float)> nearestObstructedCorners = new List<(float, float)>();
            foreach ((int x, int y) obstructedCell in nearestObstructedCells)
            {
                List<float> nearestX = new List<float>();
                if (obstructedCell.x == x)
                {
                    nearestX.Add(obstructedCell.x + 0.5f);
                    nearestX.Add(obstructedCell.x - 0.5f);
                } else if (obstructedCell.x < x)
                {
                    nearestX.Add(obstructedCell.x + 0.5f);
                } else
                {
                    nearestX.Add(obstructedCell.x - 0.5f);
                }
                
                List<float> nearestY = new List<float>();
                if (obstructedCell.y == y)
                {
                    nearestY.Add(obstructedCell.y + 0.5f);
                    nearestY.Add(obstructedCell.y - 0.5f);
                } else if (obstructedCell.y < y)
                {
                    nearestY.Add(obstructedCell.y + 0.5f);
                } else
                {
                    nearestY.Add(obstructedCell.y - 0.5f);
                }

                foreach (float X in nearestX)
                {
                    foreach (float Y in nearestY) nearestObstructedCorners.Add((X, Y));
                }
                
            }
            return nearestObstructedCorners;
        }
        
        private List<(int, int)> FindNearestObstructedCells(int x, int y, float pathfinderSize)
        {
            List<(int, int)> closestCells = new List<(int, int)>();
            if (GetTerrainCost(x, y) <= 0)
            {
                return closestCells;
            }

            // Search in expanding square perimeters - no need to search cells that the pathfinder could never touch from this cell
            int maxDimension = (int)MathF.Ceiling(0.5f + pathfinderSize / 2);

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
                    float minDistanceSq = float.MaxValue;
                    // First pass: find min distance
                    foreach ((int, int) cell in obstructedCellsOnPerimeter)
                    {
                        float distSq = (float)Distances2D.GetDistance(cell, (x, y), Distances2D.HeuristicType.EuclidianSquared);
                        if (distSq < minDistanceSq)
                        {
                            minDistanceSq = distSq;
                        }
                    }
                    
                    // Second pass: collect all cells with that distance
                    foreach ((int, int) cell in obstructedCellsOnPerimeter)
                    {
                        float distSq = (float)Distances2D.GetDistance(cell, (x, y), Distances2D.HeuristicType.EuclidianSquared);
                        if (MathF.Abs(distSq - minDistanceSq) < 1e-6f)
                        {
                            closestCells.Add(cell);
                        }
                    }

                    break;
                }
            }
            
            return closestCells;
        }
    }

    public struct OccupiableCellCoordinates
    {
        public (float, float)? Centre { get; set; }
        public List<(float, float)> CornersFarthestFromBlockages { get; set; }
        public List<(float, float)> OtherCorners { get; set; }
        public List<(float, float)> NearestBlockedCorners { get; set; }
        public bool Occupiable()
        {
            return Centre != null || CornersFarthestFromBlockages.Count > 0;
        }
        public bool AllCoordsOccupiable { get; set; }
    }
}