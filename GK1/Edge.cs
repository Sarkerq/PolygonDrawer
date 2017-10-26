using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GK1
{
    public class Edge
    {
        public Vertice v1, v2;
        public EdgeState state;
        public Edge(Vertice _v1, Vertice _v2)
        {
            v1 = _v1;
            v2 = _v2;
            state = EdgeState.None;
        }

        public bool CorrectVertical()
        {

            return state != EdgeState.Vertical || Math.Abs(v1.coords.X - v2.coords.X) < Global.pixelEpsilon;
        }

        public bool CorrectHorizontal()
        {
            return state != EdgeState.Horizontal || Math.Abs(v1.coords.Y - v2.coords.Y) < Global.pixelEpsilon;

        }
        internal void ForceVertical()
        {

            Point bottom = v1.coords;
            Point top = v2.coords;
            if (bottom.Y > top.Y)
            {
                Point tmp = bottom;
                bottom = top;
                top = tmp;

                Polar pol = new Polar(bottom, Global.Distance(bottom, top), Math.PI / 2);
                v1.coords = pol.toCartesian();

                v1.fixedVertical = VerticeState.Top;
                v2.fixedVertical = VerticeState.Bottom;
            }
            else
            {
                Polar pol = new Polar(bottom, Global.Distance(bottom, top), Math.PI / 2);
                v2.coords = pol.toCartesian();

                v1.fixedVertical = VerticeState.Bottom;
                v2.fixedVertical = VerticeState.Top;

            }
            state = EdgeState.Vertical;

        }
        internal void ForceHorizontal()
        {
            Point left = v1.coords;
            Point right = v2.coords;
            if (left.X > right.X)
            {
                Point tmp = left;
                left = right;
                right = tmp;

                Polar pol = new Polar(left, Global.Distance(left, right), 0);
                v1.coords = pol.toCartesian();
                v1.fixedHorizontal = VerticeState.Right;
                v2.fixedHorizontal = VerticeState.Left;

            }
            else
            {
                Polar pol = new Polar(left, Global.Distance(left, right), 0);
                v2.coords = pol.toCartesian();
                v1.fixedHorizontal = VerticeState.Left;
                v2.fixedHorizontal = VerticeState.Right;
            }
            state = EdgeState.Horizontal;

        }
    }
}
