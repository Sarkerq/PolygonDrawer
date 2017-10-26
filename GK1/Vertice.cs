using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace GK1
{
    public enum VerticeState
    {
        Left,
        Right,
        Top,
        Bottom,
        None
    }
    public class Vertice
    {
        public VerticeState fixedVertical;
        public VerticeState fixedHorizontal;
        public bool fixedAngle;
        public double fixedAngleValue;
        public Point coords;
        public Ellipse visualRepresentation;
        public Vertice(double x, double y)
        {
            fixedVertical = fixedHorizontal = VerticeState.None;
            fixedAngle = false;
            coords = new Point(x, y);
        }
        public Vertice(Point p)
        {
            fixedVertical = fixedHorizontal = VerticeState.None;
            fixedAngle = false;
            coords = p;
        }
        public Vertice(Vertice v)
        {
            fixedVertical = v.fixedVertical;
            fixedHorizontal = v.fixedHorizontal;
            fixedAngle = v.fixedAngle;
            fixedAngleValue = v.fixedAngleValue;
            coords = v.coords;
        }
        public void clearStatus()
        {
            fixedAngle = false;
            fixedAngleValue = 0;
            fixedHorizontal = VerticeState.None;
            fixedVertical = VerticeState.None;
        }
    }
    
}
