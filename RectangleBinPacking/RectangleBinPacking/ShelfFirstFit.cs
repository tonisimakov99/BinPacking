using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RectangleBinPacking
{
    public class ShelfFirstFit : ShelfAlgorithm
    {
        public ShelfFirstFit(Vector2 binSize, bool rotate) : base(binSize, rotate)
        {
        }

        private List<Shelf> shelves = new List<Shelf>();
        public override InsertResult? Insert(Vector2 size)
        {
            if (shelves.Count == 0)
            {
                if (size.X <= binSize.X && size.Y <= binSize.Y)
                {
                    shelves.Add(new Shelf()
                    {
                        LastPosition = new Vector2(0, 0),
                        Size = new Vector2(binSize.X, size.Y),
                    });
                    return Insert(size);
                }
                return null;
            }

            for (var i = 0; i != shelves.Count; i++)
            {
                if (rotate && size.X < shelves[i].Size.Y && size.X > size.Y && shelves[i].Fits(new Vector2(size.Y, size.X)))
                {
                    var result = new InsertResult() { Rotate = true, Position = shelves[i].LastPosition };
                    shelves[i] = new Shelf()
                    {
                        LastPosition = shelves[i].LastPosition + new Vector2(size.Y, 0),
                        Size = shelves[i].Size,
                    };
                    return result;
                }
                else if (shelves[i].Fits(size))
                {
                    var result = new InsertResult() { Rotate = false, Position = shelves[i].LastPosition };
                    shelves[i] = new Shelf()
                    {
                        LastPosition = shelves[i].LastPosition + new Vector2(size.X, 0),
                        Size = shelves[i].Size,
                    };
                    return result;
                }
            }

            var last = shelves.Last();
            if (binSize.Y - last.LastPosition.Y - last.Size.Y - size.Y >= 0)
            {
                shelves.Add(new Shelf()
                {
                    Size = new Vector2(binSize.X, size.Y),
                    LastPosition = new Vector2(0, last.LastPosition.Y + last.Size.Y)
                });
                return Insert(size);
            }


            return null;
        }
    }
}
