using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GK1
{

    class Carbon
    {
        public const int MAX_WIDTH = 1920;
        public const int MAX_HEIGHT = 1080;
        public int rawStride , width, height;
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

        }
    }
}
