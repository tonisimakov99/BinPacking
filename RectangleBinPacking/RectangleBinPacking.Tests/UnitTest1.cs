namespace RectangleBinPacking.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ShelfBinPackNotThrowExceptionsOnFirstInsertTest()
        {
            Assert.DoesNotThrow(() =>
            {
                var shelfBinPack = new ShelfBinPack<int>(256, 256, false, ShelfChoiceHeuristic.ShelfNextFit);
                var result = shelfBinPack.Insert(0, 10, 10);
            });
        }

        [Test]
        public void ShelfBinPackCorrectOnFirstInsertTest()
        {
            var shelfBinPack = new ShelfBinPack<int>(256, 256, false, ShelfChoiceHeuristic.ShelfNextFit);
            var result = shelfBinPack.Insert(0, 10, 10);
        }
    }
}