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
        internal FontAtlas(Dictionary<char, byte[,]> data, Dictionary<char, InsertResult> positions )
        {
            Data = data;
            Positions = positions;
        }
        public int Width { get => Atlas.GetLength(0); }
        public int Height { get => Atlas.GetLength(1); }

        public Dictionary<char, byte[,]> Data { get; private set; } = new Dictionary<char, byte[,]>();
        public Dictionary<char, InsertResult> Positions { get; private set; } = new Dictionary<char, InsertResult>();

        public byte[,] Atlas { get; internal set; }
    }
}
