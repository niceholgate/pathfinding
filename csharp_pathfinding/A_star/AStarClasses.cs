using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using utils;

namespace AStarNickNS
{
    public interface IPlace<LabelType>
    {
        bool IsNeighbour(IPlace<LabelType> neighbour);
        LabelType GetLabel();
    }

    public interface INode<CoordType>
    {
        double GetCostToLeave(INode<CoordType> neighbour);

        CoordType Coord { get; set; }
    }

    public interface INodeAStar<CoordType> : INode<CoordType>
    {
        double GetHeuristicDist(INodeAStar<CoordType> other, string heuristicType);
    }

    public struct GenericPlace : IPlace<string>
    {
        public GenericPlace(string name, HashSet<IPlace<string>> neighbours)
        {
            Name = name;
            Neighbours = neighbours;
        }

        public string Name { get; }

        public string GetLabel() { return Name; }

        public HashSet<IPlace<string>> Neighbours { get; set; }

        public bool IsNeighbour(IPlace<string> other) { return Neighbours.Contains(other); }

        public override string ToString() => Name;
    }

    public struct GridCoords2D : IPlace<int[]>
    {
        public GridCoords2D(int x, int y)
        {
            X = x;
            Y = y;
        }
        // TODO: is it possible to set the X and Y publically with below?
        public int X { get; }
        public int Y { get; }
        public int[] GetLabel() { return new int[2] { X, Y }; }
        public bool IsDiagonalNeighbour(IPlace<int[]> other) 
        {
            int[] delta = DeltaFrom(other);
            return Math.Pow(delta[0], 2) + Math.Pow(delta[1], 2) == 2;
        }
        public bool IsStraightNeighbour(IPlace<int[]> other)
        {
            int[] delta = DeltaFrom(other);
            return Math.Abs(delta[0]) + Math.Abs(delta[1]) == 1;
        }
        public bool IsNeighbour(IPlace<int[]> other) { return IsDiagonalNeighbour(other) || IsStraightNeighbour(other); }
        public int[] DeltaFrom(IPlace<int[]> other) { return new int[] { this.X - other.GetLabel()[0], this.Y - other.GetLabel()[1] }; }
        public override string ToString() => $"({X}, {Y})";
    }

    public class Node<CoordType> : INode<CoordType>
    {
        public Node(CoordType coord) { this.Coord = coord; }
        public CoordType Coord { get; set; }
        protected Dictionary<INode<CoordType>, double> neighboursCosts { get; set; }
        public Dictionary<INode<CoordType>, double> GetNeighboursCosts() { return this.neighboursCosts; }
        public virtual double GetCostToLeave(INode<CoordType> neighbour) { return neighboursCosts[neighbour]; }
    }

    class Square : Node<GridCoords2D>, INodeAStar<GridCoords2D>
    {
        public static readonly double SQRT2 = Math.Sqrt(2.0);

        public Square(GridCoords2D coord) : base(coord) { }

        public double GetCostToLeave(Square neighbour)
        {
            double distance = neighbour.IsDiagonalNeighbour(this) ? SQRT2 : 1.0;
            return neighboursCosts[neighbour] * distance;
        }

        private bool IsDiagonalNeighbour(Square other) { return Coord.IsDiagonalNeighbour(other.Coord); }

        public double GetHeuristicDist(INodeAStar<GridCoords2D> other, string heuristicType)
        {
            double[] thisLabelAsDoubles = Array.ConvertAll(Coord.GetLabel(), el => (double) el);
            double[] otherLabelAsDoubles = Array.ConvertAll(other.Coord.GetLabel(), el => (double)el);
            return Distances2D.GetDistance(thisLabelAsDoubles, otherLabelAsDoubles, heuristicType);
        }

    }
    //class Hex : Node, INodeAStar
    //{
    //    interface_method(Hex hex)
    //}

    class A_star
    {
        static void Main(string[] args)
        {
            List<int[]> listInt = new List<int[]> { new int[] { 1, 2 }, new int[] { 0, -1 }, new int[] { 0, 1 }, new int[] { 1, 0 } };
            int[] searchArrayInt = { 1, 2 };

            List<double[]> listDouble = new List<double[]> { new double[] { 1, 2 }, new double[] { 0, -1 }, new double[] { 0, 1 }, new double[] { 1, 0 } };
            double[] searchArrayDouble = { 1.1, 2.0 };

            List<int>[] arrayInt = new List<int>[] { new List<int> { 1, 2 }, new List<int> { 0, -1 }, new List<int> { 0, 1 }, new List<int> { 1, 0 } };
            List<int> searchListInt = new List<int> { 1, 2 };

            bool cont = NickEnumerables<int>.ContainsEnumerable(listInt, searchArrayInt);
            bool cont2 = NickEnumerables<double>.ContainsEnumerable(listDouble, searchArrayDouble);
            bool cont3 = NickEnumerables<int>.ContainsEnumerable(arrayInt, searchListInt);

            double d = Distances2D.GetDistance(new double[2] { 3.0, 1.0 }, new double[2] { 6.0, 5.0 }, "euclidian");
            Console.WriteLine(d);

            Square squareTest1 = new Square(new GridCoords2D(0, 0));
            Square squareTestDiag = new Square(new GridCoords2D(1, 1));
            Square squareTestStra = new Square(new GridCoords2D(1, 0));
            double hd = squareTest1.GetHeuristicDist(squareTestDiag, "euclidian");
            
            var costsDict = new Dictionary<INode<GridCoords2D>, double>();
            costsDict.Add(squareTestDiag, 1);
            costsDict.Add(squareTestStra, 1);
            var costsDict2 = new Dictionary<INode<GridCoords2D>, double> { { squareTestDiag, 2 }, { squareTestStra, 4 } };

            squareTest1.SetNeighboursCosts(costsDict);
            double costDiag = squareTest1.GetCostToLeave(squareTestDiag);
            double costStra = squareTest1.GetCostToLeave(squareTestStra);

            Console.WriteLine(hd);
        }
    }
}
