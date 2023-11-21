using ArrayBinPacking;
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
    public class ArrayBinTests
    {
        private int size = 1024;
        private readonly List<(int source, int result)> segments =
            new()
            {
                (10,0),
                (25,10),
                (25,35)
            };


        [Test]
        public void ArrayBinPackCorrectTest()
        {
            var arrayBinPack = new ArrayBinPack<int>(size, 1);

            var sourceWidths = segments.Select(t => t.source).ToArray();
            var resultPosExpected = segments.Select(t => t.result).ToArray();

            var resultPosActual = new List<int>();

            for (var i = 0; i != sourceWidths.Length; i++)
            {
                var result = arrayBinPack.Insert(i, sourceWidths[i]);
                Assert.That(result.HasValue, Is.True, "Результат работы с индексом {0} был false", i);
                resultPosActual.Add(result.Value.Offset);
            }
            Assert.That(resultPosActual, Is.EqualTo(resultPosExpected));
        }
    }
}
