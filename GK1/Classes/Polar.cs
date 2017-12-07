using System;
using System.Runtime.CompilerServices;
using System.Windows;
using MathNet.Numerics;
namespace GK1
{
    public class Polar
    {
        Point origin;
        double radius;
        public double angle;

        public Polar(Point o, double r, double a)
        {
            origin = o;
            radius = r;
            angle = a;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point toCartesian()
        {
            return new Point(origin.X + radius * MathNet.Numerics.Trig.Cos(angle), origin.Y + radius * MathNet.Numerics.Trig.Sin(angle));
        }
    }
}