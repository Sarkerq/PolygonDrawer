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
        public const int TICKS_PER_DAY = 24;
        public const int MAX_WIDTH = 1440;
        public const int MAX_HEIGHT = 810;
        public int rawStride, width, height;
        public PixelFormat pf = PixelFormats.Rgb24;
        public byte[] pixelData;

        public byte[] texturePixelData;
        public int texturePixelDataWidth, texturePixelDataHeight;

        public double[] lightPixelVector; //[x,y,z]

        public byte[] disturbancePixelData;
        public int disturbancePixelDataWidth, disturbancePixelDataHeight;

        public byte[] mapPixelData;
        public int mapPixelDataWidth, mapPixelDataHeight;

        public double lightsourceRadius;
        public Color lightsourceColor = Colors.White;

        public Edge[] pixelOwner;
        public Algorithm algorithm;
        public Image lineCarbon;
        BitmapSource bitmap;

        public double f_value, Kd_value, Ks_value, m_value;

        public Carbon(Image _lineCarbon)
        {
            lineCarbon = _lineCarbon;
            algorithm = Algorithm.Bresenham;
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

        public void RefreshPolyline(GKPolyline polyline)
        {
            clear();
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
        private void SetPixelFromTexture(int x, int y)
        {
            if (x < 0 || x > MAX_WIDTH || y < 0 || y > MAX_HEIGHT) return;
            int xIndex = x * 3;
            int yIndex = y * rawStride;

            if (texturePixelDataWidth == 0 || mapPixelDataWidth == 0 || disturbancePixelDataWidth == 0) return;
            int xTextureIndex = (x % texturePixelDataWidth) * 3;
            int textureRawStride = (texturePixelDataWidth * pf.BitsPerPixel + 7) / 8;
            int yTextureIndex = (y % texturePixelDataHeight) * textureRawStride;
            //computeIndices(out xTextureIndex, out yTextureIndex, texturePixelDataWidth, texturePixelDataHeight, x, y);

            int xMapIndex = (x % mapPixelDataWidth) * 3;
            int mapRawStride = (mapPixelDataWidth * pf.BitsPerPixel + 7) / 8;
            int yMapIndex = (y % mapPixelDataHeight) * mapRawStride;


            int xDisturbanceIndex = (x % disturbancePixelDataWidth) * 3;
            int nextXDisturbanceIndex = ((x + 1) % disturbancePixelDataWidth) * 3;
            if (nextXDisturbanceIndex < 0) nextXDisturbanceIndex = 0;
            int disturbanceRawStride = (disturbancePixelDataWidth * pf.BitsPerPixel + 7) / 8;

            int yDisturbanceIndex = (y % disturbancePixelDataHeight) * disturbanceRawStride;
            int nextYDisturbanceIndex = ((y + 1) % disturbancePixelDataHeight) * disturbanceRawStride;
            if (nextYDisturbanceIndex < 0) nextYDisturbanceIndex = 0;


            int xLightIndex = 0;
            int lightRawStride = 0;
            int yLightIndex = 0;
            double[] normalizedVector = new double[3] {
                ((double)(mapPixelData[xMapIndex + yMapIndex]  - 127))/128,
                ((double)(mapPixelData[xMapIndex + yMapIndex + 1]  - 127))/128,
                ((double)(mapPixelData[xMapIndex + yMapIndex + 2]))/ 255};
            reduceToZOne(normalizedVector);
            double dh_x =
                disturbancePixelData[xDisturbanceIndex + yDisturbanceIndex] -
                disturbancePixelData[nextXDisturbanceIndex + yDisturbanceIndex];
            double dh_y =
                disturbancePixelData[xDisturbanceIndex + yDisturbanceIndex] -
                disturbancePixelData[xDisturbanceIndex + nextYDisturbanceIndex]; ;
            double[] disturbedVector = new double[3] {
                -dh_x * f_value,
                -dh_y * f_value,
                (-normalizedVector[0] * dh_x - normalizedVector[1] * dh_y) *   f_value};
            if (lightsourceRadius != 0) normalizeVector(disturbedVector);
            double[] normalizedDisturbedVector = new double[3] {
                normalizedVector[0] +  disturbedVector[0],
                normalizedVector[1] +  disturbedVector[1],
                normalizedVector[2] +  disturbedVector[2] };
            normalizeVector(normalizedDisturbedVector);

            double[] thispixelLighting;
            if (lightsourceRadius == 0)
            {
                thispixelLighting = lightPixelVector;
            }
            else
            {
                thispixelLighting = new double[3] {
                lightPixelVector[xLightIndex + yLightIndex] - x,
                y -  lightPixelVector[xLightIndex + yLightIndex + 1],
                lightPixelVector[xLightIndex + yLightIndex + 2] };
                normalizeVector(thispixelLighting);
            }


            double NLAngle =
                normalizedDisturbedVector[0] * thispixelLighting[0] +
                normalizedDisturbedVector[1] * thispixelLighting[1] +
                normalizedDisturbedVector[2] * thispixelLighting[2];
            if (NLAngle > 1) NLAngle = 1;
            if (NLAngle < 0) NLAngle = 0;
            double[] reflectionVector = new double[3]{
                2 * NLAngle * normalizedDisturbedVector[0] - thispixelLighting[0],
                2 * NLAngle * normalizedDisturbedVector[1] - thispixelLighting[1],
                2 * NLAngle * normalizedDisturbedVector[2] - - thispixelLighting[2]
            };
            normalizeVector(reflectionVector);
            double RLAngle = reflectionVector[2];
            if (x < width && x >= 0 && y < height && y >= 0)
            {
                pixelData[xIndex + yIndex] = computeLambertModel(
                    texturePixelData[(xTextureIndex + yTextureIndex)],
                    lightsourceColor.R,
                    NLAngle, RLAngle);


                pixelData[xIndex + yIndex + 1] = computeLambertModel(
                    texturePixelData[xTextureIndex + yTextureIndex + 1],
                    lightsourceColor.G,
                    NLAngle, RLAngle);


                pixelData[xIndex + yIndex + 2] = computeLambertModel(
                    texturePixelData[xTextureIndex + yTextureIndex + 2],
                    lightsourceColor.B,
                    NLAngle, RLAngle);
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
        public void redrawPolyline(GKPolyline drawnPolyline, Color? _edgeColor = null, Color? _verticeInsideColor = null, Color? _verticeBorderColor = null)
        {
            if (drawnPolyline.vertices.Count >= 1)
            {
                Color edgeColor = _edgeColor == null ? Colors.DarkGray : (Color)_edgeColor;
                Color verticeInsideColor = _verticeInsideColor == null ? Colors.White : (Color)_verticeInsideColor;
                Color verticeBorderColor = _verticeBorderColor == null ? Colors.Black : (Color)_verticeBorderColor;

                Vertice first = drawnPolyline.vertices[0];


                for (int i = 0; i < drawnPolyline.edges.Count; i++)
                {
                    drawEdge(drawnPolyline.edges[i], edgeColor);
                }
                for (int i = 0; i < drawnPolyline.vertices.Count; i++)
                    drawVertice(drawnPolyline.vertices[i], verticeBorderColor, verticeInsideColor);

            }

        }
        internal void redrawCurrentPolyline(GKPolyline polyline)
        {
            redrawPolyline(polyline, Colors.LightBlue, Colors.White, Colors.DarkBlue);
        }
        public void fillPolyline(GKPolyline polyline)
        {
            List<ETElement>[] ET = new List<ETElement>[MAX_HEIGHT];
            for (int i = 0; i < MAX_HEIGHT; i++) ET[i] = new List<ETElement>();
            List<AETElement> AET = new List<AETElement>();
            int maxYMin = 0;
            int minYMin = int.MaxValue;
            foreach (Edge e in polyline.edges)
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
                        SetPixelFromTexture(k, i);

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

    }
}
