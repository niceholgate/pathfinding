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
        private TPlace _current;
        private double _newCostForNeighbour;
        private Dictionary<TPlace, TPlace> _cameFrom;
        private Dictionary<TPlace, double> _costSoFar;
        private bool _hasRun = false;
        private bool _foundPath = false;

        public DijkstraSolver(PlaceGraph<TCoord> graph)
        {
            _graph = graph;
        }

        public void Solve(IPlace<TCoord> start, IPlace<TCoord> target)
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

            _hasRun = true;
            FibonacciHeap<TPlace, double> frontier = new(0);
            frontier.Insert(new FibonacciHeapNode<TPlace, double>((TPlace)start, 0));
            _cameFrom = new Dictionary<TPlace, TPlace> { { (TPlace)start, null } };
            _costSoFar = new Dictionary<TPlace, double> { { (TPlace)start, 0.0 } };

            while (!frontier.IsEmpty())
            {
                _current = frontier.RemoveMin().Data;
                if (_current.Equals(target))
                {
                    _foundPath = true;
                    break;
                }

                foreach (TPlace neighbour in _current.Neighbours)
                {
                    if (!_graph.IsBlocked(_current.Label, neighbour.Label))
                    {
                        _newCostForNeighbour = _costSoFar[_current] + _current.CostToLeave(neighbour, _graph);
                        if (!_costSoFar.TryGetValue(neighbour, out double neighbourCostSoFar)
                            || _newCostForNeighbour < neighbourCostSoFar)
                        {
                            _costSoFar[neighbour] = _newCostForNeighbour;
                            frontier.Insert(new FibonacciHeapNode<TPlace, double>(neighbour, _newCostForNeighbour));
                            _cameFrom[neighbour] = _current;
                        }
                    }
                }
            }
        }

        public IEnumerable<TPlace> ReconstructPath(IPlace<TCoord> start, IPlace<TCoord> target)
        {
            if (_hasRun && _foundPath)
            {
                _current = (TPlace)target;
                List<TPlace> path = new List<TPlace>();
                while (!_current.Equals(start))
                {
                    path.Add(_current);
                    _current = _cameFrom[_current];
                }

                path.Add((TPlace)start);
                path.Reverse();
                return path;
            }

            return null;
        }
    }
}