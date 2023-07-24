using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrayBinPacking
{
    internal class Segment
    {
        internal Segment(int position, int length)
        {
            Position = position;
            Length = length;
        }
        public int Position { get; set; }
        public int Length { get; set; }
    }
}
