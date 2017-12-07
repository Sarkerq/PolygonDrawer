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

        public static Vertice operator -(Vertice p1, Vertice p2) { return new Vertice(p1.coords.X - p2.coords.X, p1.coords.Y - p2.coords.Y); }



        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static double CrossProduct(Vertice p1, Vertice p2) { return p1.coords.X * p2.coords.Y - p2.coords.X * p1.coords.Y; }
    }
}
