using RectangleBinPacking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontAtlasBuilding
{
    public class FontAtlas
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public Dictionary<char, byte[,]> Data { get; set; } = new Dictionary<char, byte[,]>();
        public Dictionary<char, InsertResult> Positions { get; set; } = new Dictionary<char, InsertResult>();
    }
}
