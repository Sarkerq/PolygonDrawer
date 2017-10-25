using System;
using System.Windows;

namespace GK1
{
    public class Polar
    {
        Point origin;
        double radius;
        double angle;

        public Polar(Point o, double r, double a)
        {
            origin = o;
            radius = r;
            angle = a;
        }

        public Point toCartesian()
        {
            return new Point(origin.X + radius * Math.Cos(angle), origin.Y + radius * Math.Sin(angle));
        }
    }
}