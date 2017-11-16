using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static GK1.Geometry;

namespace GK1
{

    public class Edge
    {
        public Vertice v1, v2;
        public Edge(Vertice _v1, Vertice _v2)
        {
            v1 = _v1;
            v2 = _v2;
        }

        // funkcja zwraca orientacje punktu p wzgledem odcinka
        public Direction Direction(Vertice p)
        {
            return (Direction)Vertice.CrossProduct(v2 - v1, p - v1);
        }

    }
    public class AETElement
    {
        public double yMax;
        public double x;
        public double mRecip;

        public AETElement(ETElement e)
        {
            yMax = e.yMax;
            x = e.xMin;
            mRecip = e.mRecip;

        }
    }
    public class ETElement
    {
        public double yMax;
        public double yMin;
        public double xMin;
        public double mRecip;

        public ETElement(Edge e)
        {
            // Math.Min because we count from top
            yMax = Math.Min(e.v1.coords.Y, e.v2.coords.Y);
            yMin = Math.Max(e.v1.coords.Y, e.v2.coords.Y);
            //xMin is x in (x,yMin) of Edge
            if (yMin == e.v1.coords.Y)
            {
                xMin = e.v1.coords.X;

            }
            else
            {
                xMin = e.v2.coords.X;
            }

            if (e.v1.coords.Y == e.v2.coords.Y) mRecip = 0;
            else mRecip = (e.v2.coords.X - e.v1.coords.X) / (e.v2.coords.Y - e.v1.coords.Y);
        }
    }
}
