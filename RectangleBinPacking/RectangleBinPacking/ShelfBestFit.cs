using RectangleBinPacking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RectangleBinPacking
{
    public class ShelfBestFit : PackingAlgorithm
    {
        private List<Shelf> shelves = new List<Shelf>();
        private Dictionary<ShelfMode, Func<List<Shelf>, int, int, Vector2, bool>> compares = new Dictionary<ShelfMode, Func<List<Shelf>, int, int, Vector2, bool>>()
        {
            { ShelfMode.FitToFirst, (shelves,previousBest, two, size) => { return two==0; } },
            { ShelfMode.FitToMinWidth, (shelves,previousBest, two, size) => { return shelves[previousBest].Size.X-shelves[previousBest].LastPosition.X > shelves[two].Size.X-shelves[two].LastPosition.X; } },
            { ShelfMode.FitToMinHeight, (shelves,previousBest, two, size) => { return shelves[previousBest].Size.Y-size.Y > shelves[two].Size.Y-size.Y; } },
            { ShelfMode.FitToMinArea, (shelves,previousBest, two, size) => { return shelves[previousBest].Size.Y*(shelves[previousBest].Size.X-shelves[previousBest].LastPosition.X) > shelves[two].Size.Y*(shelves[two].Size.X-shelves[two].LastPosition.X); } },
            { ShelfMode.FitToMaxWidth, (shelves,previousBest, two, size) => {return shelves[previousBest].Size.X-shelves[previousBest].LastPosition.X < shelves[two].Size.X-shelves[two].LastPosition.X; } },
            { ShelfMode.FitToMaxHeight, (shelves,previousBest, two, size) => {return shelves[previousBest].Size.Y-size.Y < shelves[two].Size.Y-size.Y; } },
            { ShelfMode.FitToMaxArea, (shelves,previousBest, two, size) => { return shelves[previousBest].Size.Y*(shelves[previousBest].Size.X-shelves[previousBest].LastPosition.X) < shelves[two].Size.Y*(shelves[two].Size.X-shelves[two].LastPosition.X); } },
        };

        private Func<List<Shelf>, int, int, Vector2, bool> compare = null;
        public ShelfBestFit(Vector2 binSize, bool rotate, ShelfMode shelfMode) : base(binSize, rotate)
        {
            compare = compares[shelfMode];
        }

        public override InsertResult? Insert(Vector2 size)
        {
            if (shelves.Count == 0)
            {
                if (size.X <= binSize.X && size.Y <= binSize.Y)
                {
                    shelves.Add(new Shelf()
                    {
                        LastPosition = new Vector2(size.X, 0),
                        Size = new Vector2(binSize.X, size.Y),
                    });
                    return new InsertResult() { Rotate = false, Position = new Vector2(0, 0) };
                }
                return null;
            }

            var bestIndex = -1;
            for (var i = 0; i < shelves.Count; i++)
            {
                if (rotate && size.X < shelves[i].Size.Y && size.X > size.Y && shelves[i].Fits(new Vector2(size.Y, size.X)))
                {
                    if (bestIndex == -1)
                        bestIndex = i;

                    if (compare(shelves, bestIndex, i, new Vector2(size.Y, size.X)))
                    {
                        bestIndex = i;
                    }
                }
                else if (shelves[i].Fits(size))
                {
                    if (bestIndex == -1)
                        bestIndex = i;

                    if (compare(shelves, bestIndex, i, size))
                    {
                        bestIndex = i;
                    }
                }
            }

            if (bestIndex == -1)
            {
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

            if (rotate && size.X < shelves[bestIndex].Size.Y && size.X > size.Y && shelves[bestIndex].Fits(new Vector2(size.Y, size.X)))
            {
                var result = new InsertResult() { Rotate = true, Position = shelves[bestIndex].LastPosition };
                shelves[bestIndex] = new Shelf()
                {
                    LastPosition = shelves[bestIndex].LastPosition + new Vector2(size.Y, 0),
                    Size = shelves[bestIndex].Size,
                };
                return result;
            }
            else if (shelves[bestIndex].Fits(size))
            {
                var result = new InsertResult() { Rotate = false, Position = shelves[bestIndex].LastPosition };
                shelves[bestIndex] = new Shelf()
                {
                    LastPosition = shelves[bestIndex].LastPosition + new Vector2(size.X, 0),
                    Size = shelves[bestIndex].Size,
                };
                return result;
            }

            return null;
        }
    }
}
