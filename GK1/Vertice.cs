using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace GK1
{

    public class Vertice
    {

        public Point coords;
        public Vertice(double x, double y)
        {
            coords = new Point(x, y);
        }
        public Vertice(Point p)
        {
            coords = p;
        }
        public Vertice()
        {
            coords = new Point();
        }
        public Vertice(Vertice v)
        {

            coords = new Point(v.coords.X, v.coords.Y);
        }

        public static Vertice operator +(Vertice p1, Vertice p2) { return new Vertice(p1.coords.X + p2.coords.X, p1.coords.Y + p2.coords.Y); }

        public static Vertice operator -(Vertice p1, Vertice p2) { return new Vertice(p1.coords.X - p2.coords.X, p1.coords.Y - p2.coords.Y); }

        public static bool operator ==(Vertice p1, Vertice p2) { return (p1 is null && p2 is null) || (!(p1 is   null) && !(p2 is null) && Math.Abs(p1.coords.X - p2.coords.X) < double.Epsilon && Math.Abs(p1.coords.Y - p2.coords.Y) < double.Epsilon); }

        public static bool operator !=(Vertice p1, Vertice p2) { return !(p1 == p2); }

        public double this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return coords.X;
                    case 1:
                        return coords.Y;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0:
                        coords.X = value;
                        return;
                    case 1:
                        coords.Y = value;
                        return;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static double CrossProduct(Vertice p1, Vertice p2) { return p1.coords.X * p2.coords.Y - p2.coords.X * p1.coords.Y; }

        public static double DotProduct(Vertice p1, Vertice p2) { return p1.coords.X * p2.coords.X + p1.coords.Y * p2.coords.Y; }

        public static double Distance(Vertice p1, Vertice p2)
        {
            double dx, dy;
            dx = p1.coords.X - p2.coords.X;
            dy = p1.coords.Y - p2.coords.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

}
