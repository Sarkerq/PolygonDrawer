using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;

namespace GK1
{

    public class GKPolyline
    {
        public List<Vertice> vertices = new List<Vertice>();
        public List<Edge> edges = new List<Edge>();
        public Carbon visuals;

        public GKPolyline(Carbon _visuals)
        {
            visuals = _visuals;
        }

        public GKPolyline(GKPolyline original)
        {
            visuals = original.visuals;
            for (int i = 0; i < original.vertices.Count; i++)
                vertices.Add(new Vertice(original.vertices[i]));
            for (int i = 0; i < original.vertices.Count - 1; i++)
                edges.Add(new Edge(vertices[i], vertices[i + 1]));
        }





        internal void deleteVertice(Vertice target)
        {
            int index = vertices.IndexOf(target);

            if (index != vertices.Count - 1) edges.RemoveAt(index);
            if (index != 0) edges.RemoveAt(index - 1);
            // !!!!!!!!!!!!!!!!!!!!DO POPRAWY !!!!!!!!!!
            if (index != 0 && index != vertices.Count - 1)
            {
                edges.Insert(index - 1, new Edge(vertices[index - 1], vertices[index + 1]));
            }
            vertices.RemoveAt(index);


        }


        public Vertice nextVertice(Vertice v)
        {
            if (v == vertices.Last())
                return null;
            int index = vertices.IndexOf(v);
            return vertices[index + 1];
        }
        public Vertice previousVertice(Vertice v)
        {
            if (v == vertices.First())
                return null;
            int index = vertices.IndexOf(v);
            return vertices[index - 1];
        }
        public Edge nextEdge(Vertice v)
        {
            if (v == vertices.Last())
                return null;
            int index = vertices.IndexOf(v);
            return edges[index];
        }
        public Edge previousEdge(Vertice v)
        {
            if (v == vertices.First())
                return null;
            int index = vertices.IndexOf(v);
            return edges[index - 1];
        }
        public Edge nextEdge(Edge e)
        {
            if (e == edges.Last())
                return null;
            int index = edges.IndexOf(e);
            return edges[index + 1];
        }
        public Edge previousEdge(Edge e)
        {
            if (e == edges.First())
                return null;
            int index = edges.IndexOf(e);
            return edges[index - 1];
        }
        public void PutVerticeInTheMiddle(Edge target)
        {
            int indexVert1 = 0, indexVert2 = 0;
            int indexEdge = 0;

            for (int i = 0; i < vertices.Count; i++)
            {
                if (target.v1 == vertices[i]) indexVert1 = i;
                if (target.v2 == vertices[i]) indexVert2 = i;
                if (i != edges.Count && target == edges[i]) indexEdge = i;
            }
            System.Windows.Point newVerticeCoords = new System.Windows.Point((target.v1.coords.X + target.v2.coords.X) / 2,
                                                (target.v1.coords.Y + target.v2.coords.Y) / 2);
            Vertice newV = new Vertice(newVerticeCoords);
            vertices.Insert((indexVert1 + indexVert2) / 2 + 1, newV);
            PopulateEdges();
        }

        internal void PopulateEdges()
        {
            edges = new List<Edge>();
            if (vertices.Count != 0)
            {
                for (int i = 0; i < vertices.Count - 1; i++)
                {
                    edges.Add(new Edge(vertices[i], vertices[i + 1]));
                }
            }
        }
    }
}
