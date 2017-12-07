using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        internal bool filterRotation = false;
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
        internal void RefreshAll(GKPolyline polyline, int segments = 200, GKPolygon polygon = null, double tilt = 0)
        {
            clear();
            if (showPolyline)
                redrawPolyline(polyline);
            redrawBezierCurve(polyline, segments);
            if (polygon != null)
            {
                redrawPolygon(polygon, tilt);
            }
            UpdateScreen();
        }

        public void redrawBezierCurve(GKPolyline polyline, int segments)
        {
            GKPolyline bezierCurve = new GKPolyline(this);
            for (int i = 0; i < segments; i++)
            {
                bezierCurve.AddNewVertice(Bezier.DeCasteljau(polyline.vertices, (double)i / ((double)segments - 1)), ApplicationMode.AddPolyline);
            }
            bezierCurve.PopulateEdges();
            redrawPolylineEdges(bezierCurve, Colors.Red);
        }
        public GKPolyline getBezierCurve(GKPolyline polyline, int segments)
        {
            GKPolyline bezierCurve = new GKPolyline(this);
            for (int i = 0; i < segments; i++)
            {
                bezierCurve.AddNewVertice(Bezier.DeCasteljau(polyline.vertices, (double)i / ((double)segments - 1)), ApplicationMode.AddPolyline);
            }
            bezierCurve.PopulateEdges();
            return bezierCurve;
        }
        public double[] getBezierAngles(GKPolyline polyline, int segments)
        {
            double[] bezierAngles = new double[segments];
            for (int i = 0; i < segments; i++)
            {
                bezierAngles[i] = Bezier.DeCasteljauAngle(polyline.vertices, (double)i / ((double)segments - 1));
            }
            return bezierAngles;
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
            if (xTexture < 0 || xTexture > texturePixelDataWidth || yTexture < 0 || yTexture > texturePixelDataHeight) return;

            int xIndex = x * 3;
            int yIndex = y * rawStride;

            if (texturePixelDataWidth == 0) return;
            int xTextureIndex = ((xTexture + texturePixelDataWidth) % texturePixelDataWidth) * 3;
            int textureRawStride = (texturePixelDataWidth * pf.BitsPerPixel + 7) / 8;
            int yTextureIndex = ((yTexture + texturePixelDataHeight) % texturePixelDataHeight) * textureRawStride;

            if (x < width && x >= 0 && y < height && y >= 0)
            {
                if (inGrayscale)
                {
                    pixelData[xIndex + yIndex] = pixelData[xIndex + yIndex + 1] = pixelData[xIndex + yIndex + 2] =
                        (byte)(
                        0.299 * (double)texturePixelData[(xTextureIndex + yTextureIndex)] +
                        0.587 * (double)texturePixelData[xTextureIndex + yTextureIndex + 1] +
                        0.114 * (double)texturePixelData[xTextureIndex + yTextureIndex + 2]
                        );
                }
                else
                {
                    pixelData[xIndex + yIndex] = texturePixelData[(xTextureIndex + yTextureIndex)];


                    pixelData[xIndex + yIndex + 1] = texturePixelData[xTextureIndex + yTextureIndex + 1];


                    pixelData[xIndex + yIndex + 2] = texturePixelData[xTextureIndex + yTextureIndex + 2];
                }
            }
        }
        private void SetFilteredPixelFromTexture(int x, int y, double xTexture, double yTexture)
        {
            if (x < 0 || x > MAX_WIDTH || y < 0 || y > MAX_HEIGHT) return;
            if (xTexture < 0 || xTexture > texturePixelDataWidth || yTexture < 0 || yTexture > texturePixelDataHeight) return;

            int xIndex = x * 3;
            int yIndex = y * rawStride;

            if (texturePixelDataWidth == 0) return;
            int xTextureFloor = (int)xTexture;
            int yTextureFloor = (int)yTexture;
            double xTextureMantissa = xTexture - (double)xTextureFloor;
            double yTextureMantissa = yTexture - (double)yTextureFloor;

            int xTextureIndex = (((int)xTexture + texturePixelDataWidth) % texturePixelDataWidth) * 3;
            int nextXTextureIndex;
            if ((int)xTexture == texturePixelDataWidth - 1)
                nextXTextureIndex = xTextureIndex;
            else
                nextXTextureIndex = (((int)xTexture + 1 + texturePixelDataWidth) % texturePixelDataWidth) * 3;

            int textureRawStride = (texturePixelDataWidth * pf.BitsPerPixel + 7) / 8;
            int yTextureIndex = (((int)yTexture + texturePixelDataHeight) % texturePixelDataHeight) * textureRawStride;
            int nextYTextureIndex;
            if ((int)yTexture == texturePixelDataHeight - 1)
                nextYTextureIndex = yTextureIndex;
            else
                nextYTextureIndex = (((int)yTexture + 1 + texturePixelDataHeight) % texturePixelDataHeight) * textureRawStride;

            if (x < width && x >= 0 && y < height && y >= 0)
            {

                pixelData[xIndex + yIndex] =
                    getFilteredColor(xTextureMantissa, yTextureMantissa, xTextureIndex, yTextureIndex, nextXTextureIndex, nextYTextureIndex, 0);

                pixelData[xIndex + yIndex + 1] =
                    getFilteredColor(xTextureMantissa, yTextureMantissa, xTextureIndex, yTextureIndex, nextXTextureIndex, nextYTextureIndex, 1);

                pixelData[xIndex + yIndex + 2] =
                    getFilteredColor(xTextureMantissa, yTextureMantissa, xTextureIndex, yTextureIndex, nextXTextureIndex, nextYTextureIndex, 2);
                if (inGrayscale)
                {
                    pixelData[xIndex + yIndex] = pixelData[xIndex + yIndex + 1] = pixelData[xIndex + yIndex + 2] =
                        (byte)((
                        0.299 * (double)pixelData[xIndex + yIndex] +
                        0.587 * (double)pixelData[xIndex + yIndex + 1] +
                        0.114 * (double)pixelData[xIndex + yIndex + 2]
                        ));
                }
            }
        }

        private byte getFilteredColor(double xTextureMantissa, double yTextureMantissa, int xTextureIndex, int yTextureIndex, int nextXTextureIndex, int nextYTextureIndex, int displacement)
        {
            return (byte)(
                        (1 - yTextureMantissa) * (
                        (1 - xTextureMantissa) * (double)texturePixelData[(xTextureIndex + yTextureIndex + displacement)] +
                        (xTextureMantissa) * (double)texturePixelData[(nextXTextureIndex + yTextureIndex + displacement)]
                        ) + yTextureMantissa * (
                        (1 - xTextureMantissa) * (double)texturePixelData[(xTextureIndex + nextYTextureIndex + displacement)] +
                        (xTextureMantissa) * (double)texturePixelData[(nextXTextureIndex + nextYTextureIndex + displacement)]
                        )
                        );
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
        private void redrawPolygon(GKPolygon drawnPolygon, double tilt, Color? _edgeColor = null, Color? _verticeInsideColor = null, Color? _verticeBorderColor = null)
        {
            if (drawnPolygon.vertices.Count >= 1)
            {
                Color edgeColor = _edgeColor == null ? Colors.DarkGray : (Color)_edgeColor;
                Color verticeInsideColor = _verticeInsideColor == null ? Colors.White : (Color)_verticeInsideColor;
                Color verticeBorderColor = _verticeBorderColor == null ? Colors.Black : (Color)_verticeBorderColor;

                Vertice first = drawnPolygon.vertices[0];
                GKPolygon tiltedPolygon = tiltPolygon(drawnPolygon, tilt);

                fillPolygon(tiltedPolygon, tilt);
            }

        }

        private GKPolygon tiltPolygon(GKPolygon drawnPolygon, double tilt)
        {
            Point middle = new Point((drawnPolygon.vertices[0].coords.X + drawnPolygon.vertices[drawnPolygon.vertices.Count / 2].coords.X) / 2,
             (drawnPolygon.vertices[0].coords.Y + drawnPolygon.vertices[drawnPolygon.vertices.Count / 2].coords.Y) / 2);
            GKPolygon tiltedPolygon = new GKPolygon(drawnPolygon);
            for (int i = 0; i < drawnPolygon.vertices.Count; i++)
            {
                tiltedPolygon.vertices[i].coords = getTiltedPoint(middle, tiltedPolygon.vertices[i].coords, tilt);
            }
            return tiltedPolygon;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Point getTiltedPoint(Point middle, Point point, double tilt)
        {
            Polar tiltedPoint = Global.CartesianToPolar(middle, point);
            tiltedPoint.angle += tilt * Math.PI / 180;
            return tiltedPoint.toCartesian();
        }



        internal void redrawCurrentPolyline(GKPolyline polyline)
        {
            redrawPolyline(polyline, Colors.LightBlue, Colors.White, Colors.DarkBlue);
        }
        public void fillPolygon(GKPolygon polygon, double tilt)
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
            Point middle = new Point((polygon.vertices[0].coords.X + polygon.vertices[polygon.vertices.Count / 2].coords.X) / 2,
                                    (polygon.vertices[0].coords.Y + polygon.vertices[polygon.vertices.Count / 2].coords.Y) / 2);
            GKPolygon originalPolygon = tiltPolygon(polygon, 360 - tilt);
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
                    if (filterRotation)
                    {
                        Parallel.For((int)AETList[j].x, (int)AETList[j + 1].x, k =>
                        {
                            Point tiltedPoint = getTiltedPoint(middle, new Point(k, i), -tilt);
                            SetFilteredPixelFromTexture(k, i, tiltedPoint.X - originalPolygon.vertices[0].coords.X, tiltedPoint.Y - originalPolygon.vertices[0].coords.Y);
                        });
                    }
                    else
                    {

                        Parallel.For((int)AETList[j].x, (int)AETList[j + 1].x, k =>
                        {
                            Point tiltedPoint = getTiltedPoint(middle, new Point(k, i), -tilt);
                            SetPixelFromTexture((int)k, i, (int)tiltedPoint.X - (int)originalPolygon.vertices[0].coords.X, (int)tiltedPoint.Y - (int)originalPolygon.vertices[0].coords.Y);
                        });
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
