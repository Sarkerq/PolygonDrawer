using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GK1
{


    public class Carbon
    {
        public const int MAX_WIDTH = 1920;
        public const int MAX_HEIGHT = 1080;
        public int rawStride, width, height;
        public PixelFormat pf = PixelFormats.Rgb24;
        public byte[] pixelData;
        public Edge[] pixelOwner;
        public Carbon()
        {
            width = MAX_WIDTH;
            height = MAX_HEIGHT;
            pf = PixelFormats.Rgb24;
            rawStride = (MAX_WIDTH * pf.BitsPerPixel + 7) / 8;
            pixelData = new byte[rawStride * MAX_HEIGHT];
            pixelOwner = new Edge[rawStride * MAX_HEIGHT];
            for (int i = 0; i < pixelData.Length; i++)
            {
                pixelData[i] = 255;
            }
        }
        public void drawEdge(Edge e, Color color)
        {
            int x = (int)e.v1.coords.X;
            int y = (int)e.v1.coords.Y;
            int x2 = (int)e.v2.coords.X;
            int y2 = (int)e.v2.coords.Y;
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                for (int j = -1; j <= 1; j++)
                    for (int k = -1; k <= 1; k++)
                        SetPixel(x + k, y + j, color, e);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }

        }
        public void SetPixel(int x, int y, Color c, Edge owner = null)
        {
            int xIndex = x * 3;
            int yIndex = y * rawStride;
            if (x < width && x >= 0 && y < height && y >= 0)
            {
                pixelData[xIndex + yIndex] = c.R;
                pixelData[xIndex + yIndex + 1] = c.G;
                pixelData[xIndex + yIndex + 2] = c.B;
                if (owner != null)
                    pixelOwner[xIndex + yIndex] = owner;
            }
        }
        public void drawVertice(Vertice v, Color border, Color middle)
        {
            for (int y = (int)v.coords.Y - (int)Global.verticeRadius; y < (int)v.coords.Y + (int)Global.verticeRadius; y++)
                for (int x = (int)v.coords.X - (int)Global.verticeRadius; x < (int)v.coords.X + (int)Global.verticeRadius; x++)
                {
                    if (Math.Sqrt(Math.Pow(x - (int)v.coords.X, 2) + Math.Pow(y - (int)v.coords.Y, 2)) <= Global.verticeRadius) SetPixel(x, y, border);
                }
            for (int y = (int)v.coords.Y - (int)(Global.verticeRadius / 1.2); y < (int)v.coords.Y + (int)(Global.verticeRadius / 1.2); y++)
                for (int x = (int)v.coords.X - (int)(Global.verticeRadius / 1.2); x < (int)v.coords.X + (int)(Global.verticeRadius / 1.2); x++)
                {
                    if (Math.Sqrt(Math.Pow(x - (int)v.coords.X, 2) + Math.Pow(y - (int)v.coords.Y, 2)) <= Global.verticeRadius / 1.2) SetPixel(x, y, middle);
                }
        }
        public void redrawPolygon(GKPolygon drawnPolygon)
        {
            if (drawnPolygon.vertices.Count >= 1)
            {
                clear();

                Vertice first = drawnPolygon.vertices[0];


                for (int i = 0; i < drawnPolygon.edges.Count; i++)
                {
                    drawEdge(drawnPolygon.edges[i], Colors.DarkGray);
                }
                for (int i = 0; i < drawnPolygon.vertices.Count; i++)
                    drawVertice(drawnPolygon.vertices[i], Colors.Black, Colors.White);

                for (int i = 0; i < drawnPolygon.vertices.Count; i++)
                {
                    if (drawnPolygon.vertices[i].fixedAngle)
                        drawVertice(drawnPolygon.vertices[i], Colors.Black, Colors.Red);
                }
                for (int i = 0; i < drawnPolygon.edges.Count; i++)
                {
                    drawEdgeStatus(drawnPolygon.edges[i]);
                }
            }

        }

        private void drawEdgeStatus(Edge edge)
        {
            Point middle = (Point)(Vector.Add((Vector)edge.v1.coords, (Vector)edge.v2.coords) / 2);
            if (edge.state == EdgeState.Horizontal)
            {
                for (int y = (int)middle.Y - Global.statusRadius; y <= (int)middle.Y + Global.statusRadius; y++)
                    for (int x = (int)middle.X - Global.statusRadius; x <= (int)middle.X + Global.statusRadius; x++)
                    {
                        if (Math.Abs(y - (int)middle.Y) == Global.statusLineSeparRadius && Math.Abs(x - (int)middle.X) <= Global.statusLineLengthRadius)
                            SetPixel(x, y, Colors.Black);
                        else if (y == (int)middle.Y - Global.statusRadius || y == (int)middle.Y + Global.statusRadius ||
                                 x == (int)middle.X - Global.statusRadius || x == (int)middle.X + Global.statusRadius)
                            SetPixel(x, y, Colors.Black);

                        else
                            SetPixel(x, y, Colors.Gray);

                    }

            }
            else if (edge.state == EdgeState.Vertical)
            {
                for (int y = (int)middle.Y - Global.statusRadius; y <= (int)middle.Y + Global.statusRadius; y++)
                    for (int x = (int)middle.X - Global.statusRadius; x <= (int)middle.X + Global.statusRadius; x++)
                    {
                        if (Math.Abs(x - (int)middle.X) == Global.statusLineSeparRadius && Math.Abs(y - (int)middle.Y) <= Global.statusLineLengthRadius)
                            SetPixel(x, y, Colors.Black);
                        else if (y == (int)middle.Y - Global.statusRadius || y == (int)middle.Y + Global.statusRadius ||
                                 x == (int)middle.X - Global.statusRadius || x == (int)middle.X + Global.statusRadius)
                            SetPixel(x, y, Colors.Black);

                        else
                            SetPixel(x, y, Colors.Gray);

                    }
            }
        }

        private void drawFixedVertice(Vertice vertice)
        {
            throw new NotImplementedException();
        }

        public void clear()
        {
            pixelData = new byte[rawStride * height];
            pixelOwner = new Edge[rawStride * height];
            for (int i = 0; i < pixelData.Length; i++)
            {
                pixelData[i] = 255;
            }
        }
    }
}
