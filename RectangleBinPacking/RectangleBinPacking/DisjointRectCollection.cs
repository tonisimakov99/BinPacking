using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RectangleBinPacking
{
    internal class DisjointRectCollection
    {
        public List<Rect> rects = new List<Rect>();

        public bool Add(Rect r)
        {
            if (r.width == 0 || r.height == 0)
            {
                return true;
            }

            if (!Disjoint(r))
            {
                return false;
            }
            rects.Add(r);
            return true;
        }

        public void Clear()
        {
            rects.Clear();
        }

        public bool Disjoint(Rect r)
        {
            if (r.width == 0 || r.height == 0)
            {
                return true;
            }

            for (var i = 0; i < rects.Count; ++i)
            {
                if (!Disjoint(rects[i], r))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Disjoint(Rect a, Rect b)
        {
            if (a.x + a.width <= b.x || b.x + b.width <= a.x || a.y + a.height <= b.y || b.y + b.height <= a.y)
            {
                return true;
            }
            return false;
        }
    }
}
