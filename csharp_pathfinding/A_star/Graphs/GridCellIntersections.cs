using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace AStarNickNS
{
    public readonly struct CellIntersectionData
    {
        public readonly int x, y;
        public readonly (double, double) int1, int2;

        public CellIntersectionData(int x, int y, (double, double) int1, (double, double) int2)
        {
            this.x = x;
            this.y = y;
            this.int1 = int1;
            this.int2 = int2;
        }

        public double IntersectedDistance
        {
            get
            {
                double dx = int1.Item1 - int2.Item1;
                double dy = int1.Item2 - int2.Item2;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }
    }

    // public class Line
    // {
    //     public float M { get; }
    //     public float C { get; }
    //     
    //     public Line(float m, float c)
    //     {
    //         M = m;
    //         C = c;
    //     }
    //     
    //     public Line((int x, int y) start, (int x, int y) end)
    //     {
    //         if (end.x == start.x)
    //         {
    //             M = float.PositiveInfinity; // Vertical line
    //             C = start.x; // Store x-intercept
    //         }
    //         else
    //         {
    //             M = (float)(end.y - start.y) / (end.x - start.x);
    //             C = start.y - M * start.x;
    //         }
    //     }
    //
    //     public float X(float y)
    //     {
    //         if (float.IsPositiveInfinity(M)) return C; // Vertical line
    //         if (M == 0) throw new InvalidOperationException("Cannot calculate Y for a horizontal line.");
    //         return (y - C) / M;
    //     }
    //     
    //     public float Y(float x)
    //     {
    //         if (float.IsPositiveInfinity(M)) throw new InvalidOperationException("Cannot calculate Y for a vertical line.");
    //         return M * x + C;
    //     }
    // }
    
    /*
      Uses a grid traversal algorithm, which is a variant of the Amanatides and Woo algorithm commonly used in ray tracing for finding voxel intersections. Its purpose is
      to determine every grid cell that a line segment passes through, from a start cell to an end cell.

      1. Initialization
       - Direction and Steps: It first calculates the direction of the line (dx, dy) and determines the direction of traversal for each axis (signDx, signDy), which will be either +1 or -1.
       - Start and End Cells: It sets the starting cell (currentCellX, currentCellY) and the target end cell (endCellX, endCellY).
       - t-values: This is the core of the algorithm. It uses a parametric representation of the line P(t) = start + t * direction.
           - tDeltaX and tDeltaY: These values represent how far you must move along the line (in terms of t) to cross one full grid cell in the X or Y direction, respectively. It's calculated from the inverse of the
             line's direction vector components.
           - tMaxX and tMaxY: These values represent the total distance (in terms of t) from the start of the line to the next grid line crossing in the X or Y direction.

      2. The Main Traversal Loop
      The algorithm proceeds in a loop, stepping from one cell to the next until it reaches the end cell.

       - The Decision: In each iteration, it compares tMaxX and tMaxY.
           - If tMaxX < tMaxY, it means the line will hit a vertical grid line first. Therefore, the next cell to enter is in the X direction. The algorithm updates currentCellX and increments tMaxX by tDeltaX (to set
             it up for the next vertical crossing).
           - If tMaxY < tMaxX, it means the line will hit a horizontal grid line first. The next cell is in the Y direction, so it updates currentCellY and increments tMaxY by tDeltaY.
       - Corner Case: If tMaxX and tMaxY are nearly equal, it means the line passes very close to a corner of a cell. In this implementation, it records intersections with the three cells that touch that corner before
         moving diagonally to the next cell.

      3. Recording Intersections
       - With each step, the algorithm calculates the precise intersection point on the grid line that is being crossed.
       - It creates a CellIntersectionData object for the cell it is leaving. This object stores the cell's coordinates (currentCellX, currentCellY), the entry point (lastIntersection), and the exit point (the newly
         calculated intersection point).
       - The lastIntersection is then updated to the current exit point, which becomes the entry point for the next cell in the path.

      4. Termination
       - The loop continues until currentCellX and currentCellY match the end cell's coordinates.
       - A t > 1.0f check ensures the traversal doesn't go beyond the bounds of the line segment (where t=0 is the start and t=1 is the end).
       - Finally, after the loop, it adds the last segment, which is from the final grid line crossing to the actual end point of the line, and associates it with the end cell.

      In summary, the algorithm efficiently "walks" along the line from cell to cell by repeatedly calculating which grid boundary (vertical or horizontal) is closer and taking a step in that direction.
     */
    public static class GridCellIntersections
    {
        public static List<CellIntersectionData> GetCellIntersectionsWithLineSegment(
            (double x, double y) start, (double x, double y) end)
        {
            var intersectedCells = new List<CellIntersectionData>();
            
            int currentCellX = (int)Math.Round(start.x);
            int currentCellY = (int)Math.Round(start.y);
            
            int endCellX = (int)Math.Round(end.x);
            int endCellY = (int)Math.Round(end.y);

            // Handle the case where start and end are the same point
            if (currentCellX == endCellX && currentCellY == endCellY)
            {
                intersectedCells.Add(new CellIntersectionData(currentCellX, currentCellY, (start.x, start.y), (end.x, end.y)));
                return intersectedCells;
            }
            
            double dx = end.x - start.x;
            double dy = end.y - start.y;

            int signDx = Math.Sign(dx);
            int signDy = Math.Sign(dy);
            
            //  parametric representation of the line P(t) = start + t * direction
            
            // how far you must move along the line (in terms of t) to cross one full grid cell in the X or Y direction
            double tDeltaX = (dx == 0) ? float.PositiveInfinity : Math.Abs(1.0f / dx);
            double tDeltaY = (dy == 0) ? float.PositiveInfinity : Math.Abs(1.0f / dy);

            // the total distance (in terms of t) from the start of the line to the next grid line crossing in the X or Y direction
            double tMaxX;
            if (dx == 0)
            {
                tMaxX = double.PositiveInfinity;
            }
            else
            {
                double nextBoundaryX = currentCellX + signDx * 0.5f;
                tMaxX = (nextBoundaryX - start.x) / dx;
            }

            double tMaxY;
            if (dy == 0)
            {
                tMaxY = double.PositiveInfinity;
            }
            else
            {
                double nextBoundaryY = currentCellY + signDy * 0.5f;
                tMaxY = (nextBoundaryY - start.y) / dy;
            }

            (double, double) lastIntersection = (start.x, start.y);

            float epsilon = 1e-6f;
            while (currentCellX != endCellX || currentCellY != endCellY)
            {
                // the line will hit a vertical grid line first
                if (tMaxX < tMaxY - epsilon)
                {
                    if (tMaxX > 1.0f) break;

                    double intersectX = start.x + tMaxX * dx;
                    double intersectY = start.y + tMaxX * dy;
                    
                    intersectedCells.Add(new CellIntersectionData(currentCellX, currentCellY, lastIntersection, (intersectX, intersectY)));
                    lastIntersection = (intersectX, intersectY);
                    
                    currentCellX += signDx;
                    tMaxX += tDeltaX;
                }
                // the line will hit a horizontal grid line first
                else if (tMaxY < tMaxX - epsilon)
                {
                    if (tMaxY > 1.0f) break;
                    
                    double intersectX = start.x + tMaxY * dx;
                    double intersectY = start.y + tMaxY * dy;

                    intersectedCells.Add(new CellIntersectionData(currentCellX, currentCellY, lastIntersection, (intersectX, intersectY)));
                    lastIntersection = (intersectX, intersectY);

                    currentCellY += signDy;
                    tMaxY += tDeltaY;
                }
                //  If tMaxX and tMaxY are nearly equal, it means the line passes very close to a corner of a cell
                else
                {
                    if (tMaxX > 1.0f) break;

                    double intersectX = start.x + tMaxX * dx;
                    double intersectY = start.y + tMaxX * dy;
                    
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
            
            intersectedCells.Add(new CellIntersectionData(endCellX, endCellY, lastIntersection, (end.x, end.y)));

            return intersectedCells;
        }
    }
}
