using FibonacciHeap;
using System.Collections.Generic;
using System.IO;

namespace AStarNickNS
{
    // Dijkstra is a generic solver for any coordinate type
    public class DijkstraSolver<TPlace, TCoord> : IPathfindingSolver<TPlace, TCoord>
        where TPlace : class, IPlace<TCoord>
    {
        private readonly PlaceGraph<TCoord> _graph;
        
        public DijkstraSolver(PlaceGraph<TCoord> graph)
        {
            _graph = graph;
        }
        
        public IEnumerable<TPlace> SolvePath(IPlace<TCoord> start, IPlace<TCoord> target, double pathfinderSize=0.9)
        {
            _graph.CheckDisjoint();
            if (!_graph.Places.ContainsKey(start.Label))
            {
                throw new IOException($"The start place (\"{start.Label}\") is not on the graph!");
            }

            if (!_graph.Places.ContainsKey(target.Label))
            {
                throw new IOException($"The target place (\"{target.Label}\") is not on the graph!");
            }

            FibonacciHeap<TPlace, double> frontier = new(0);
            frontier.Insert(new FibonacciHeapNode<TPlace, double>((TPlace)start, 0));
            Dictionary<TPlace, TPlace> cameFrom = new();
            Dictionary<TPlace, double> costSoFar = new() { { (TPlace)start, 0.0 } };
            
            var visited = new HashSet<TPlace>();
            
            while (!frontier.IsEmpty())
            {
                TPlace current = frontier.RemoveMin().Data;
                if (!visited.Add(current)) continue;
                if (current.Equals(target)) break;
                
                foreach (TPlace neighbour in current.Neighbours)
                {
                    if (_graph.IsBlocked(current.Label, neighbour.Label, pathfinderSize)) continue;
                    
                    double newCostForNeighbour = costSoFar[current] + current.CostToLeave(neighbour, _graph);

                    if (costSoFar.TryGetValue(neighbour, out double neighbourCostSoFar) &&
                        newCostForNeighbour >= neighbourCostSoFar) continue;
                    
                    costSoFar[neighbour] = newCostForNeighbour;
                    cameFrom[neighbour] = current;
                    UpdateFrontier(frontier, neighbour, newCostForNeighbour, (TPlace)target);
                }
            }
            
            return ReconstructPath(start, target, cameFrom);
        }

        protected virtual void UpdateFrontier(
            FibonacciHeap<TPlace, double> frontier,
            TPlace neighbour,
            double newCostForNeighbour,
            TPlace target)
        {
            frontier.Insert(new FibonacciHeapNode<TPlace, double>(neighbour, newCostForNeighbour));
        }

        private IEnumerable<TPlace> ReconstructPath(IPlace<TCoord> start, IPlace<TCoord> target, Dictionary<TPlace, TPlace> cameFrom)
        {
            TPlace current = (TPlace)target;
            List<TPlace> path = new List<TPlace>();
            while (cameFrom.ContainsKey(current) && !current.Equals(start))
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Add((TPlace)start);
            path.Reverse();
            return path;
        }
    }
}