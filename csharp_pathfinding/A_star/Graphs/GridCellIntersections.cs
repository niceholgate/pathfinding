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
    
    // TODO: understand this, since it's from Gemini
    public static class GridCellIntersections
    {
        public static List<CellIntersectionData> GetCellIntersectionsWithLineSegment(
            (int x, int y) start, (int x, int y) end)
        {
            var intersectedCells = new List<CellIntersectionData>();

            // Get start and end as floats
            float startX = start.x;
            float startY = start.y;
            float endX = end.x;
            float endY = end.y;

            // Handle the case where start and end are the same point
            if (start.x == end.x && start.y == end.y)
            {
                intersectedCells.Add(new CellIntersectionData(start.x, start.y, (startX, startY), (endX, endY)));
                return intersectedCells;
            }
            
            float dx = endX - startX;
            float dy = endY - startY;

            int signDx = Math.Sign(dx);
            int signDy = Math.Sign(dy);
            
            int currentCellX = start.x;
            int currentCellY = start.y;
            
            int endCellX = end.x;
            int endCellY = end.y;

            float tDeltaX = (dx == 0) ? float.PositiveInfinity : Math.Abs(1.0f / dx);
            float tDeltaY = (dy == 0) ? float.PositiveInfinity : Math.Abs(1.0f / dy);

            float tMaxX = dx == 0 ? float.PositiveInfinity : signDx * 0.5f / dx;
            float tMaxY = dy == 0 ? float.PositiveInfinity : signDy * 0.5f / dy;

            (float, float) lastIntersection = (startX, startY);

            float epsilon = 1e-6f;
            while (currentCellX != endCellX || currentCellY != endCellY)
            {
                if (tMaxX < tMaxY - epsilon)
                {
                    if (tMaxX > 1.0f) break;

                    float intersectX = startX + tMaxX * dx;
                    float intersectY = startY + tMaxX * dy;
                    
                    intersectedCells.Add(new CellIntersectionData(currentCellX, currentCellY, lastIntersection, (intersectX, intersectY)));
                    lastIntersection = (intersectX, intersectY);
                    
                    currentCellX += signDx;
                    tMaxX += tDeltaX;
                }
                else if (tMaxY < tMaxX - epsilon)
                {
                    if (tMaxY > 1.0f) break;
                    
                    float intersectX = startX + tMaxY * dx;
                    float intersectY = startY + tMaxY * dy;

                    intersectedCells.Add(new CellIntersectionData(currentCellX, currentCellY, lastIntersection, (intersectX, intersectY)));
                    lastIntersection = (intersectX, intersectY);

                    currentCellY += signDy;
                    tMaxY += tDeltaY;
                }
                else // Corner case
                {
                    // tMaxX is approx tMaxY
                    if (tMaxX > 1.0f) break;

                    float intersectX = startX + tMaxX * dx;
                    float intersectY = startY + tMaxX * dy;
                    
                    // Current cell
                    intersectedCells.Add(new CellIntersectionData(currentCellX, currentCellY, lastIntersection, (intersectX, intersectY)));
                    
                    // Add the other 2 cells that share the corner. The 4th one will be the next current cell.
                    // The intersection with these cells is just the corner point.
                    intersectedCells.Add(new CellIntersectionData(currentCellX, currentCellY + signDy, (intersectX, intersectY), (intersectX, intersectY)));
                    intersectedCells.Add(new CellIntersectionData(currentCellX + signDx, currentCellY, (intersectX, intersectY), (intersectX, intersectY)));

                    lastIntersection = (intersectX, intersectY);
                    
                    currentCellX += signDx;
                    currentCellY += signDy;
                    tMaxX += tDeltaX;
                    tMaxY += tDeltaY;
                }
            }
            
            intersectedCells.Add(new CellIntersectionData(endCellX, endCellY, lastIntersection, (endX, endY)));

            return intersectedCells;
        }
    }
}
