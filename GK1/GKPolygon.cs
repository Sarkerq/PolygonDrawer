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

        internal bool IsCorrectVertice(Vertice v, bool front)
        {
            int edgeIndex;
            if (front)
                edgeIndex = vertices.IndexOf(v);
            else
                edgeIndex = (vertices.IndexOf(v) - 1 + vertices.Count) % vertices.Count;
            return CorrectAngle(v) && edges[edgeIndex].CorrectHorizontal() && edges[edgeIndex].CorrectVertical();
        }
        private void RepairVertice(Vertice v, bool front)
        {

            int edgeIndex;
            if (front)
                edgeIndex = vertices.IndexOf(v);
            else
                edgeIndex = (vertices.IndexOf(v) - 1 + vertices.Count) % vertices.Count;

            Edge chosen = edges[edgeIndex];
            Vertice toChange;
            if (chosen.v1 == v)
                toChange = chosen.v2;
            else
                toChange = chosen.v1;
            if (v.fixedAngle)
            {
                ForceAngle(v, v.fixedAngleValue, chosen);
            }
            if (chosen.state == EdgeState.Horizontal)
            {

            }
            if (chosen.state == EdgeState.Vertical)
            {

            }

        }
        public void RepairVertices(Edge changedEdge)
        {
            RepairVertices(vertices.IndexOf(changedEdge.v1));
        }

        public bool RepairVertices(int changedVerticeIndex)
        {
            return true;
            bool[] verticeFixed = new bool[vertices.Count];
            int iter = changedVerticeIndex;
            bool first = true;
            // vertices following
            while (first || iter != changedVerticeIndex)
            {
                first = false;
                Vertice v = vertices[iter];
                if (IsCorrectVertice(v, true)) break;
                else RepairVertice(v, true);


                iter = (iter + 1) % vertices.Count;

            }
            if (iter == changedVerticeIndex) return false;
            int newIter = changedVerticeIndex;
            first = true;
            while (first || newIter != iter)
            {
                first = false;
                Vertice v = vertices[iter];
                if (IsCorrectVertice(v, false)) break;
                else RepairVertice(v, false);


                newIter = (newIter - 1 + vertices.Count) % vertices.Count;

            }
            if (iter == newIter) return false;
            return true;
        }

        private bool CorrectAngle(Vertice v)
        {
            int index = vertices.IndexOf(v);
            return !v.fixedAngle || Math.Abs(Global.GetAngle(
                                             vertices[(index - 1 + vertices.Count) % vertices.Count],
                                             v,
                                             vertices[(index + 1) % vertices.Count])
                                             - v.fixedAngleValue) < Global.angleEpsilon;

        }
        public void PutVerticeInTheMiddle(Edge target)
        {
            int indexVert = 0;
            int indexEdge = 0;
            target.v1.ClearStatus();
            target.v1.ClearStatus();

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
        public void ForceAngle(Vertice target, double angle, Edge chosen)
        {
            Vertice left, right;
            Edge[] pair = new Edge[2];
            int i = 0;
            target.fixedAngle = true;
            target.fixedAngleValue = angle;
            foreach (Edge e in edges)
            {
                if (e.v1 == target || e.v2 == target)
                {
                    pair[i] = e;
                    i++;
                }
            }
            if (pair[0].v1 == target) left = pair[0].v2;
            else left = pair[0].v1;
            if (pair[1].v1 == target) right = pair[1].v2;
            else right = pair[1].v1;

            //double originalAngle = getAngle(left, target, right);
            //if(originalAngle < 0)
            //{
            //    Vertice tmp = left;
            //    left = right;
            //    right = tmp;
            //}

            double originalLeftAngle = Global.AngleAgainstXAxis(target.coords, left.coords);
            if (chosen == pair[0])
            {
                Polar pol = new Polar(target.coords, Global.Distance(target.coords, right.coords), angle + originalLeftAngle);
                Point result = pol.toCartesian();
                right.coords = result;
            }
            else
            {
                Polar pol = new Polar(target.coords, Global.Distance(target.coords, left.coords), angle + originalLeftAngle);
                Point result = pol.toCartesian();
                left.coords = result;
            }
        }
        public void ForceAngle(Vertice target, double angle, VerticeFix fix)
        {
            Vertice left, right;
            Edge[] pair = new Edge[2];
            int i = 0;
            target.fixedAngle = true;
            target.fixedAngleValue = angle;
            foreach (Edge e in edges)
            {
                if (e.v1 == target || e.v2 == target)
                {
                    pair[i] = e;
                    i++;
                }
            }
            if (pair[0].v1 == target) left = pair[0].v2;
            else left = pair[0].v1;
            if (pair[1].v1 == target) right = pair[1].v2;
            else right = pair[1].v1;

            //double originalAngle = getAngle(left, target, right);
            //if(originalAngle < 0)
            //{
            //    Vertice tmp = left;
            //    left = right;
            //    right = tmp;
            //}

            double originalLeftAngle = Global.AngleAgainstXAxis(target.coords, left.coords);
            if (fix == VerticeFix.Left)
            {
                Polar pol = new Polar(target.coords, Global.Distance(target.coords, right.coords), angle + originalLeftAngle);
                Point result = pol.toCartesian();
                right.coords = result;
            }
            else
            {
                Polar pol = new Polar(target.coords, Global.Distance(target.coords, left.coords), angle + originalLeftAngle);
                Point result = pol.toCartesian();
                left.coords = result;
            }
        }



    }
}
