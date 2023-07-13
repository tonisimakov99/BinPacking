
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RectangleBinPacking.Tests
{
    [TestFixture]
    public class Tests
    {
        private (int x, int y) binSize = (40, 40);
        private readonly List<((int width, int height) source, (int x, int y) result)> rects =
            new()
            {
                ((10, 10), (0, 0)),
                ((15, 15), (10, 0)),
                ((5, 10), (25, 0)),
                ((10, 5), (30, 0)),
                ((10, 10), (35, 15)),
                ((10, 15), (10, 15)),
                ((10, 12), (20, 15))
            };

        [OneTimeSetUp]
        public void Setup()
        {

        }

        [Test]
        public void ShelfBinPackCorrectTest()
        {
            var shelfBinPack = new ShelfBinPack<int>(binSize.x, binSize.y, false, ShelfChoiceHeuristic.ShelfNextFit);

            var rectsSources = rects.Select(t => t.source).ToArray();
            var rectsExpected = rects.Select(t => t.result).ToArray();

            var rectsActual = new List<(int x, int y)>();

            for (var i = 0; i != rectsSources.Length; i++)
            {
                var result = shelfBinPack.Insert(i, rectsSources[i].width, rectsSources[i].height);
                Assert.That(result, Is.Not.Null, "Результат работы с индексом {0} был null", i);
                rectsActual.Add((result.X, result.Y));
            }
            Assert.That(rectsActual, Is.EqualTo(rectsExpected));
        }
    }
}