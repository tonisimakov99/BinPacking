using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RectangleBinPacking
{
    public struct Shelf
    {
        public Vector2 Size { get; set; }
        public Vector2 LastPosition { get; set; }

        public bool Fits(Vector2 fit)
        {
            return Size.X - LastPosition.X - fit.X >= 0 && fit.Y <= Size.Y;
        }
    }
}
