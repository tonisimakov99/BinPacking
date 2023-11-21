using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrayBinPacking
{
    internal class Segment
    {
        internal Segment(int position, int length, int binId)
        {
            Position = position;
            Length = length;
            BinId = binId;
        }
        public int BinId { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }
    }
}
