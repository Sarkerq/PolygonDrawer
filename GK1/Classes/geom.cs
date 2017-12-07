using System;

namespace GK1
{
    public static partial class Geometry
    {
        public const int ClockWise = -1;
        public const int Collinear = 0;
        public const int AntiClockWise = 1;

        public static double Epsilon = 0.00000001;

        public static readonly Vertice NullPoint = new Vertice(0, 0);
        public static readonly Edge NullSegment = new Edge(NullPoint, NullPoint);

        public struct Direction
        {
            private int x;

            private Direction(double d)
            {
                x = (Math.Abs(d) < Epsilon) ? 0 : (d > 0 ? 1 : -1);
            }

            public static bool operator ==(Direction a, Direction b)
            {
                return a.x == b.x;
            }

            public static bool operator !=(Direction a, Direction b)
            {
                return !(a == b);
            }

            public static int operator *(Direction a, Direction b)
            {
                return a.x * b.x;
            }

            public static implicit operator int(Direction d)
            {
                return d.x;
            }

            public static explicit operator Direction(double d)
            {
                return new Direction(d);
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

        }
    } // static partial class Geometry
} // namespace ASD
