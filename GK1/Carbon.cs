using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GK1
{

    public enum Algorithm
    {
        WU,
        Bresenham
    }
    public class Carbon
    {
        public const int MAX_WIDTH = 1920;
        public const int MAX_HEIGHT = 1080;
        public int rawStride, width, height;
        public PixelFormat pf = PixelFormats.Pbgra32;
        public byte[] pixelData;
        public Edge[] pixelOwner;
        public Algorithm algorithm;
        public Image lineCarbon;
        BitmapSource bitmap;

        public Carbon(Image _lineCarbon)
        {
            lineCarbon = _lineCarbon;
            algorithm = Algorithm.Bresenham;
            width = MAX_WIDTH;
            height = MAX_HEIGHT;
            pf = PixelFormats.Pbgra32;
            rawStride = (MAX_WIDTH * pf.BitsPerPixel + 7) / 8;
            pixelData = new byte[rawStride * MAX_HEIGHT];
            pixelOwner = new Edge[rawStride * MAX_HEIGHT];
            for (int i = 0; i < pixelData.Length; i++)
            {
                pixelData[i] = 255;
            }
        }
        public void UpdateScreen()
        {
            bitmap = BitmapSource.Create(width, height,
                96, 96, pf, null, pixelData, rawStride);
            lineCarbon.Source = bitmap;
        }

        public void drawEdge(Edge e, Color color)
        {
            if (algorithm == Algorithm.Bresenham)
                Bresenham(e, color);
            else
                WU(e, color);
        }
        public void WU(Edge e, Color color)
        {
            double x0 = e.v1.coords.X;
            double y0 = e.v1.coords.Y;
            double x1 = e.v2.coords.X;
            double y1 = e.v2.coords.Y;
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            double temp;
            if (steep)
            {
                temp = x0; x0 = y0; y0 = temp;
                temp = x1; x1 = y1; y1 = temp;
            }
            if (x0 > x1)
            {
                temp = x0; x0 = x1; x1 = temp;
                temp = y0; y0 = y1; y1 = temp;
            }

            double dx = x1 - x0;
            double dy = y1 - y0;
            double gradient = dy / dx;

            double xEnd = round(x0);
            double yEnd = y0 + gradient * (xEnd - x0);
            double xGap = rfpart(x0 + 0.5);
            double xPixel1 = xEnd;
            double yPixel1 = ipart(yEnd);

            if (steep)
            {
                SetPixel(yPixel1, xPixel1, rfpart(yEnd) * xGap, Colors.Red, e);
                SetPixel(yPixel1 + 1, xPixel1, fpart(yEnd) * xGap, Colors.Red, e);
            }
            else
            {
                SetPixel(xPixel1, yPixel1, rfpart(yEnd) * xGap, Colors.Red, e);
                SetPixel(xPixel1, yPixel1 + 1, fpart(yEnd) * xGap, Colors.Red, e);
            }
            double intery = yEnd + gradient;

            xEnd = round(x1);
            yEnd = y1 + gradient * (xEnd - x1);
            xGap = fpart(x1 + 0.5);
            double xPixel2 = xEnd;
            double yPixel2 = ipart(yEnd);
            if (steep)
            {
                SetPixel(yPixel2, xPixel2, rfpart(yEnd) * xGap, Colors.Red, e);
                SetPixel(yPixel2 + 1, xPixel2, fpart(yEnd) * xGap, Colors.Red, e);
            }
            else
            {
                SetPixel(xPixel2, yPixel2, rfpart(yEnd) * xGap, Colors.Red, e);
                SetPixel(xPixel2, yPixel2 + 1, fpart(yEnd) * xGap, Colors.Red, e);
            }

            if (steep)
            {
                for (int x = (int)(xPixel1 + 1); x <= xPixel2 - 1; x++)
                {
                    SetPixel(ipart(intery), x, rfpart(intery), Colors.Red, e);
                    SetPixel(ipart(intery) + 1, x, fpart(intery), Colors.Red, e);
                    intery += gradient;
                }
            }
            else
            {
                for (int x = (int)(xPixel1 + 1); x <= xPixel2 - 1; x++)
                {
                    SetPixel(x, ipart(intery), rfpart(intery), Colors.Red, e);
                    SetPixel(x, ipart(intery) + 1, fpart(intery), Colors.Red, e);
                    intery += gradient;
                }
            }
        }



        int ipart(double x) { return (int)x; }

        int round(double x) { return ipart(x + 0.5); }

        double fpart(double x)
        {
            if (x < 0) return (1 - (x - Math.Floor(x)));
            return (x - Math.Floor(x));
        }

        double rfpart(double x)
        {
            return 1 - fpart(x);
        }
        public void Bresenham(Edge e, Color color)
        {
            int x = (int)e.v1.coords.X;
            int y = (int)e.v1.coords.Y;
            int x2 = (int)e.v2.coords.X;
            int y2 = (int)e.v2.coords.Y;


            int width = x2 - x;
            int height = y2 - y;


            int dx1 = Math.Sign(width);
            int dy1 = Math.Sign(height);
            int dx2 = Math.Sign(width);
            int dy2 = 0;


            int longest = Math.Abs(width);
            int shortest = Math.Abs(height);

            if (!(longest > shortest))
            {
                longest = Math.Abs(height);
                shortest = Math.Abs(width);
                if (height < 0) dy2 = -1; else if (height > 0) dy2 = 1;
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
        private void SetPixel(double x, double y, double c, Color original, Edge owner)
        {
            int alpha = (int)(c * 255);
            if (alpha > 255) alpha = 255;
            if (alpha < 0) alpha = 0;
            Color aliased = Color.FromArgb((byte)alpha, original.R, original.G, original.B);
            SetPixel((int)x, (int)y, aliased, owner);
        }
        public void SetPixel(int x, int y, Color c, Edge owner = null)
        {
            int xIndex = x * 4;
            int yIndex = y * rawStride;
            if (x < width && x >= 0 && y < height && y >= 0)
            {
                pixelData[xIndex + yIndex] = c.R;
                pixelData[xIndex + yIndex + 1] = c.G;
                pixelData[xIndex + yIndex + 2] = c.B;
                pixelData[xIndex + yIndex + 3] = c.A;
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

                Vertice first = drawnPolygon.vertices[0];


                for (int i = 0; i < drawnPolygon.edges.Count; i++)
                {
                    drawEdge(drawnPolygon.edges[i], Colors.DarkGray);
                }
                for (int i = 0; i < drawnPolygon.vertices.Count; i++)
                    drawVertice(drawnPolygon.vertices[i], Colors.Black, Colors.White);

            }

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

        internal void redrawOldPolygon(GKPolygon drawnPolygon)
        {
            if (drawnPolygon.vertices.Count >= 1)
            {

                Vertice first = drawnPolygon.vertices[0];


                for (int i = 0; i < drawnPolygon.edges.Count; i++)
                {
                    drawEdge(drawnPolygon.edges[i], Colors.LightGray);
                }
                for (int i = 0; i < drawnPolygon.vertices.Count; i++)
                    drawVertice(drawnPolygon.vertices[i], Colors.Black, Colors.White);
            }

        }
    }
}
