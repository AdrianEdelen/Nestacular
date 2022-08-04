using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nestacular.NESCore.PPUCore
{
    public class Pixel
    {
        public int x;
        public int y;
        public SkiaSharp.SKColor color;

        public Pixel(int x, int y, SKColor color)
        {
            this.x = x;
            this.y = y;
            this.color = color;
        }
    }
}
