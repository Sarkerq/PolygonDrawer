using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GK1
{
    public class GKPolygon
    {
        public List<Vertice> vertices = new List<Vertice>();
        public List<Edge> edges = new List<Edge>();
        public Carbon visuals;

        public GKPolygon(Carbon _visuals)
        {
            visuals = _visuals;
        }
        public void putVerticeInTheMiddle(Edge target)
        {
            int indexVert = 0;
            int indexEdge = 0;
            target.v1.clearStatus();
            target.v1.clearStatus();

            for (int i = 0; i < vertices.Count; i++)
            {
                if (target.v1 == vertices[i] || target.v2 == vertices[i]) indexVert = i;
                if (target == edges[i]) indexEdge = i;
            }
            Point newVerticeCoords = new Point((target.v1.coords.X + target.v2.coords.X) / 2,
                                                (target.v1.coords.Y + target.v2.coords.Y) / 2);
            Vertice newV = new Vertice(newVerticeCoords);
            vertices.Insert(indexVert, newV);
            edges.RemoveAt(indexEdge);
            edges.Insert(indexEdge, new Edge(target.v1, newV));
            edges.Insert(indexEdge + 1, new Edge(newV, target.v2));
        }
        internal void forceVertical(Edge target)
        {

            Point bottom = target.v1.coords;
            Point top = target.v2.coords;
            if (bottom.Y > top.Y)
            {
                Point tmp = bottom;
                bottom = top;
                top = tmp;

                Polar pol = new Polar(bottom, Global.distance(bottom, top), Math.PI / 2);
                target.v1.coords = pol.toCartesian();

                target.v1.fixedVertical = VerticeState.Top;
                target.v2.fixedVertical = VerticeState.Bottom;
            }
            else
            {
                Polar pol = new Polar(bottom, Global.distance(bottom, top), Math.PI / 2);
                target.v2.coords = pol.toCartesian();

                target.v1.fixedVertical = VerticeState.Bottom;
                target.v2.fixedVertical = VerticeState.Top;

            }
            target.state = EdgeState.Vertical;

        }
    }
}
