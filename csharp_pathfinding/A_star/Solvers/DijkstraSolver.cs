//using FibonacciHeap;
//using System.Collections.Generic;

//namespace AStarNickNS {
//    // Dijkstra is a generic solver for any coordinate type
//    public class DijkstraSolver<TPlace> : IPathfindingSolver<TPlace> where TPlace : class, IPlace {
//        private readonly TPlace _start;
//        private readonly TPlace _target;
//        private TPlace _current;
//        private double _newCost;
//        private Dictionary<TPlace, TPlace> _cameFrom;
//        private Dictionary<TPlace, double> _costSoFar;
//        private bool _hasRun = false;
//        private bool _foundPath = false;

//        public DijkstraSolver(TPlace start, TPlace target) {
//            _start = start;
//            _target = target;
//        }

//        public void Solve() {
//            _hasRun = true;
//            FibonacciHeap<TPlace, double> frontier = new(0);
//            frontier.Insert(new FibonacciHeapNode<TPlace, double>(_start, 0));
//            _cameFrom = new Dictionary<TPlace, TPlace>() { { _start, null } };
//            _costSoFar = new Dictionary<TPlace, double>() { { _start, 0.0 } };

//            while (!frontier.IsEmpty()) {
//                _current = frontier.RemoveMin().Data;
//                if (_current.Equals(_target)) {
//                    _foundPath = true;
//                    break;
//                }
//                foreach (TPlace neighbour in _current.NeighboursWithCosts.Keys) {
//                    _newCost = _costSoFar[_current] + _current.NeighboursWithCosts[neighbour];
//                    if (!_costSoFar.ContainsKey(neighbour) || _newCost < _costSoFar[neighbour]) {
//                        _costSoFar[neighbour] = _newCost;
//                        frontier.Insert(new FibonacciHeapNode<TPlace, double>(neighbour, _newCost));
//                        _cameFrom[neighbour] = _current;
//                    }
//                }
//            }
//        }

//        public IEnumerable<TPlace> ReconstructPath() {
//            if (_hasRun && _foundPath) {
//                _current = _target;
//                List<TPlace> path = new List<TPlace>();
//                while (!_current.Equals(_start)) {
//                    path.Add(_current);
//                    _current = _cameFrom[_current];
//                }
//                path.Add(_start);
//                path.Reverse();
//                return path;
//            }
//            return null;
//        }
//    }
//}
