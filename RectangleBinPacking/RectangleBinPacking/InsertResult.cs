using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RectangleBinPacking
{
    public struct InsertResult
    {
        public Vector2 Position { get; set; }
        public bool Rotate { get; set; }
    }
}
