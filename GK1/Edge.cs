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
}
