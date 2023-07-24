using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace RectangleBinPacking
{
    public class MaxRectsBinPack<TId> : Algorithm<TId> where TId : struct
    {
        public MaxRectsBinPack(int width, int height, FreeRectChoiceHeuristic method, bool allowFlip = true) : base(width, height)
        {
            this.method = method;
            binAllowFlip = allowFlip;

            Rect n = new Rect();
            n.x = 0;
            n.y = 0;
            n.width = width;
            n.height = height;

            usedRectangles.Clear();

            freeRectangles.Clear();
            freeRectangles.Add(n);
        }

        private struct SelectResult
        {
            public Rect? rect;
            public bool rotate;
        }

        public override InsertResult? Insert(TId id, int width, int height)
        {
            SelectResult selectNodeResult = default;
            switch (method)
            {
                case FreeRectChoiceHeuristic.RectBestShortSideFit:
                    selectNodeResult = FindPositionForNewNodeBestShortSideFit(width, height);
                    break;
                case FreeRectChoiceHeuristic.RectBottomLeftRule:
                    selectNodeResult = FindPositionForNewNodeBottomLeft(width, height);
                    break;
                case FreeRectChoiceHeuristic.RectContactPointRule:
                    selectNodeResult = FindPositionForNewNodeContactPoint(width, height);
                    break;
                case FreeRectChoiceHeuristic.RectBestLongSideFit:
                    selectNodeResult = FindPositionForNewNodeBestLongSideFit(width, height);
                    break;
                case FreeRectChoiceHeuristic.RectBestAreaFit:
                    selectNodeResult = FindPositionForNewNodeBestAreaFit(width, height);
                    break;
            }

            if (selectNodeResult.rect == null)
                return null;

            PlaceToRect(selectNodeResult, width, height);

            return new InsertResult()
            {
                Rotate = selectNodeResult.rotate,
                X = selectNodeResult.rect.x,
                Y = selectNodeResult.rect.y,
            };
        }

        public double Occupancy()
        {
            int usedSurfaceArea = 0;
            for (int i = 0; i < usedRectangles.Count; ++i)
            {
                usedSurfaceArea += usedRectangles[i].width * usedRectangles[i].height;
            }

            return (double)usedSurfaceArea / (width * height);
        }

        private readonly FreeRectChoiceHeuristic method;
        private bool binAllowFlip;

        private List<Rect> usedRectangles = new List<Rect>();
        private List<Rect> freeRectangles = new List<Rect>();

        private void PlaceToRect(SelectResult selectResult, int width, int height)
        {
            var splitResult = SplitNode(selectResult, width, height);

            PruneFreeList(selectResult.rect);

            freeRectangles.Remove(selectResult.rect);

            freeRectangles.Add(splitResult.newRectOne);
            freeRectangles.Add(splitResult.newRectTwo);

            usedRectangles.Add(splitResult.used);
        }

        private static int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
        {
            if (i1end < i2start || i2end < i1start)
            {
                return 0;
            }
            return Math.Min(i1end, i2end) - Math.Max(i1start, i2start);
        }

        private int ContactPointScoreNode(int x, int y, int width, int height)
        {
            int score = 0;

            if (x == 0 || x + width == width)
            {
                score += height;
            }
            if (y == 0 || y + height == height)
            {
                score += width;
            }

            for (int i = 0; i < usedRectangles.Count; ++i)
            {
                if (usedRectangles[i].x == x + width || usedRectangles[i].x + usedRectangles[i].width == x)
                {
                    score += CommonIntervalLength(usedRectangles[i].y, usedRectangles[i].y + usedRectangles[i].height, y, y + height);
                }
                if (usedRectangles[i].y == y + height || usedRectangles[i].y + usedRectangles[i].height == y)
                {
                    score += CommonIntervalLength(usedRectangles[i].x, usedRectangles[i].x + usedRectangles[i].width, x, x + width);
                }
            }
            return score;
        }

        private SelectResult FindPositionForNewNodeBottomLeft(int width, int height)
        {
            Rect? bestRect = null;
            var rotate = false;

            var bestY = int.MaxValue;
            var bestX = int.MaxValue;

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int topSideY = freeRectangles[i].y + height;
                    if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].x < bestX))
                    {
                        bestRect = freeRectangles[i];
                        bestY = topSideY;
                        bestX = freeRectangles[i].x;
                    }
                }
                if (binAllowFlip && freeRectangles[i].width >= height && freeRectangles[i].height >= width)
                {
                    int topSideY = freeRectangles[i].y + width;
                    if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].x < bestX))
                    {
                        bestRect = freeRectangles[i];
                        bestY = topSideY;
                        bestX = freeRectangles[i].x;
                    }
                }
            }

            return new SelectResult() { rect = bestRect, rotate = rotate };
        }

        private SelectResult FindPositionForNewNodeBestShortSideFit(int width, int height)
        {
            Rect? bestRect = null;
            var rotate = false;

            var bestShortSideFit = int.MaxValue;
            var bestLongSideFit = int.MaxValue;

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].width - width);
                    int leftoverVert = Math.Abs(freeRectangles[i].height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (shortSideFit < bestShortSideFit || (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit))
                    {
                        bestRect = freeRectangles[i];
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (binAllowFlip && freeRectangles[i].width >= height && freeRectangles[i].height >= width)
                {
                    int flippedLeftoverHoriz = Math.Abs(freeRectangles[i].width - height);
                    int flippedLeftoverVert = Math.Abs(freeRectangles[i].height - width);
                    int flippedShortSideFit = Math.Min(flippedLeftoverHoriz, flippedLeftoverVert);
                    int flippedLongSideFit = Math.Max(flippedLeftoverHoriz, flippedLeftoverVert);

                    if (flippedShortSideFit < bestShortSideFit || (flippedShortSideFit == bestShortSideFit && flippedLongSideFit < bestLongSideFit))
                    {
                        bestRect = freeRectangles[i];
                        bestShortSideFit = flippedShortSideFit;
                        bestLongSideFit = flippedLongSideFit;
                    }
                }
            }

            return new SelectResult() { rect = bestRect, rotate = rotate };
        }

        private SelectResult FindPositionForNewNodeBestLongSideFit(int width, int height)
        {
            Rect? bestRect = null;
            var rotate = false;

            var bestShortSideFit = int.MaxValue;
            var bestLongSideFit = int.MaxValue;

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].width - width);
                    int leftoverVert = Math.Abs(freeRectangles[i].height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestRect = freeRectangles[i];
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (binAllowFlip && freeRectangles[i].width >= height && freeRectangles[i].height >= width)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].width - height);
                    int leftoverVert = Math.Abs(freeRectangles[i].height - width);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestRect = freeRectangles[i];
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }
            }

            return new SelectResult() { rect = bestRect, rotate = rotate };
        }

        private SelectResult FindPositionForNewNodeBestAreaFit(int width, int height)
        {
            Rect? bestRect = null;
            var rotate = false;

            var bestAreaFit = int.MaxValue;
            var bestShortSideFit = int.MaxValue;

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                int areaFit = freeRectangles[i].width * freeRectangles[i].height - width * height;

                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].width - width);
                    int leftoverVert = Math.Abs(freeRectangles[i].height - height);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        rotate = false;
                        bestRect = freeRectangles[i];
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }

                if (binAllowFlip && freeRectangles[i].width >= height && freeRectangles[i].height >= width)
                {
                    int leftoverHoriz = Math.Abs(freeRectangles[i].width - height);
                    int leftoverVert = Math.Abs(freeRectangles[i].height - width);
                    int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        rotate = true;
                        bestRect = freeRectangles[i];
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }
            }

            return new SelectResult() { rect = bestRect, rotate = rotate };
        }

        private SelectResult FindPositionForNewNodeContactPoint(int width, int height)
        {
            Rect? bestRect = null;
            var rotate = false;

            var bestContactScore = -1;

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int score = ContactPointScoreNode(freeRectangles[i].x, freeRectangles[i].y, width, height);
                    if (score > bestContactScore)
                    {
                        bestRect = freeRectangles[i];
                        bestContactScore = score;
                    }
                }
                if (binAllowFlip && freeRectangles[i].width >= height && freeRectangles[i].height >= width)
                {
                    rotate = true;
                    int score = ContactPointScoreNode(freeRectangles[i].x, freeRectangles[i].y, height, width);
                    if (score > bestContactScore)
                    {
                        bestRect = freeRectangles[i];
                        bestContactScore = score;
                    }
                }
            }

            return new SelectResult() { rect = bestRect, rotate = rotate };
        }

        //private void InsertNewFreeRectangle(Rect newFreeRect)
        //{
        //    Debug.Assert(newFreeRect.width > 0);
        //    Debug.Assert(newFreeRect.height > 0);

        //    for (int i = 0; i < newFreeRectanglesLastSize;)
        //    {
        //        if (newFreeRect.IsContainedIn(newFreeRectangles[i]))
        //        {
        //            return;
        //        }

        //        if (newFreeRectangles[i].IsContainedIn(newFreeRect))
        //        {
        //            newFreeRectangles[i] = newFreeRectangles[--newFreeRectanglesLastSize];
        //            newFreeRectangles[newFreeRectanglesLastSize] = newFreeRectangles[newFreeRectangles.Count - 1];
        //            newFreeRectangles.RemoveAt(newFreeRectangles.Count - 1);
        //        }
        //        else
        //        {
        //            ++i;
        //        }
        //    }
        //    newFreeRectangles.Add(newFreeRect);
        //}

        private (Rect used, Rect newRectOne, Rect newRectTwo) SplitNode(SelectResult selectResult, int width, int height)
        {
            if (selectResult.rotate)
            {
                var one = new Rect()
                {
                    x = selectResult.rect.x + height,
                    y = selectResult.rect.y,
                    width = selectResult.rect.width - height,
                    height = selectResult.rect.height
                };

                var two = new Rect()
                {
                    x = selectResult.rect.x,
                    y = selectResult.rect.y + width,
                    width = selectResult.rect.width,
                    height = selectResult.rect.height - width
                };

                return (new Rect() { x = selectResult.rect.x, y = selectResult.rect.y, width = height, height = width }, one, two);
            }
            else
            {
                var one = new Rect()
                {
                    x = selectResult.rect.x + width,
                    y = selectResult.rect.y,
                    width = selectResult.rect.width - width,
                    height = selectResult.rect.height
                };

                var two = new Rect()
                {
                    x = selectResult.rect.x,
                    y = selectResult.rect.y + height,
                    width = selectResult.rect.width,
                    height = selectResult.rect.height - height
                };

                return (new Rect() { x = selectResult.rect.x, y = selectResult.rect.y, width = width, height = height }, one, two);
            }
        }

        private void PruneFreeList(Rect selectedRect)
        {
            for (var i = 0; i != freeRectangles.Count; i++)
            {
                if (selectedRect.x + selectedRect.width == freeRectangles[i].x + freeRectangles[i].width &&
                    selectedRect.y + selectedRect.height == freeRectangles[i].y + freeRectangles[i].height &&
                    selectedRect != freeRectangles[i])
                {
                    if (selectedRect.x < freeRectangles[i].x)
                    {
                        freeRectangles.Add(new Rect()
                        {
                            x = freeRectangles[i].x,
                            y = freeRectangles[i].y,
                            width = freeRectangles[i].width,
                            height = freeRectangles[i].height - selectedRect.height
                        });
                    }
                    else
                    {
                        freeRectangles.Add(new Rect()
                        {
                            x = freeRectangles[i].x,
                            y = freeRectangles[i].y,
                            width = freeRectangles[i].width - selectedRect.width,
                            height = freeRectangles[i].height
                        });
                    }
                    freeRectangles.Remove(freeRectangles[i]);
                    return;
                }
            }
        }
    }

}


