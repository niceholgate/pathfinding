using System.Collections.Generic;
using System.IO;
using FibonacciHeap;
using NicUtils;

namespace AStarNickNS
{

    public class AStarSolver<TPlace, TCoord> : DijkstraSolver<TPlace, TCoord>
        where TPlace : class, IPlaceAStar<TCoord>
    {
        // private readonly PlaceGraph<TCoord> _graph;

        public AStarSolver(PlaceGraph<TCoord> graph) : base(graph)
        {
            
        }
        
        protected override void UpdateFrontier(
            FibonacciHeap<TPlace, double> frontier,
            TPlace neighbour,
            double newCostForNeighbour,
            TPlace target)
        {
            frontier.Insert(new FibonacciHeapNode<TPlace, double>(neighbour, 
                newCostForNeighbour + neighbour.DistanceFrom(target, Distances2D.HeuristicType.Euclidian)));
        }

    }
}