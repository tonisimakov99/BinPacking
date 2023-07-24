using NUnit.Framework;
using RectangleBinPacking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinPackingTests
{
    [TestFixture]
    public class MaxRectsTests
    {
        private (int x, int y) binSize = (40, 40);
        private readonly List<((int width, int height) source, (int x, int y) result)> rects =
            new()
            {
                ((10, 10), (0, 0)),
                ((15, 15), (10, 0)),
                ((25, 5), (0, 10)),
                ((10, 5), (0, 35)),
                ((10, 10), (25, 0)),
                ((10, 15), (10, 15)),
                ((10, 12), (10, 25))
            };
        [Test]
        public void MaxRectsBinPackCorrectTest()
        {
            var maxRectsBinPack = new MaxRectsBinPack<int>(binSize.x, binSize.y, FreeRectChoiceHeuristic.RectBestAreaFit, true);
            var rectsSources = rects.Select(t => t.source).ToArray();
            var rectsExpected = rects.Select(t => t.result).ToArray();

            var rectsActual = new List<(int x, int y)>();

            for (var i = 0; i != rectsSources.Length; i++)
            {
                var result = maxRectsBinPack.Insert(i, rectsSources[i].width, rectsSources[i].height);
                Assert.That(result, Is.Not.Null, "Результат работы с индексом {0} был null", i);
                rectsActual.Add((result.X, result.Y));
            }
            Assert.That(rectsActual, Is.EqualTo(rectsExpected));
        }
    }
}
