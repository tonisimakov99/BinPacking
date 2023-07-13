using System;

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
            InsertResult? newResult = null;

            if (((width > height && width < shelfHeight) || (width < height && height > shelfHeight)))
            {
                newResult.Rotate = true;
                var swap = height;
                height = width;
                width = swap;
            }
            else
            {
                newResult.Rotate = false;
            }

            if (currentX + width > this.width)
            {
                currentX = 0;
                currentY += shelfHeight;
                shelfHeight = 0;

                if (width < height)
                {
                    var swap = height;
                    height = width;
                    width = swap;
                    newResult.Rotate = !newResult.Rotate;
                }
            }

            if (width > this.width || currentY + height > this.height)
            {
                var swap = height;
                height = width;
                width = swap;
                newResult.Rotate = !newResult.Rotate;
            }

            if (width > this.width || currentY + height > this.height)
            {
                return newResult;
            }

            newResult.X = currentX;
            newResult.Y = currentY;

            currentX += width;
            shelfHeight = Math.Max(shelfHeight, height);

            usedSurfaceArea += width * height;

            return newResult;
        }

        public float Occupancy()
        {
            return (float)usedSurfaceArea / (width * height);
        }

        private int currentX;
        private int currentY;
        private int shelfHeight;

        private int usedSurfaceArea;
    }

}


