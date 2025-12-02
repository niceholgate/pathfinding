using System;
using System.Collections.Generic;

namespace AStarNickNS
{
    public readonly struct CellIntersectionData
    {
        public readonly int x, y;
        public readonly (float, float) int1, int2;

        public CellIntersectionData(int x, int y, (float, float) int1, (float, float) int2)
        {
            this.x = x;
            this.y = y;
            this.int1 = int1;
            this.int2 = int2;
        }

        public float IntersectedDistance
        {
            get
            {
                float dx = int1.Item1 - int2.Item1;
                float dy = int1.Item2 - int2.Item2;
                return MathF.Sqrt(dx * dx + dy * dy);
            }
        }
    }

    public class Line
    {
        public float M { get; }
        public float C { get; }
        
        public Line(float m, float c)
        {
            M = m;
            C = c;
        }
        
        public Line((int x, int y) start, (int x, int y) end)
        {
            if (end.x == start.x)
            {
                M = float.PositiveInfinity; // Vertical line
                C = start.x; // Store x-intercept
            }
            else
            {
                M = (float)(end.y - start.y) / (end.x - start.x);
                C = start.y - M * start.x;
            }
        }

        public float X(float y)
        {
            if (float.IsPositiveInfinity(M)) return C; // Vertical line
            if (M == 0) throw new InvalidOperationException("Cannot calculate Y for a horizontal line.");
            return (y - C) / M;
        }
        
        public float Y(float x)
        {
            if (float.IsPositiveInfinity(M)) throw new InvalidOperationException("Cannot calculate Y for a vertical line.");
            return M * x + C;
        }
    }
    
    public static class GridCellIntersections
    {
        public static List<CellIntersectionData> GetCellIntersectionsWithLineSegment(
            (int x, int y) start, (int x, int y) end)
        {
            var intersectedCells = new List<CellIntersectionData>();

            float startX = start.x;
            float startY = start.y;
            float endX = end.x;
            float endY = end.y;

            if (startX == endX && startY == endY)
            {
                intersectedCells.Add(new CellIntersectionData(start.x, start.y, (startX, startY), (endX, endY)));
                return intersectedCells;
            }
            
            float dx = endX - startX;
            float dy = endY - startY;

            int currentCellX = (int)Math.Floor(startX + 0.5f);
            int currentCellY = (int)Math.Floor(startY + 0.5f);
            
            int endCellX = (int)Math.Floor(endX + 0.5f);
            int endCellY = (int)Math.Floor(endY + 0.5f);

            int stepX = Math.Sign(dx);
            int stepY = Math.Sign(dy);

            float tDeltaX = (dx == 0) ? float.PositiveInfinity : Math.Abs(1.0f / dx);
            float tDeltaY = (dy == 0) ? float.PositiveInfinity : Math.Abs(1.0f / dy);
            
            float nextVerticalBoundary = (stepX > 0) ? (float)Math.Floor(startX + 0.5f) + 0.5f : (float)Math.Floor(startX - 0.5f) + 0.5f;
            float nextHorizontalBoundary = (stepY > 0) ? (float)Math.Floor(startY + 0.5f) + 0.5f : (float)Math.Floor(startY - 0.5f) + 0.5f;

            float tMaxX = (dx == 0) ? float.PositiveInfinity : (nextVerticalBoundary - startX) / dx;
            float tMaxY = (dy == 0) ? float.PositiveInfinity : (nextHorizontalBoundary - startY) / dy;

            (float, float) lastIntersection = (startX, startY);

            while (currentCellX != endCellX || currentCellY != endCellY)
            {
                if (tMaxX < tMaxY)
                {
                    float t = tMaxX;
                    if (t > 1.0f) break;

                    float intersectX = startX + t * dx;
                    float intersectY = startY + t * dy;
                    
                    intersectedCells.Add(new CellIntersectionData(currentCellX, currentCellY, lastIntersection, (intersectX, intersectY)));
                    lastIntersection = (intersectX, intersectY);
                    
                    currentCellX += stepX;
                    tMaxX += tDeltaX;
                }
                else
                {
                    float t = tMaxY;
                    if (t > 1.0f) break;
                    
                    float intersectX = startX + t * dx;
                    float intersectY = startY + t * dy;

                    intersectedCells.Add(new CellIntersectionData(currentCellX, currentCellY, lastIntersection, (intersectX, intersectY)));
                    lastIntersection = (intersectX, intersectY);

                    currentCellY += stepY;
                    tMaxY += tDeltaY;
                }
            }
            
            intersectedCells.Add(new CellIntersectionData(endCellX, endCellY, lastIntersection, (endX, endY)));

            return intersectedCells;
        }
    }
}
