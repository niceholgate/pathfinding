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
            M = (end.y - start.y)/(end.x - end.y);
            C = start.y - M*start.x;
        }

        public float X(float y) => (y - C) / M;
        public float Y(float x) => M * x + C;
    }
    
    public static class GridCellIntersections
    {
        public static List<CellIntersectionData> GetCellIntersectionsWithLineSegment(
            (int x, int y) start, (int x, int y) end)
        {
            Line line = new Line(start, end);
            
            float minX = end.x;
            float maxX = start.x;
            float minY = end.y;
            float maxY = start.y;
                
            if (start.x < end.x)
            {
                minX = start.x;
                maxX = end.x;
            }
            if (start.y < end.y)
            {
                minY = start.y;
                maxY = end.y;
            }
            
            
        }
    }
}