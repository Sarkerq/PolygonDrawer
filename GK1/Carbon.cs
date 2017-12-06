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


    public class Carbon
    {
        public const int TICKS_PER_DAY = 24;
        public const int MAX_WIDTH = 1240;
        public const int MAX_HEIGHT = 810;
        public int rawStride, width, height;
        public PixelFormat pf = PixelFormats.Rgb24;
        public byte[] pixelData;

        public byte[] texturePixelData;
        public int texturePixelDataWidth, texturePixelDataHeight;


        public Edge[] pixelOwner;
        public Image lineCarbon;
        BitmapSource bitmap;

        public Point middle;
        public double f_value, Kd_value, Ks_value, m_value;

        public bool showPolyline = true;
        internal bool inGrayscale = false;
        internal bool shearRotation = false;
        internal bool fixedRotation = false;

        public Carbon(Image _lineCarbon)
        {
            lineCarbon = _lineCarbon;
            width = MAX_WIDTH;
            height = MAX_HEIGHT;
            middle = new Point(width / 2, height / 2);
            pf = PixelFormats.Rgb24;
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
        internal void RefreshAll(GKPolyline polyline, GKPolygon polygon = null)
        {
            clear();
            if (showPolyline)
                redrawPolyline(polyline);
            int segments = 100;
            redrawBezierCurve(polyline, segments);
            if (polygon != null)
                redrawPolygon(polygon);
            UpdateScreen();
        }

        private void redrawBezierCurve(GKPolyline polyline, int segments)
        {
            GKPolyline bezierCurve = new GKPolyline(this);
            for (int i = 0; i < segments; i++)
            {
                bezierCurve.AddNewVertice(Bezier.DeCasteljau(polyline.vertices, (double)i / ((double)segments - 1)), ApplicationMode.AddPolyline);
            }
            bezierCurve.PopulateEdges();
            redrawPolylineEdges(bezierCurve, Colors.Red);
        }

        public void RefreshPolyline(GKPolyline polyline)
        {

            clear();
            if (showPolyline)
                redrawPolyline(polyline);
            UpdateScreen();
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
        private void SetPixelFromTexture(int x, int y, int xTexture, int yTexture)
        {
            if (x < 0 || x > MAX_WIDTH || y < 0 || y > MAX_HEIGHT) return;
            int xIndex = x * 3;
            int yIndex = y * rawStride;

            if (texturePixelDataWidth == 0) return;
            int xTextureIndex = (xTexture % texturePixelDataWidth) * 3;
            int textureRawStride = (texturePixelDataWidth * pf.BitsPerPixel + 7) / 8;
            int yTextureIndex = (yTexture% texturePixelDataHeight) * textureRawStride;

            if (x < width && x >= 0 && y < height && y >= 0)
            {
                pixelData[xIndex + yIndex] = texturePixelData[(xTextureIndex + yTextureIndex)];


                pixelData[xIndex + yIndex + 1] = texturePixelData[xTextureIndex + yTextureIndex + 1];


                pixelData[xIndex + yIndex + 2] = texturePixelData[xTextureIndex + yTextureIndex + 2];
            }
        }

        public void normalizeVector(double[] normalizedVector)
        {

            {

                double norm = Math.Sqrt(
                    normalizedVector[0] * normalizedVector[0] +
                    normalizedVector[1] * normalizedVector[1] +
                    normalizedVector[2] * normalizedVector[2]);
                for (int i = 0; i < 3; i++)
                {
                    normalizedVector[i] /= norm;

                }

            }
        }
        public void reduceToZOne(double[] reducedVector)
        {
            if (reducedVector[2] == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (reducedVector[i] > 0)
                        reducedVector[i] = 1;
                    else if (reducedVector[i] == 0)
                        reducedVector[i] = 0;
                    else
                        reducedVector[i] = -1;

                }
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    reducedVector[i] /= reducedVector[2];

                }
                reducedVector[2] = 1;
            }
        }

        private byte computeLambertModel(byte objectColor, byte lightsourceColor, double NLAngle, double VRAngle)
        {
            double I_O = ((double)objectColor) / 255;
            double I_L = ((double)lightsourceColor) / 255;

            double result = Kd_value * I_O * I_L * NLAngle + Ks_value * I_L * Math.Pow(VRAngle, m_value);
            if (result > 1) result = 1;
            if (result < 0) result = 0;
            return (byte)(result * 255);
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
        public void redrawPolyline(GKPolyline drawnPolyline, Color? edgeColor = null, Color? verticeInsideColor = null, Color? verticeBorderColor = null)
        {
            redrawPolylineEdges(drawnPolyline, edgeColor);
            redrawPolylineVertices(drawnPolyline, verticeInsideColor, verticeBorderColor);
        }

        public void redrawPolylineVertices(GKPolyline drawnPolyline, Color? _verticeInsideColor = null, Color? _verticeBorderColor = null)
        {
            if (drawnPolyline.vertices.Count >= 1)
            {
                Color verticeInsideColor = _verticeInsideColor == null ? Colors.White : (Color)_verticeInsideColor;
                Color verticeBorderColor = _verticeBorderColor == null ? Colors.Black : (Color)_verticeBorderColor;

                for (int i = 0; i < drawnPolyline.vertices.Count; i++)
                    drawVertice(drawnPolyline.vertices[i], verticeBorderColor, verticeInsideColor);
            }
        }

        public void redrawPolylineEdges(GKPolyline drawnPolyline, Color? _edgeColor = null)
        {
            if (drawnPolyline.vertices.Count >= 1)
            {
                Color edgeColor = _edgeColor == null ? Colors.DarkGray : (Color)_edgeColor;

                Vertice first = drawnPolyline.vertices[0];


                for (int i = 0; i < drawnPolyline.edges.Count; i++)
                {
                    drawEdge(drawnPolyline.edges[i], edgeColor);
                }

            }

        }
        private void redrawPolygon(GKPolygon drawnPolygon, Color? _edgeColor = null, Color? _verticeInsideColor = null, Color? _verticeBorderColor = null)
        {
            if (drawnPolygon.vertices.Count >= 1)
            {
                Color edgeColor = _edgeColor == null ? Colors.DarkGray : (Color)_edgeColor;
                Color verticeInsideColor = _verticeInsideColor == null ? Colors.White : (Color)_verticeInsideColor;
                Color verticeBorderColor = _verticeBorderColor == null ? Colors.Black : (Color)_verticeBorderColor;

                Vertice first = drawnPolygon.vertices[0];

                fillPolygon(drawnPolygon);

                for (int i = 0; i < drawnPolygon.edges.Count; i++)
                {
                    drawEdge(drawnPolygon.edges[i], edgeColor);
                }
                for (int i = 0; i < drawnPolygon.vertices.Count; i++)
                    drawVertice(drawnPolygon.vertices[i], verticeBorderColor, verticeInsideColor);
            }

        }



        internal void redrawCurrentPolyline(GKPolyline polyline)
        {
            redrawPolyline(polyline, Colors.LightBlue, Colors.White, Colors.DarkBlue);
        }
        public void fillPolygon(GKPolygon polygon)
        {
            List<ETElement>[] ET = new List<ETElement>[MAX_HEIGHT];
            for (int i = 0; i < MAX_HEIGHT; i++) ET[i] = new List<ETElement>();
            List<AETElement> AET = new List<AETElement>();
            int maxYMin = 0;
            int minYMin = int.MaxValue;
            foreach (Edge e in polygon.edges)
            {
                ETElement etElement = new ETElement(e);
                if (etElement.yMin >= 0)
                {
                    ET[Math.Min((int)etElement.yMin, MAX_HEIGHT - 1)].Add(etElement);
                    if (etElement.yMin > maxYMin) maxYMin = (int)etElement.yMin;
                    if (etElement.yMin < minYMin) minYMin = (int)etElement.yMin;
                }

            }
            //We count from the top
            for (int i = Math.Min(maxYMin, MAX_HEIGHT - 1); i >= 0; i--)
            {
                foreach (ETElement e in ET[i])
                {
                    if (e.yMax <= i)
                    {

                        AET.Add(new AETElement(e));
                        if (e.yMin > i)
                        {
                            AET.Last().x -= (e.yMin - i) * AET.Last().mRecip;
                        }
                    }
                }
                AET.Sort(new ByYMax());
                while (AET.Count > 0 && (int)AET.Last().yMax == i)
                {
                    AET.Remove(AET.Last());
                }
                AET.Sort(new ByX());
                List<AETElement> AETList = AET.ToList();
                for (int j = 0; j < AETList.Count - 1; j += 2)
                {
                    for (int k = (int)AETList[j].x; k <= AETList[j + 1].x; k++)
                    {
                        SetPixelFromTexture(k, i,k - (int)polygon.vertices[0].coords.X, i - (int)polygon.vertices[0].coords.Y);

                    }
                }

                foreach (AETElement e in AET)
                {
                    e.x -= e.mRecip;
                    if (e.x > e.xMax && e.mRecip < 0) e.x = e.xMax;
                    if (e.x < e.xMax && e.mRecip > 0) e.x = e.xMax;

                }
            }
        }



        public class ByX : IComparer<AETElement>
        {
            public int Compare(AETElement p1, AETElement p2)
            {
                if (p1.x == p2.x) return 0;
                return p1.x > p2.x ? 1 : -1;

            }
        }
        public class ByYMax : IComparer<AETElement>
        {
            public int Compare(AETElement p1, AETElement p2)
            {
                if (p1.yMax == p2.yMax) return 0;
                return p1.yMax > p2.yMax ? 1 : -1;

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

        internal void drawImageIn(out GKPolygon currentImageState, Point point, GKPolyline currentPolyline)
        {
            ApplicationMode mode = ApplicationMode.NewPolyline;
            currentImageState = new GKPolygon(this);
            currentImageState.AddNewVertice(new Point(point.X - texturePixelDataWidth / 2, point.Y - texturePixelDataHeight / 2), mode);
            currentImageState.AddNewVertice(new Point(point.X + texturePixelDataWidth / 2, point.Y - texturePixelDataHeight / 2), mode);
            currentImageState.AddNewVertice(new Point(point.X + texturePixelDataWidth / 2, point.Y + texturePixelDataHeight / 2), mode);
            currentImageState.AddNewVertice(new Point(point.X - texturePixelDataWidth / 2, point.Y + texturePixelDataHeight / 2), mode);
            currentImageState.PopulateEdges();
        }
    }
}
