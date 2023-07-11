namespace RectangleBinPacking
{
    public class ShelfNextFitBinPack<TId> : Algorithm<TId> where TId : struct
    {
        public ShelfNextFitBinPack(int width, int height) : base(width, height)
        {
            currentX = 0;
            currentY = 0;
            shelfHeight = 0;
            usedSurfaceArea = 0;
        }

        public override InsertResult? Insert(TId id, int width, int height)
        {
            Node newNode = new Node();

            if (((width > height && width < shelfHeight) || (width < height && height > shelfHeight)))
            {
                newNode.flipped = true;
                var swap = height;
                height = width;
                width = swap;
            }
            else
            {
                newNode.flipped = false;
            }

            if (currentX + width > binWidth)
            {
                currentX = 0;
                currentY += shelfHeight;
                shelfHeight = 0;

                if (width < height)
                {
                    var swap = height;
                    height = width;
                    width = swap;
                    newNode.flipped = !newNode.flipped;
                }
            }

            if (width > binWidth || currentY + height > binHeight)
            {
                var swap = height;
                height = width;
                width = swap;
                newNode.flipped = !newNode.flipped;
            }

            if (width > binWidth || currentY + height > binHeight)
            {
                return newNode;
            }

            newNode.width = width;
            newNode.height = height;
            newNode.x = currentX;
            newNode.y = currentY;

            currentX += width;
            shelfHeight = Math.Max(shelfHeight, height);

            usedSurfaceArea += width * height;

            return newNode;
        }

        public float Occupancy()
        {
            return (float)usedSurfaceArea / (binWidth * binHeight);
        }

        private int binWidth;
        private int binHeight;

        private int currentX;
        private int currentY;
        private int shelfHeight;

        private int usedSurfaceArea;
    }

}


