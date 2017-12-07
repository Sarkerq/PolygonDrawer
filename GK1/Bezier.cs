using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GK1
{
    static class Bezier
    {
        static public Point DeCasteljau(List<Vertice> vertices, double u)
        {
            Point[] workingArray = new Point[vertices.Count];
            for (int i = 0; i < vertices.Count; i++) workingArray[i] = vertices[i].coords;
            for (int k = 1; k < vertices.Count; k++)
            {
                for (int i = 0; i < vertices.Count - k; i++)
                {
                    workingArray[i] = new Point((1 - u) * workingArray[i].X + u * workingArray[i + 1].X,
                                                (1 - u) * workingArray[i].Y + u * workingArray[i + 1].Y);
                }
            }
            return workingArray[0];
        }

        internal static double DeCasteljauAngle(List<Vertice> vertices, double u)
        {
            Point[] workingArray = new Point[vertices.Count];
            for (int i = 0; i < vertices.Count; i++) workingArray[i] = vertices[i].coords;
            for (int k = 1; k < vertices.Count - 1 ; k++)
            {
                for (int i = 0; i < vertices.Count - k; i++)
                {
                    workingArray[i] = new Point((1 - u) * workingArray[i].X + u * workingArray[i + 1].X,
                                                (1 - u) * workingArray[i].Y + u * workingArray[i + 1].Y);
                }
            }
            return Global.AngleAgainstXAxis(workingArray[0], workingArray[1]);
        }
    }
}
