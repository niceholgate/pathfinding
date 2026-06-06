using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NicUtils;

namespace AStarNickNS
{

    public class CachingPathfinderObstacleIntersector
    {
        public enum CacheCheckResult
        {
            Hit,
            Miss,
            Implied
        }
        
        public CacheCheckResult LastCacheCheckResult { get; private set; }

        private readonly int _width;
        private readonly int _height;

        private readonly Dictionary<string, bool[,]> _blockages = new();

        // null bool means not yet calculated/cache invalidated
        private readonly Dictionary<PathfinderAttributes, bool?[,]> _isOccupiableCache = new();
        
        // don't need to worry about caching invalidation on this one
        // - just stores the last seen coordinate where a pathfinder fits
        private readonly Dictionary<PathfinderAttributes, OccupiableCellCoordinates[,]> _fitsCoords = new();
        
        private readonly List<(float, float)> GRID_CORNER_DELTAS = new()
        {
            (0.5f, 0.5f), (-0.5f, 0.5f), (-0.5f, -0.5f), (0.5f, -0.5f)
        };
        
        private readonly List<(float, float)> GRID_EDGE_DELTAS = new()
        {
            (0.5f, 0.0f), (-0.5f, 0.0f), (0.0f, -0.5f), (0.0f, 0.5f)
        };
        
        private readonly SortedList<float, float?> _descendingOrderedPathfinderSizesWithNextLargestSizes
            = new(Comparer<float>.Create((x, y) => y.CompareTo(x)));

        public CachingPathfinderObstacleIntersector(int width, int height, List<float> pathfinderSizes)
        {
            _width = width;
            _height = height;
            List<float> descendingSizes = pathfinderSizes.ToList();
            descendingSizes.Sort((x, y) => y.CompareTo(x));
            float? previousPathfinderSize = null;
            foreach (var size in descendingSizes)
            {
                _descendingOrderedPathfinderSizesWithNextLargestSizes.Add(size, previousPathfinderSize);
                
                previousPathfinderSize = size;
            }
        }

        public bool[,] GetBlockages(string blockageLayer)
        {
            if (!_blockages.TryGetValue(blockageLayer, out var layer))
            {
                throw new IOException("blockages empty!");
            }
            return layer;
        }

        public void SetBlockageLayer(string name, bool[,] blockageGrid)
        {
            if (blockageGrid == null) throw new ArgumentNullException(nameof(blockageGrid));
            _blockages[name] = blockageGrid;
            InvalidateEntireLayer(name);
        }

        public void SetBlockage(string blockageLayer, int x, int y, bool isBlocked)
        {
            if (!_blockages.TryGetValue(blockageLayer, out var layer))
            {
                layer = new bool[_width, _height];
                _blockages[blockageLayer] = layer;
            }

            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
                throw new ArgumentOutOfRangeException("Coordinate is out of bounds.");
            }
            bool oldBlocked = layer[x, y];
            if (oldBlocked != isBlocked)
            {
                layer[x, y] = isBlocked;
                Invalidate(x, y, blockageLayer);
            }
        }

        private void Invalidate(int x, int y, string blockageLayer)
        {
            var keysToUpdate = _isOccupiableCache.Keys.Where(k => k.BlockageLayer == blockageLayer).ToList();
            foreach (var key in keysToUpdate)
            {
                // We need to invalidate + recalc not only the cell whose blockage changed, but also nearby cells
                // where pathfinders of various sizes may have lost or gained the ability to fit there due to
                // this blockage change.
                float halfWidth = key.Size / 2;
                int radius = (int)MathF.Ceiling(halfWidth);
                for (int cellX = x - radius; cellX <= x + radius; cellX++)
                {
                    if (cellX < 0 || cellX >= _width) continue;
                    for (int cellY = y - radius; cellY <= y + radius; cellY++)
                    {
                        if (cellY < 0 || cellY >= _height) continue;
                        _isOccupiableCache[key][cellX, cellY] = null;
                        EnsureCached(cellX, cellY, key);
                    }
                }
            }
        }

        public void InvalidateEntireLayer(string blockageLayer)
        {
            var keysToRemove = _isOccupiableCache.Keys.Where(k => k.BlockageLayer == blockageLayer).ToList();
            foreach (var key in keysToRemove)
            {
                _isOccupiableCache.Remove(key);
                _fitsCoords.Remove(key);
            }
        }
        
        public bool IsOccupiable(int x, int y, PathfinderAttributes attrs)
        {
            LastCacheCheckResult = EnsureCached(x, y, attrs);
            return _isOccupiableCache[attrs][x, y].Value;
        }
        
        public OccupiableCellCoordinates GetOccupiableCellCoordinates(int x, int y,
            PathfinderAttributes attrs)
        {
            LastCacheCheckResult = EnsureCached(x, y, attrs);
            return _fitsCoords[attrs][x, y];
        }
        
        private CacheCheckResult EnsureCached(int x, int y, PathfinderAttributes attrs)
        {
            if (!_isOccupiableCache.ContainsKey(attrs))
            {
                _isOccupiableCache[attrs] = new bool?[_width, _height];
                _fitsCoords[attrs] = new OccupiableCellCoordinates[_width, _height];
            }

            if (_isOccupiableCache[attrs][x, y] != null) return CacheCheckResult.Hit;
            
            // If the previous (larger) pathfinder fits here on all coordinates, then so will the
            // current (smaller) pathfinder, so skip the expensive intersection check and just copy the
            // previous pathfinder's results.
            float? nextLargestPathfinderSize = _descendingOrderedPathfinderSizesWithNextLargestSizes[attrs.Size];
            if (nextLargestPathfinderSize != null)
            {
                var nextAttrs = new PathfinderAttributes(nextLargestPathfinderSize.Value, attrs.BlockageLayer);
                EnsureCached(x, y, nextAttrs);
                
                if (_isOccupiableCache[nextAttrs][x, y].Value
                    && _fitsCoords[nextAttrs][x, y].AllCoordsOccupiable)
                {
                    _isOccupiableCache[attrs][x, y] = true;
                    _fitsCoords[attrs][x, y] = _fitsCoords[nextAttrs][x, y];
                    return CacheCheckResult.Implied;
                }
            }

            // Otherwise, need to do actual computation for this size and coords
            OccupiableCellCoordinates fitCoordinates =
                CoordinatesWherePathfinderDoesNotIntersectAnyObstaclesInner(x, y, attrs.Size, GetBlockages(attrs.BlockageLayer));
            _fitsCoords[attrs][x, y] = fitCoordinates;
            _isOccupiableCache[attrs][x, y] = fitCoordinates.Occupiable();
            
            return CacheCheckResult.Miss;
        }
        
        private OccupiableCellCoordinates CoordinatesWherePathfinderDoesNotIntersectAnyObstaclesInner(int x, int y, float pathfinderSize, bool[,] blockages)
        {
            if (blockages == null || blockages.Length == 0)
            {
                throw new IOException("blockages empty!");
            }
            
            OccupiableCellCoordinates occ = new OccupiableCellCoordinates {
                Centre = null,
                CornersFarthestFromBlockages = new List<(float, float)>(),
                NearestBlockedCorners = new List<(float, float)>(),
                OtherCorners = new List<(float, float)>(),
                AllCoordsOccupiable = false,
                OccupiableEdges = new List<(float, float)>()
            };
            
            if (IsBlocked(x, y, blockages)) return occ;
            
            float halfWidth = pathfinderSize / 2;
            float radiusSq = halfWidth * halfWidth;
            (float cx, float cy) = (x, y);
            int radius = (int)MathF.Ceiling(halfWidth);
            List<(int, int)> candidateCells = new();
            for (int cellX = x - radius; cellX <= x + radius; cellX++)
            {
                for (int cellY = y - radius; cellY <= y + radius; cellY++) candidateCells.Add((cellX, cellY));
            }
            
            foreach ((float, float) edgeDelta in GRID_EDGE_DELTAS)
            {
                float circleCentreX = cx + edgeDelta.Item1;
                float circleCentreY = cy + edgeDelta.Item2;
                if (!CircleIntersectsWithAnyObstacle(candidateCells, circleCentreX, circleCentreY, radiusSq, blockages))
                {
                    // We found an edge (circleCentreX, circleCentreY) of this cell (x, y) where the pathfinder fits
                    occ.OccupiableEdges.Add((circleCentreX, circleCentreY));
                }
            }
            
            List<(int, int)> nearestObstructedCells = FindNearestObstructedCells(x, y, pathfinderSize, blockages);
            occ.NearestBlockedCorners = FindNearestObstructedCorners(nearestObstructedCells, x, y);
            
            // Sub-cell pathfinders just go to the center
            
            // The pathfinder fits in this cell if it can stand on any part of the cell with no intersections with obstacles.
            occ.Centre = CircleIntersectsWithAnyObstacle(candidateCells, cx, cy, radiusSq, blockages)
                ? null : (cx, cy);

            List<(float, float)> cornersWithoutIntersections = new();
            foreach ((float, float) cornerDelta in GRID_CORNER_DELTAS)
            {
                float circleCentreX = cx + cornerDelta.Item1;
                float circleCentreY = cy + cornerDelta.Item2;
                if (!CircleIntersectsWithAnyObstacle(candidateCells, circleCentreX, circleCentreY, radiusSq, blockages))
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
            
            occ.AllCoordsOccupiable = cornersWithoutIntersections.Count == 4 && occ.OccupiableEdges.Count == 4;

            // If there are multiple corners to choose from, find the one/s maximally distant
            // from the cell's nearest obstructed cell(s).
            List<float> minDistancesSq = new();
            float maxMinDistanceSq = float.MinValue;
            foreach ((float, float) corner in cornersWithoutIntersections)
            {
                float minCornerDistSq = float.MaxValue;
                foreach ((int, int) obs in nearestObstructedCells)
                {
                    float d2 = Distances2D.GetDistance(corner, obs, Distances2D.HeuristicType.EuclidianSquared);
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

        private bool CircleIntersectsWithAnyObstacle(List<(int, int)> candidateCells, float circleCentreX,
            float circleCentreY, float circleRadiusSquared, bool[,] blockages)
        {
            foreach ((int cellCentreX, int cellCentreY) in candidateCells)
            {
                // If this cell is an obstacle, check for intersection with the pathfinder's circle.
                if (IsBlocked(cellCentreX, cellCentreY, blockages)
                    && CircleIntersectsCell(cellCentreX, cellCentreY, circleCentreX, circleCentreY,
                        circleRadiusSquared))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CircleIntersectsCell(int cellCentreX, int cellCentreY, float circleCentreX, float circleCentreY,
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

        private static bool IsBlocked(int x, int y, bool[,] blockages)
        {
            if (CoordinateOutOfBounds(x, y, blockages)) return true;
            return blockages[x, y];
        }

        private static bool CoordinateOutOfBounds(int x, int y, bool[,] blockages)
        {
            return x < 0 || x >= blockages.GetLength(0)
                         || y < 0 || y >= blockages.GetLength(1);
        }

        private static List<(float, float)> FindNearestObstructedCorners(List<(int, int)> nearestObstructedCells, int x, int y)
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
        
        private static List<(int, int)> FindNearestObstructedCells(int x, int y, float pathfinderSize, bool[,] blockages)
        {
            List<(int, int)> closestCells = new List<(int, int)>();
            if (IsBlocked(x, y, blockages))
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
                    if (IsBlocked(topCell.Item1, topCell.Item2, blockages))
                    {
                        obstructedCellsOnPerimeter.Add(topCell);
                    }
                    
                    (int, int) bottomCell = (x + i, y + d);
                    if (IsBlocked(bottomCell.Item1, bottomCell.Item2, blockages))
                    {
                        obstructedCellsOnPerimeter.Add(bottomCell);
                    }
                }

                // Left and right edges (excluding corners, which are already checked)
                for (int i = -d + 1; i < d; i++)
                {
                    (int, int) leftCell = (x - d, y + i);
                    if (IsBlocked(leftCell.Item1, leftCell.Item2, blockages))
                    {
                        obstructedCellsOnPerimeter.Add(leftCell);
                    }
                    (int, int) rightCell = (x + d, y + i);
                    if (IsBlocked(rightCell.Item1, rightCell.Item2, blockages))
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
                        float distSq = Distances2D.GetDistance(cell, (x, y), Distances2D.HeuristicType.EuclidianSquared);
                        if (distSq < minDistanceSq)
                        {
                            minDistanceSq = distSq;
                        }
                    }
                    
                    // Second pass: collect all cells with that distance
                    foreach ((int, int) cell in obstructedCellsOnPerimeter)
                    {
                        float distSq = Distances2D.GetDistance(cell, (x, y), Distances2D.HeuristicType.EuclidianSquared);
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
        
        public List<(float, float)> OccupiableEdges { get; set; }
    }
}