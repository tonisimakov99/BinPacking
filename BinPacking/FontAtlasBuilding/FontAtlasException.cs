using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontAtlasBuilding
{
    public class FontAtlasException :Exception
    {
        public FontAtlasException()
        {
        }

        public FontAtlasException(string message)
            : base(message)
        {
        }

        public FontAtlasException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
