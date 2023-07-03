using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RectangleBinPacking
{
    public abstract class ShelfAlgorithm
    {
        protected readonly Vector2 binSize;
        protected readonly bool rotate;

        public ShelfAlgorithm(Vector2 binSize, bool rotate)
        {
            this.binSize = binSize;
            this.rotate = rotate;
        }
        public abstract InsertResult? Insert(Vector2 size);
    }
}
