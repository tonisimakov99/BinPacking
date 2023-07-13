using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RectangleBinPacking
{
    public class ShelfBinPack<TId> : Algorithm<TId> where TId : struct
    {
        public ShelfBinPack(int width, int height, bool useWasteMap, ShelfChoiceHeuristic method) : base(width, height)
        {
            this.useWasteMap = useWasteMap;
            this.method = method;
            currentY = 0;
            usedSurfaceArea = 0;

            shelves.Clear();
            StartNewShelf(0);

            if (useWasteMap)
            {
                wasteMap = new GuillotineBinPack<TId>(width, height, true, FreeRectChoiceHeuristic.RectBestShortSideFit, GuillotineSplitHeuristic.SplitMaximizeArea);
                wasteMap.GetFreeRectangles().Clear();
            }
        }

        public override InsertResult? Insert(TId id, int width, int height)
        {
            if (useWasteMap)
            {
                var result = wasteMap.Insert(id, width, height);
                if (result == null)
                    return null;
                else
                {
                    usedSurfaceArea += width * height;
                    return result;
                }
            }
            if (method == ShelfChoiceHeuristic.ShelfNextFit)
            {

            }
            else if (method == ShelfChoiceHeuristic.ShelfFirstFit)
            {

            }
            else
            {

            }
            switch (method)
            {
                case ShelfChoiceHeuristic.ShelfNextFit:
                    if (FitsOnShelf(shelves[shelves.Count - 1], width, height, true))
                    {
                        return AddToShelf(shelves[shelves.Count - 1], width, height);
                    }
                    break;
                case ShelfChoiceHeuristic.ShelfFirstFit:
                    for (int i = 0; i < shelves.Count; ++i)
                    {
                        if (FitsOnShelf(shelves[i], width, height, i == shelves.Count - 1))
                        {
                            return AddToShelf(shelves[i], width, height);
                        }
                    }
                    break;

                case ShelfChoiceHeuristic.ShelfBestAreaFit:
                    {
                        Shelf bestShelf = null;
                        int bestShelfSurfaceArea = -1;
                        for (int i = 0; i < shelves.Count; ++i)
                        {
                            RotateToShelf(shelves[i], ref width, ref height);
                            if (FitsOnShelf(shelves[i], width, height, i == shelves.Count - 1))
                            {
                                int surfaceArea = ((this.width - shelves[i].currentX) * shelves[i].height);
                                if (surfaceArea < bestShelfSurfaceArea)
                                {
                                    bestShelf = shelves[i];
                                    bestShelfSurfaceArea = surfaceArea;
                                }
                            }
                        }

                        if (bestShelf != null)
                        {
                            return AddToShelf(bestShelf, width, height);
                        }
                    }
                    break;

                case ShelfChoiceHeuristic.ShelfWorstAreaFit:
                    {
                        Shelf bestShelf = null;
                        int bestShelfSurfaceArea = -1;
                        for (int i = 0; i < shelves.Count; ++i)
                        {
                            RotateToShelf(shelves[i], ref width, ref height);
                            if (FitsOnShelf(shelves[i], width, height, i == shelves.Count - 1))
                            {
                                int surfaceArea = (width - shelves[i].currentX) * shelves[i].height;
                                if (surfaceArea > bestShelfSurfaceArea)
                                {
                                    bestShelf = shelves[i];
                                    bestShelfSurfaceArea = surfaceArea;
                                }
                            }
                        }

                        if (bestShelf != null)
                        {
                            return AddToShelf(bestShelf, width, height);
                        }
                    }
                    break;

                case ShelfChoiceHeuristic.ShelfBestHeightFit:
                    {
                        Shelf bestShelf = null;
                        int bestShelfHeightDifference = 0x7FFFFFFF;
                        for (int i = 0; i < shelves.Count; ++i)
                        {
                            RotateToShelf(shelves[i], ref width, ref height);
                            if (FitsOnShelf(shelves[i], width, height, i == shelves.Count - 1))
                            {
                                int heightDifference = Math.Max(shelves[i].height - height, 0);
                                Debug.Assert(heightDifference >= 0);

                                if (heightDifference < bestShelfHeightDifference)
                                {
                                    bestShelf = shelves[i];
                                    bestShelfHeightDifference = heightDifference;
                                }
                            }
                        }

                        if (bestShelf != null)
                        {
                            return AddToShelf(bestShelf, width, height);
                        }
                    }
                    break;

                case ShelfChoiceHeuristic.ShelfBestWidthFit:
                    {
                        Shelf bestShelf = null;
                        int bestShelfWidthDifference = 0x7FFFFFFF;
                        for (int i = 0; i < shelves.Count; ++i)
                        {
                            RotateToShelf(shelves[i], ref width, ref height);
                            if (FitsOnShelf(shelves[i], width, height, i == shelves.Count - 1))
                            {
                                int widthDifference = this.width - shelves[i].currentX - width;
                                Debug.Assert(widthDifference >= 0);

                                if (widthDifference < bestShelfWidthDifference)
                                {
                                    bestShelf = shelves[i];
                                    bestShelfWidthDifference = widthDifference;
                                }
                            }
                        }

                        if (bestShelf != null)
                        {
                            return AddToShelf(bestShelf, width, height);
                        }
                    }
                    break;

                case ShelfChoiceHeuristic.ShelfWorstWidthFit:
                    {
                        Shelf bestShelf = null;
                        int bestShelfWidthDifference = -1;
                        for (int i = 0; i < shelves.Count; ++i)
                        {
                            RotateToShelf(shelves[i], ref width, ref height);
                            if (FitsOnShelf(shelves[i], width, height, i == shelves.Count - 1))
                            {
                                int widthDifference = width - shelves[i].currentX - width;
                                Debug.Assert(widthDifference >= 0);

                                if (widthDifference > bestShelfWidthDifference)
                                {
                                    bestShelf = shelves[i];
                                    bestShelfWidthDifference = widthDifference;
                                }
                            }
                        }

                        if (bestShelf != null)
                        {
                            return AddToShelf(bestShelf, width, height);
                        }
                    }
                    break;

            }

            if (width < height && height <= width)
            {
                var swap = height;
                height = width;
                width = swap;
            }

            if (CanStartNewShelf(height))
            {
                if (useWasteMap)
                {
                    MoveShelfToWasteMap(shelves[shelves.Count - 1]);
                }
                StartNewShelf(height);
                Debug.Assert(FitsOnShelf(shelves[shelves.Count - 1], width, height, true));
                return AddToShelf(shelves[shelves.Count - 1], width, height);
            }

            return null;
        }

        public float Occupancy()
        {
            return (float)usedSurfaceArea / (width * height);
        }

        private int currentY;

        private int usedSurfaceArea;

        private bool useWasteMap;
        private readonly ShelfChoiceHeuristic method;
        private GuillotineBinPack<TId> wasteMap;

        private class Shelf
        {
            public int currentX;

            public int startY;

            public int height;

            public List<Rect> usedRectangles = new List<Rect>();
        }

        private List<Shelf> shelves = new List<Shelf>();

        private void MoveShelfToWasteMap(Shelf shelf)
        {
            List<Rect> freeRects = wasteMap.GetFreeRectangles();

            for (int i = 0; i < shelf.usedRectangles.Count; ++i)
            {
                Rect r = shelf.usedRectangles[i];
                Rect node = new Rect();
                node.x = r.x;
                node.y = r.y + r.height;
                node.width = r.width;
                node.height = shelf.height - r.height;
                if (node.height > 0)
                {
                    freeRects.Add(node);
                }
            }
            shelf.usedRectangles.Clear();

            Rect newNode = new Rect();
            newNode.x = shelf.currentX;
            newNode.y = shelf.startY;
            newNode.width = width - shelf.currentX;
            newNode.height = shelf.height;
            if (newNode.width > 0)
            {
                freeRects.Add(newNode);
            }

            shelf.currentX = width;

            wasteMap.MergeFreeList();
        }

        private bool FitsOnShelf(Shelf shelf, int width, int height, bool canResize)
        {
            int shelfHeight = canResize ? (this.height - shelf.startY) : shelf.height;
            if ((shelf.currentX + width <= this.width && height <= shelfHeight) || (shelf.currentX + height <= this.width && width <= shelfHeight))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void RotateToShelf(Shelf shelf, ref int width, ref int height)
        {
            if ((width > height && width > this.width - shelf.currentX) || (width > height && width < shelf.height) || (width < height && height > shelf.height && height <= this.width - shelf.currentX))
            {
                var swap = height;
                height = width;
                width = swap;
            }
        }

        private InsertResult? AddToShelf(Shelf shelf, int width, int height)
        {
            InsertResult? newResult = new InsertResult();
            Debug.Assert(FitsOnShelf(shelf, width, height, true));

            RotateToShelf(shelf, ref width, ref height);

            newResult.X = shelf.currentX;
            newResult.Y = shelf.startY;
            shelf.usedRectangles.Add(new Rect() { x = newResult.X, y = newResult.Y, width = width, height = height });

            shelf.currentX += width;
            Debug.Assert(shelf.currentX <= this.width);

            shelf.height = Math.Max(shelf.height, height);
            Debug.Assert(shelf.height <= this.height);

            usedSurfaceArea += width * height;
            return newResult;
        }

        private bool CanStartNewShelf(int height)
        {
            return shelves[shelves.Count - 1].startY + shelves[shelves.Count - 1].height + height <= height;
        }

        private void StartNewShelf(int startingHeight)
        {
            if (shelves.Count > 0)
            {
                Debug.Assert(shelves[shelves.Count - 1].height != 0);
                currentY += shelves[shelves.Count - 1].height;

                Debug.Assert(currentY < height);
            }

            Shelf shelf = new Shelf();
            shelf.currentX = 0;
            shelf.height = startingHeight;
            shelf.startY = currentY;

            Debug.Assert(shelf.startY + shelf.height <= height);
            shelves.Add(shelf);
        }
    }

}

