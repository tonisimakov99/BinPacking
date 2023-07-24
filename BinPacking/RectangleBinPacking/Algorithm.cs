using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RectangleBinPacking
{
    public abstract class Algorithm<TId> where TId : struct
    {
        protected readonly int width;
        protected readonly int height;

        public Algorithm(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public abstract InsertResult? Insert(TId id, int width, int height);
    }
}
