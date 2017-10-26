using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GK1
{
    static class Global
    {
        public const double verticeRadius = 9;
        public const int lineWidth = 3;
        static public double distance(Vertice v1, Vertice v2)
        {
            return Math.Sqrt(Math.Pow(v1.coords.X - v2.coords.X, 2) + Math.Pow(v1.coords.Y - v2.coords.Y, 2));
        }
        static public double distance(Point x, Point y)
        {
            return Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));
        }
        static public Polar cartesianToPolar(Point x, Point y)
        {
            return new Polar(x, distance(x, y), angleAgainstXAxis(x, y));
        }
        static public double angleAgainstXAxis(Point x, Point y)
        {
            Vector yRelative = y - x;
            return Math.Atan2(yRelative.Y, yRelative.X);
        }
    }
}
