using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RectangleBinPacking
{
    public class ShelfNextFit : ShelfAlgorithm
    {
        public ShelfNextFit(Vector2 binSize, bool rotate) : base(binSize, rotate)
        {

        }

        private Vector2 currentPosition = Vector2.Zero;
        private float currentHeight;
        public override InsertResult? Insert(Vector2 size)
        {
            if (currentPosition == Vector2.Zero)
            {
                if (!Fits(size, binSize))
                    return null;

                var result = new InsertResult() { Rotate = false, Position = Vector2.Zero };
                currentPosition.X = size.X;
                currentHeight = size.Y;
                return result;
            }

            if (rotate && size.X < currentHeight && size.X > size.Y && Fits(new Vector2(size.Y, size.X), new Vector2(binSize.X, currentHeight)))
            {
                var result = new InsertResult() { Rotate = true, Position = currentPosition };
                currentPosition.X += size.Y;
                return result;
            }
            else if (Fits(size, new Vector2(binSize.X, currentHeight)))
            {
                var result = new InsertResult() { Rotate = false, Position = currentPosition };
                currentPosition.X += size.X;
                return result;
            }
            else
            {
                if (binSize.Y - currentPosition.Y - currentHeight - size.Y >= 0)
                {
                    currentPosition.Y += currentHeight;
                    currentPosition.X = 0;
                    currentHeight = size.Y;
                    return Insert(size);
                }
            }
            return null;
        }

        private bool Fits(Vector2 fit, Vector2 size)
        {
            return size.X - currentPosition.X - fit.X >= 0 && fit.Y <= size.Y;
        }
    }
}
