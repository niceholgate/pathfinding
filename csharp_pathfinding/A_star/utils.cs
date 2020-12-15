using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace utils
{
    class CSVReader
    {
        public List<int[]> ReadRectangularIntCSV(string filepath)
        {
            var reader = new StreamReader(filepath);
            var table = new List<int[]>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                string[] stringValues = line.Split(',');
                int[] intValues = new int[stringValues.Length];
                for (int i = 0; i < stringValues.Length; i++)
                {
                    intValues[i] = int.Parse(stringValues[i]);
                }
                table.Add(intValues);
            }
            reader.Close();
            return table;
        }
    }


    public class NickEnumerables<T>
    {
        public static bool ContainsEnumerable(IEnumerable<IEnumerable<T>> list, IEnumerable<T> searchArray)
        {
            foreach (IEnumerable<T> array in list)
            {
                if (array.SequenceEqual(searchArray)) return true;
            }
            return false;
        }
    }


    public class Distances2D
    {
        public static readonly string[] heuristicTypes = { "euclidian", "octile", "manhattan" };
        public static double GetDistance(double[] coords1, double[] coords2, string heuristicType)
        {
            // Check that the coordinates are both of the same length
            if (coords1.Length != coords2.Length) { throw new ArgumentException($"Coordinates must of the same length, but instead are of lengths {coords1.Length} and {coords2.Length}."); }

            double sum = 0.0;
            if (heuristicType == "euclidian")
            {
                for (int i = 0; i < coords1.Length; i++)
                {
                    sum += Math.Pow((coords1[i] - coords2[i]), 2);
                }
                return Math.Sqrt(sum);
            }
            else if (heuristicType == "octile")
            {
                double dx = Math.Abs(coords1[0] - coords2[0]);
                double dy = Math.Abs(coords1[1] - coords2[1]);
                return dx-dy+Math.Sqrt(2)*Math.Min(dx, dy);
            }
            else if (heuristicType == "manhattan")
            {
                for (int i = 0; i < coords1.Length; i++)
                {
                    sum += Math.Abs(coords1[i] - coords2[i]);
                }
                return sum;
            }
            else
            {
                throw new ArgumentException($"Unknown distance type requested, must be from among: {string.Join(", ", heuristicTypes)}");
            }
        }

    }

}

