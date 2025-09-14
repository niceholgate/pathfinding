using System.Collections.Generic;
using System.IO;
using FibonacciHeap;
using NicUtils;

namespace AStarNickNS
{
    // TODO: refactor to avoid code repetition with Dijkstra?
    // AStar relies upon a grid with a coordinate type which supports heuristic distances between any two places
    public class AStarSolver<TPlace, TCoord> : IPathfindingSolver<TPlace, TCoord>
        where TPlace : class, IPlaceAStar<TCoord>
    {
        private readonly PlaceGraph<TCoord> _graph;
        
        public AStarSolver(PlaceGraph<TCoord> graph)
        {
            _graph = graph;
        }

        public IEnumerable<TPlace> SolvePath(IPlace<TCoord> start, IPlace<TCoord> target)
        {
            _graph.CheckDisjoint();
            if (!_graph.Places.ContainsKey(start.Label))
            {
                throw new IOException($"The start place ({start.Label}) is not on the graph!");
            }

            if (!_graph.Places.ContainsKey(target.Label))
            {
                throw new IOException($"The target place (\"{target.Label}\") is not on the graph!");
            }

            FibonacciHeap<TPlace, double> frontier = new(0);
            frontier.Insert(new FibonacciHeapNode<TPlace, double>((TPlace)start, 0));
            Dictionary<TPlace, TPlace> cameFrom = new Dictionary<TPlace, TPlace>();
            Dictionary<TPlace, double> costSoFar = new Dictionary<TPlace, double> { { (TPlace)start, 0.0 } };

            var closed = new HashSet<TPlace>();
            
            while (!frontier.IsEmpty())
            {
                TPlace current = frontier.RemoveMin().Data;
                if (!closed.Add(current)) continue;
                if (current.Equals(target)) break;

                foreach (TPlace neighbour in current.Neighbours)
                {
                    if (_graph.IsBlocked(current.Label, neighbour.Label)) continue;
                    
                    double newCostForNeighbour = costSoFar[current] + current.CostToLeave(neighbour, _graph);

                    if (costSoFar.TryGetValue(neighbour, out double neighbourCostSoFar) &&
                        newCostForNeighbour >= neighbourCostSoFar) continue;
                    
                    costSoFar[neighbour] = newCostForNeighbour;
                    cameFrom[neighbour] = current;
                    frontier.Insert(new FibonacciHeapNode<TPlace, double>(neighbour,
                        newCostForNeighbour +
                        neighbour.DistanceFrom((TPlace)target, Distances2D.HeuristicType.Euclidian)));
                }
            }

            return ReconstructPath(start, target, cameFrom);
        }

        private IEnumerable<TPlace> ReconstructPath(IPlace<TCoord> start, IPlace<TCoord> target, Dictionary<TPlace, TPlace> cameFrom)
        {
            TPlace current = (TPlace)target;
            List<TPlace> path = new List<TPlace>();
            while (!current.Equals(start))
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