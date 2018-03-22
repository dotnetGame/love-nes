using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes.Host
{
    public interface IHostGraphics
    {
        void DrawPixel(byte x, byte y, uint rgb);

        void Flip();
    }
}
