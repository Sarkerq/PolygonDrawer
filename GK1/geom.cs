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


        // funkcja zwraca true jesli punkt q nalezy do
        // prostokata wyznaczonego przez punkty p1 i p2
        public static bool OnRectangle(Vertice p1, Vertice p2, Vertice q)
        {
            return Math.Min(p1.coords.X, p2.coords.X) <= q.coords.X && q.coords.X <= Math.Max(p1.coords.X, p2.coords.X) && Math.Min(p1.coords.Y, p2.coords.Y) <= q.coords.Y && q.coords.Y <= Math.Max(p1.coords.Y, p2.coords.Y);
        }

        public static bool Intersection(Edge s1, Edge s2)
        {
            Direction s1s_s2 = s2.Direction(s1.v1);   // polozenie poczatku odcinka s1 wzgledem odcinka s2
            Direction s1e_s2 = s2.Direction(s1.v2);   // polozenie konca    odcinka s1 wzgledem odcinka s2
            Direction s2s_s1 = s1.Direction(s2.v1);   // polozenie poczatku odcinka s2 wzgledem odcinka s1
            Direction s2e_s1 = s1.Direction(s2.v2);   // polozenie konca    odcinka s2 wzgledem odcinka s1

            int s12 = s1s_s2 * s1e_s2;   // polozenie odcinka s1 wzgledem odcinka s2
            int s21 = s2s_s1 * s2e_s1;   // polozenie odcinka s2 wzgledem odcinka s1

            // konce jednego z odcinkow leza po tej samej stronie drugiego
            if (s12 > 0 || s21 > 0) return false;   // odcinki nie przecinaja sie

            // konce zadnego z odcinkow nie leza po tej samej stronie drugiego
            // i konce jednego z odcinkow leza po przeciwnych stronach drugiego
            if (s12 < 0 || s21 < 0) return true;    // odcinki przecinaja sie

            return Math.Min(Math.Max(s1.v1.coords.X, s1.v2.coords.X), Math.Max(s2.v1.coords.X, s2.v2.coords.X)) >= Math.Max(Math.Min(s1.v1.coords.X, s1.v2.coords.X), Math.Min(s2.v1.coords.X, s2.v2.coords.X)) &&
                   Math.Min(Math.Max(s1.v1.coords.Y, s1.v2.coords.Y), Math.Max(s2.v1.coords.Y, s2.v2.coords.Y)) >= Math.Max(Math.Min(s1.v1.coords.Y, s1.v2.coords.Y), Math.Min(s2.v1.coords.Y, s2.v2.coords.Y));

            //return s1s_s2!=Collinear || s1e_s2!=Collinear || s2s_s1!=Collinear || s2e_s1!=Collinear ||
            //       OnRectangle(s2.v1,s2.v2,s1.v1) || OnRectangle(s2.v1,s2.v2,s1.v2) || OnRectangle(s1.v1,s1.v2,s2.v1) || OnRectangle(s1.v1,s1.v2,s2.v2) ;

            //return OnRectangle(s2.v1,s2.v2,s1.v1) || OnRectangle(s2.v1,s2.v2,s1.v2) || OnRectangle(s1.v1,s1.v2,s2.v1) || OnRectangle(s1.v1,s1.v2,s2.v2) ;
        }

        // sortowanie katowe punktow z tablicy p w kierunku przeciwnym do ruchu wskazowek zegara wzgledem punktu centralnego c
        // czyli sortowanie wzgledem rosnacych katow odcinka (c,p[i]) z osia x
        // przy pomocy parametru ifAngleEqual mozna doprecyzowaz kryterium sortowania gdy katy sa rowne
        // (domyslnie nic nie doprecyzowujemy, pozostawiamy rowne)
        public static Vertice[] AngleSort(Vertice c, Vertice[] p, System.Comparison<Vertice> ifAngleEqual = null)
        {
            if (ifAngleEqual == null) ifAngleEqual = (p1, p2) => 0;
            if (p == null) throw new System.ArgumentNullException();
            if (p.Length < 2) return p;
            System.Comparison<Vertice> cmp = delegate (Vertice p1, Vertice p2)
            {
                int r = -(new Edge(c, p1)).Direction(p2);
                return r != 0 ? r : ifAngleEqual(p1, p2);
            };
            var s1 = new System.Collections.Generic.List<Vertice>();
            var s2 = new System.Collections.Generic.List<Vertice>();
            for (int i = 0; i < p.Length; ++i)
                if (p[i].coords.Y > c.coords.Y || (p[i].coords.Y == c.coords.Y && p[i].coords.X >= c.coords.X))
                    s1.Add(p[i]);
                else
                    s2.Add(p[i]);
            s1.Sort(cmp);
            s2.Sort(cmp);
            s1.AddRange(s2);
            return s1.ToArray();
        }
    } // static partial class Geometry
} // namespace ASD
