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
        Clockwise,
        CounterClockwise,
        None
    }
    public class Vertice
    {
        public VerticeState fixedVertical;
        public VerticeState fixedHorizontal;
        public bool fixedAngle;
        public int fixedAngleValue;
        public double xValue;
        public double yValue;
        public Ellipse visualRepresentation;
        public Vertice(double x, double y)
        {
            fixedVertical = fixedHorizontal = VerticeState.None;
            fixedAngle = false;
            xValue = x;
            yValue = y;
        }
        public Vertice(Point p)
        {
            fixedVertical = fixedHorizontal = VerticeState.None;
            fixedAngle = false;
            xValue = p.X;
            yValue = p.Y;
        }
        public Vertice(Vertice v)
        {
            fixedVertical = v.fixedVertical;
            fixedHorizontal = v.fixedHorizontal;
            fixedAngle = v.fixedAngle;
            fixedAngleValue = v.fixedAngleValue;
            xValue = v.xValue;
            yValue = v.yValue;
        }
    }
    
}
