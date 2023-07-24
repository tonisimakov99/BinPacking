using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RectangleBinPacking
{
    public enum GuillotineSplitHeuristic
    {
        SplitShorterLeftoverAxis,
        SplitLongerLeftoverAxis,
        SplitMinimizeArea,
        SplitMaximizeArea,
        SplitShorterAxis,
        SplitLongerAxis
    };
}
