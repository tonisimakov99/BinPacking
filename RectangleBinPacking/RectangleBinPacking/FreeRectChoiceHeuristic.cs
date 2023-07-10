using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RectangleBinPacking
{
    public enum FreeRectChoiceHeuristic
    {
        RectBestAreaFit,
        RectBestShortSideFit,
        RectBestLongSideFit,
        RectWorstAreaFit,
        RectWorstShortSideFit,
        RectWorstLongSideFit,
		RectBottomLeftRule,
		RectContactPointRule
    };
}
