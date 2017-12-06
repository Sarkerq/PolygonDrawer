using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GK1
{
    static class Global
    {
        public const double verticeRadius = 4;
        public const int lineWidth = 2;
        public const int statusRadius = 10;
        public const int statusLineSeparRadius = 3;
        public const int statusLineLengthRadius = 6;

        public const double pixelEpsilon = 0.5;
        public const double angleEpsilon = 0.5 * Math.PI / 180;

        static public double Distance(Vertice v1, Vertice v2)
        {
            return Math.Sqrt(Math.Pow(v1.coords.X - v2.coords.X, 2) + Math.Pow(v1.coords.Y - v2.coords.Y, 2));
        }
        static public double Distance(Point x, Point y)
        {
            return Math.Sqrt(Math.Pow(x.X - y.X, 2) + Math.Pow(x.Y - y.Y, 2));
        }
        static public Polar CartesianToPolar(Point x, Point y)
        {
            return new Polar(x, Distance(x, y), AngleAgainstXAxis(x, y));
        }
        static public double AngleAgainstXAxis(Point x, Point y)
        {
            Vector yRelative = y - x;
            return Math.Atan2(yRelative.Y, yRelative.X);
        }
        static public double GetAngle(Vertice left, Vertice target, Vertice right)
        {
            return Global.AngleAgainstXAxis(target.coords, left.coords) - Global.AngleAgainstXAxis(target.coords, right.coords);
        }

        internal static Point Mirror(Vertice target, Edge tmp)
        {
            Polar pol = new Polar(tmp.v1.coords, Distance(target, tmp.v1),  AngleAgainstXAxis(tmp.v1.coords,tmp.v2.coords) - GetAngle(target, tmp.v1, tmp.v2));
            return pol.toCartesian();
        }

        internal static Turn TurnDirection(Vertice v1, Vertice v2, Vertice v3)
        {
            double normalizedCrossProduct = Vertice.CrossProduct(v1 - v2, v3 - v2);
            if (normalizedCrossProduct > 0) return Turn.Left;
            else return Turn.Right;
        }
        public static byte[] ImageSourceToBytes(ImageSource imageSource)
        {
            byte[] pixelByteArray = null;
            var bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null)
            {
                BitmapImage bitmapImage = (BitmapImage)imageSource;


                ////////// Convert the BitmapSource to a new format ////////////
                // Use the BitmapImage created above as the source for a new BitmapSource object
                // which is set to a gray scale format using the FormatConvertedBitmap BitmapSource.                                               
                // Note: New BitmapSource does not cache. It is always pulled when required.

                FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();

                // BitmapSource objects like FormatConvertedBitmap can only have their properties
                // changed within a BeginInit/EndInit block.
                newFormatedBitmapSource.BeginInit();

                // Use the BitmapSource object defined above as the source for this new 
                // BitmapSource (chain the BitmapSource objects together).
                newFormatedBitmapSource.Source = bitmapImage;


                // Set the new format to Gray32Float (grayscale).
                newFormatedBitmapSource.DestinationFormat = PixelFormats.Rgb24;
                newFormatedBitmapSource.EndInit();
                int height = newFormatedBitmapSource.PixelHeight;
                int width = newFormatedBitmapSource.PixelWidth;

                int nStride = (newFormatedBitmapSource.PixelWidth * newFormatedBitmapSource.Format.BitsPerPixel + 7) / 8;
                pixelByteArray = new byte[newFormatedBitmapSource.PixelHeight * nStride];
                newFormatedBitmapSource.CopyPixels(pixelByteArray, nStride, 0);
            }
                return pixelByteArray;
        }
    }

    internal enum Turn
    {
        Left,
        Right
    }
}
