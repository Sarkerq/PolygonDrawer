using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace GK1
{

    public class Vertice
    {

        public Point coords;
        public Ellipse visualRepresentation;
        public Vertice(double x, double y)
        {
            coords = new Point(x, y);
        }
        public Vertice(Point p)
        {
            coords = p;
        }
        public Vertice(Vertice v)
        {

            coords = new Point(v.coords.X, v.coords.Y);
        }

    }

}
