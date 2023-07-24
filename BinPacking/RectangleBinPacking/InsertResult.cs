using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RectangleBinPacking
{
    public class InsertResult
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Rotate { get; set; }

        public override string ToString()
        {
            return $"x={X}, y={Y}, rotate={Rotate}";
        }
    }
}
