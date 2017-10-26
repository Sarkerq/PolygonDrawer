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
        public const double verticeRadius = 12;
        public const int lineWidth = 3;
        public const int statusRadius = 10;
        public const int statusLineSeparRadius = 3;
        public const int statusLineLengthRadius = 6;

        public const double pixelEpsilon = 0.5;
        public const double angleEpsilon = 0.5 * Math.PI / 180;

        static public double Distance(Vertice v1, Vertice v2)
        {
            return Math.Sqrt(Math.Pow(v1.coords.X - v2.coords.X, 2) + Math.Pow(v1.coords.Y - v2.coords.Y, 2));
        }
        static public double Distance(Point x, Point y)
        {
            return Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));
        }
        static public Polar CartesianToPolar(Point x, Point y)
        {
            return new Polar(x, Distance(x, y), AngleAgainstXAxis(x, y));
        }
        static public double AngleAgainstXAxis(Point x, Point y)
        {
            Vector yRelative = y - x;
            return Math.Atan2(yRelative.Y, yRelative.X);
        }
        static public double GetAngle(Vertice left, Vertice target, Vertice right)
        {
            return Global.AngleAgainstXAxis(target.coords, left.coords) - Global.AngleAgainstXAxis(target.coords, right.coords);
        }

        internal static Point Mirror(Vertice target, Edge tmp)
        {
            Polar pol = new Polar(tmp.v1.coords, Distance(target, tmp.v1),  AngleAgainstXAxis(tmp.v1.coords,tmp.v2.coords) - GetAngle(target, tmp.v1, tmp.v2));
            return pol.toCartesian();
        }
    }
}
