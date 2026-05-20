using System;

namespace AStarNickNS
{
    public static class GeometryUtils
    {
        public const float SQRT2 = 1.41421356f;

        public static float GetDistanceToLineSegment(
            (float x, float y) p1,
            (float x, float y) p2,
            (float x, float y) p0)
        {
            (float x1, float y1) = p1;
            (float x2, float y2) = p2;
            (float x0, float y0) = p0;
            
            float dx = x2 - x1;
            float dy = y2 - y1;

            if (dx == 0 && dy == 0)
            {
                return MathF.Sqrt(MathF.Pow(x1 - x0, 2) + MathF.Pow(y1 - y0, 2));
            }

            // Calculate the t parameter of the projection of p3 onto the line segment p1-p2
            // t = [(p3-p1) . (p2-p1)] / |p2-p1|^2
            float t = ((x0 - x1) * dx + (y0 - y1) * dy) / (dx * dx + dy * dy);
            if (t <= 0) return MathF.Sqrt(MathF.Pow(x1 - x0, 2) + MathF.Pow(y1 - y0, 2)); // p0 is closest to p1
            if (t >= 1) return MathF.Sqrt(MathF.Pow(x2 - x0, 2) + MathF.Pow(y2 - y0, 2)); // p0 is closest to p2

            // p0 is closest to the projection on the segment
            float projX = x1 + t * dx;
            float projY = y1 + t * dy;
            return MathF.Sqrt(MathF.Pow(projX - x0, 2) + MathF.Pow(projY - y0, 2));
        }

        public static bool CircleFitsOnBoundary(int diagType, int xFrom, int yFrom, int xTo, int yTo,
            float diameter, bool[,] blockages)
        {
            float radius = diameter / 2.0f;

            if (diagType != 0) // Diagonal
            {
                float vx = (xFrom + xTo) / 2.0f;
                float vy = (yFrom + yTo) / 2.0f;

                int dx = xTo - xFrom;
                int dy = yTo - yFrom;

                // Perpendicular diagonal direction
                int px = dy;
                int py = -dx;

                for (int k = 0; ; k++)
                {
                    float dist = k * SQRT2;
                    if (dist > radius + 1e-6f) break;

                    if (IsGridVertexBlocked(vx + k * px, vy + k * py, blockages)) return false;
                    if (k > 0 && IsGridVertexBlocked(vx - k * px, vy - k * py, blockages)) return false;
                }
            }
            else // Orthogonal
            {
                float mx = (xFrom + xTo) / 2.0f;
                float my = (yFrom + yTo) / 2.0f;

                if (xFrom != xTo) // Horizontal
                {
                    for (int k = 0; ; k++)
                    {
                        float dist = k + 0.5f;
                        if (dist > radius + 1e-6f) break;

                        if (IsGridVertexBlocked(mx, my + dist, blockages)) return false;
                        if (IsGridVertexBlocked(mx, my - dist, blockages)) return false;
                    }
                }
                else // Vertical
                {
                    for (int k = 0; ; k++)
                    {
                        float dist = k + 0.5f;
                        if (dist > radius + 1e-6f) break;

                        if (IsGridVertexBlocked(mx + dist, my, blockages)) return false;
                        if (IsGridVertexBlocked(mx - dist, my, blockages)) return false;
                    }
                }
            }

            return true;
        }

        public static bool IsGridCellBlocked(int x, int y, bool[,] blockages)
        {
            if (x < 0 || x >= blockages.GetLength(0) || y < 0 || y >= blockages.GetLength(1)) return false;
            return blockages[x, y];
        }

        public static bool IsGridVertexBlocked(float vx, float vy, bool[,] blockages)
        {
            int x1 = (int)(vx - 0.5f);
            int x2 = (int)(vx + 0.5f);
            int y1 = (int)(vy - 0.5f);
            int y2 = (int)(vy + 0.5f);

            return IsGridCellBlocked(x1, y1, blockages) || IsGridCellBlocked(x1, y2, blockages)
                || IsGridCellBlocked(x2, y1, blockages) || IsGridCellBlocked(x2, y2, blockages);
        }
    }
}
