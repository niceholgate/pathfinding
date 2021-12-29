using System;
using System.IO;
using System.Collections.Generic;
using utils;

namespace A_star_stolen
{

    public interface WeightedGraph<L>
    {
        double Cost(Location a, Location b);
        IEnumerable<Location> Neighbors(Location id);
    }

    public struct Location
    {
        // Implementation notes: I am using the default Equals but it can
        // be slow. You'll probably want to override both Equals and
        // GetHashCode in a real project.
        public readonly int x, y;
        public Location(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class SquareGrid : WeightedGraph<Location>
    {
        // Implementation notes: I made the fields public for convenience,
        // but in a real project you'll probably want to follow standard
        // style and make them private.

        public static readonly Location[] DIRS = new[]
            {
            new Location(1, 0),
            new Location(0, -1),
            new Location(-1, 0),
            new Location(0, 1)
        };

        public int width, height;
        public HashSet<Location> walls = new HashSet<Location>();
        public HashSet<Location> forests = new HashSet<Location>();

        public SquareGrid(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public bool InBounds(Location id)
        {
            return 0 <= id.x && id.x < width
                && 0 <= id.y && id.y < height;
        }

        public bool Passable(Location id)
        {
            return !walls.Contains(id);
        }

        public double Cost(Location a, Location b)
        {
            return forests.Contains(b) ? 5 : 1;
        }

        public IEnumerable<Location> Neighbors(Location id)
        {
            foreach (var dir in DIRS)
            {
                Location next = new Location(id.x + dir.x, id.y + dir.y);
                if (InBounds(next) && Passable(next))
                {
                    yield return next;
                }
            }
        }
    }

    public class PriorityQueue<T>
    {
        // I'm using an unsorted array for this example, but ideally this
        // would be a binary heap. There's an open issue for adding a binary
        // heap to the standard C# library: https://github.com/dotnet/corefx/issues/574
        //
        // Until then, find a binary heap class:
        // * https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
        // * http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
        // * http://xfleury.github.io/graphsearch.html
        // * http://stackoverflow.com/questions/102398/priority-queue-in-net

        private List<Tuple<T, double>> elements = new List<Tuple<T, double>>();

        public int Count
        {
            get { return elements.Count; }
        }

        public void Enqueue(T item, double priority)
        {
            elements.Add(Tuple.Create(item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Item2 < elements[bestIndex].Item2)
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Item1;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }

    public class AStarSearch
    {
        public Dictionary<Location, Location> cameFrom
            = new Dictionary<Location, Location>();
        public Dictionary<Location, double> costSoFar
            = new Dictionary<Location, double>();

        // Note: a generic version of A* would abstract over Location and
        // also Heuristic
        static public double Heuristic(Location a, Location b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        public AStarSearch(WeightedGraph<Location> graph, Location start, Location goal)
        {
            var frontier = new PriorityQueue<Location>();
            frontier.Enqueue(start, 0);

            cameFrom[start] = start;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goal))
                {
                    break;
                }

                foreach (var next in graph.Neighbors(current))
                {
                    double newCost = costSoFar[current]
                        + graph.Cost(current, next);
                    if (!costSoFar.ContainsKey(next)
                        || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        double priority = newCost + Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
        }
    }

    public class Test
    {
        static void DrawGrid(SquareGrid grid, AStarSearch astar)
        {
            // Print out the cameFrom array
            for (var y = 0; y < 10; y++)
            {
                for (var x = 0; x < 10; x++)
                {
                    Location id = new Location(x, y);
                    Location ptr = id;
                    if (!astar.cameFrom.TryGetValue(id, out ptr))
                    {
                        ptr = id;
                    }
                    if (grid.walls.Contains(id)) { Console.Write("##"); }
                    else if (ptr.x == x + 1) { Console.Write("\u2192 "); }
                    else if (ptr.x == x - 1) { Console.Write("\u2190 "); }
                    else if (ptr.y == y + 1) { Console.Write("\u2193 "); }
                    else if (ptr.y == y - 1) { Console.Write("\u2191 "); }
                    else { Console.Write("* "); }
                }
                Console.WriteLine();
            }
        }

        //static void Main()
        //{
        //    // Make "diagram 4" from main article
        //    var grid = new SquareGrid(10, 10);
        //    for (var x = 1; x < 4; x++)
        //    {
        //        for (var y = 7; y < 9; y++)
        //        {
        //            grid.walls.Add(new Location(x, y));
        //        }
        //    }
        //    grid.forests = new HashSet<Location>
        //    {
        //        new Location(3, 4), new Location(3, 5),
        //        new Location(4, 1), new Location(4, 2),
        //        new Location(4, 3), new Location(4, 4),
        //        new Location(4, 5), new Location(4, 6),
        //        new Location(4, 7), new Location(4, 8),
        //        new Location(5, 1), new Location(5, 2),
        //        new Location(5, 3), new Location(5, 4),
        //        new Location(5, 5), new Location(5, 6),
        //        new Location(5, 7), new Location(5, 8),
        //        new Location(6, 2), new Location(6, 3),
        //        new Location(6, 4), new Location(6, 5),
        //        new Location(6, 6), new Location(6, 7),
        //        new Location(7, 3), new Location(7, 4),
        //        new Location(7, 5)
        //    };

        //    // Run A*
        //    var astar = new AStarSearch(grid, new Location(1, 4),
        //                                new Location(8, 5));

        //    DrawGrid(grid, astar);
        //}
    }
  

    public class Graph<Location>
    {
        public Dictionary<Location, Location[]> edges
            = new Dictionary<Location, Location[]>();

        public Location[] Neighbors(Location id)
        {
            return edges[id];
        }
    };

    class A_star
    {
        static void Main(string[] args)
        {
            List<int[]> table = CSVReader.ReadRectangularIntCSV(@"E:\dev\python_projects\pathfinding\csv_mazes\walls1.csv");
        }

    }

   
}
