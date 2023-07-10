using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RectangleBinPacking
{
    public class ShelfBinPack
    {

        public ShelfBinPack(int width, int height, bool useWasteMap)
        {
            Init(width, height, useWasteMap);
        }

        public void Init(int width, int height, bool useWasteMap_)
        {
            useWasteMap = useWasteMap_;
            binWidth = width;
            binHeight = height;

            currentY = 0;
            usedSurfaceArea = 0;

            shelves.Clear();
            StartNewShelf(0);

            if (useWasteMap)
            {
                wasteMap = new GuillotineBinPack(width, height);
                wasteMap.GetFreeRectangles().Clear();
            }
        }

        public Rect Insert(int width, int height, ShelfChoiceHeuristic method)
        {
            Rect newNode = new Rect();

            if (useWasteMap)
            {
                newNode = wasteMap.Insert(width, height, true, FreeRectChoiceHeuristic.RectBestShortSideFit, GuillotineSplitHeuristic.SplitMaximizeArea);
                if (newNode.height != 0)
                {
                    usedSurfaceArea += width * height;
                    return newNode;
                }
            }

            switch (method)
            {
                case ShelfChoiceHeuristic.ShelfNextFit:
                    if (FitsOnShelf(shelves[shelves.Count - 1], width, height, true))
                    {
                        AddToShelf(shelves[shelves.Count - 1], width, height, newNode);

                        return newNode;
                    }
                    break;
                case ShelfChoiceHeuristic.ShelfFirstFit:
                    for (int i = 0; i < shelves.Count; ++i)
                    {
                        if (FitsOnShelf(shelves[i], width, height, i == shelves.Count - 1))
                        {
                            AddToShelf(shelves[i], width, height, newNode);

                            return newNode;
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
                                int surfaceArea = ((binWidth - shelves[i].currentX) * shelves[i].height);
                                if (surfaceArea < bestShelfSurfaceArea)
                                {
                                    bestShelf = shelves[i];
                                    bestShelfSurfaceArea = surfaceArea;
                                }
                            }
                        }

                        if (bestShelf != null)
                        {
                            AddToShelf(bestShelf, width, height, newNode);
                            return newNode;
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
                                int surfaceArea = (binWidth - shelves[i].currentX) * shelves[i].height;
                                if (surfaceArea > bestShelfSurfaceArea)
                                {
                                    bestShelf = shelves[i];
                                    bestShelfSurfaceArea = surfaceArea;
                                }
                            }
                        }

                        if (bestShelf != null)
                        {
                            AddToShelf(bestShelf, width, height, newNode);
                            return newNode;
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
                            AddToShelf(bestShelf, width, height, newNode);
                            return newNode;
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
                                int widthDifference = binWidth - shelves[i].currentX - width;
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
                            AddToShelf(bestShelf, width, height, newNode);
                            return newNode;
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
                                int widthDifference = binWidth - shelves[i].currentX - width;
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
                            AddToShelf(bestShelf, width, height, newNode);
                            return newNode;
                        }
                    }
                    break;

            }

            if (width < height && height <= binWidth)
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
                AddToShelf(shelves[shelves.Count - 1], width, height, newNode);

                return newNode;
            }

            return newNode;
        }

        public float Occupancy()
        {
            return (float)usedSurfaceArea / (binWidth * binHeight);
        }

        private int binWidth;
        private int binHeight;

        private int currentY;

        private int usedSurfaceArea;

        private bool useWasteMap;
        private GuillotineBinPack wasteMap;

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
            newNode.width = binWidth - shelf.currentX;
            newNode.height = shelf.height;
            if (newNode.width > 0)
            {
                freeRects.Add(newNode);
            }

            shelf.currentX = binWidth;

            wasteMap.MergeFreeList();
        }

        private bool FitsOnShelf(Shelf shelf, int width, int height, bool canResize)
        {
            int shelfHeight = canResize ? (binHeight - shelf.startY) : shelf.height;
            if ((shelf.currentX + width <= binWidth && height <= shelfHeight) || (shelf.currentX + height <= binWidth && width <= shelfHeight))
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
            if ((width > height && width > binWidth - shelf.currentX) || (width > height && width < shelf.height) || (width < height && height > shelf.height && height <= binWidth - shelf.currentX))
            {
                var swap = height;
                height = width;
                width = swap;
            }
        }

        private void AddToShelf(Shelf shelf, int width, int height, Rect newNode)
        {
            Debug.Assert(FitsOnShelf(shelf, width, height, true));

            RotateToShelf(shelf, ref width, ref height);

            newNode.x = shelf.currentX;
            newNode.y = shelf.startY;
            newNode.width = width;
            newNode.height = height;
            shelf.usedRectangles.Add(newNode);

            shelf.currentX += width;
            Debug.Assert(shelf.currentX <= binWidth);

            shelf.height = Math.Max(shelf.height, height);
            Debug.Assert(shelf.height <= binHeight);

            usedSurfaceArea += width * height;
        }

        private bool CanStartNewShelf(int height)
        {
            return shelves[shelves.Count - 1].startY + shelves[shelves.Count - 1].height + height <= binHeight;
        }

        private void StartNewShelf(int startingHeight)
        {
            if (shelves.Count > 0)
            {
                Debug.Assert(shelves[shelves.Count - 1].height != 0);
                currentY += shelves[shelves.Count - 1].height;

                Debug.Assert(currentY < binHeight);
            }

            Shelf shelf = new Shelf();
            shelf.currentX = 0;
            shelf.height = startingHeight;
            shelf.startY = currentY;

            Debug.Assert(shelf.startY + shelf.height <= binHeight);
            shelves.Add(shelf);
        }
    }

}

