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

        public override InsertResult? Insert(TId id, int width, int height)
        {
            (Rect? rect, bool rotate) selectResult = default;
            switch (method)
            {
                case FreeRectChoiceHeuristic.RectBestShortSideFit:
                    selectResult = FindPositionForNewNodeBestShortSideFit(width, height);
                    break;
                case FreeRectChoiceHeuristic.RectBottomLeftRule:
                    selectResult = FindPositionForNewNodeBottomLeft(width, height);
                    break;
                case FreeRectChoiceHeuristic.RectContactPointRule:
                    selectResult = FindPositionForNewNodeContactPoint(width, height);
                    break;
                case FreeRectChoiceHeuristic.RectBestLongSideFit:
                    selectResult = FindPositionForNewNodeBestLongSideFit(width, height);
                    break;
                case FreeRectChoiceHeuristic.RectBestAreaFit:
                    selectResult = FindPositionForNewNodeBestAreaFit(width, height);
                    break;
            }

            if (selectResult.rect == null)
                return null;

            PlaceToRect(selectResult.rect, width, height);

            return new InsertResult()
            {
                Rotate = selectResult.rotate,
                X = selectResult.rect.x,
                Y = selectResult.rect.y,
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

        private int newFreeRectanglesLastSize;
        private List<Rect> newFreeRectangles = new List<Rect>();

        private List<Rect> usedRectangles = new List<Rect>();
        private List<Rect> freeRectangles = new List<Rect>();

        private void PlaceToRect(Rect result, int width, int height)
        {
            for (int i = 0; i < freeRectangles.Count;)
            {
                if (SplitFreeNode(freeRectangles[i], result, width, height))
                {
                    freeRectangles[i] = freeRectangles[freeRectangles.Count - 1];
                    freeRectangles.RemoveAt(freeRectangles.Count - 1);
                }
                else
                {
                    ++i;
                }
            }

            PruneFreeList();

            usedRectangles.Add(result);
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

        private (Rect? rect, bool rotate) FindPositionForNewNodeBottomLeft(int width, int height)
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

            return (bestRect, rotate);
        }

        private (Rect? rect, bool rotate) FindPositionForNewNodeBestShortSideFit(int width, int height)
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

            return (bestRect, rotate);
        }

        private (Rect? rect, bool rotate) FindPositionForNewNodeBestLongSideFit(int width, int height)
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

            return (bestRect, rotate);
        }

        private (Rect? rect, bool rotate) FindPositionForNewNodeBestAreaFit(int width, int height)
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
                        bestRect = freeRectangles[i];
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }
            }

            return (bestRect, rotate);
        }

        private (Rect? rect, bool rotate) FindPositionForNewNodeContactPoint(int width, int height)
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

            return (bestRect, rotate);
        }

        private void InsertNewFreeRectangle(Rect newFreeRect)
        {
            Debug.Assert(newFreeRect.width > 0);
            Debug.Assert(newFreeRect.height > 0);

            for (int i = 0; i < newFreeRectanglesLastSize;)
            {
                if (newFreeRect.IsContainedIn(newFreeRectangles[i]))
                {
                    return;
                }

                if (newFreeRectangles[i].IsContainedIn(newFreeRect))
                {
                    newFreeRectangles[i] = newFreeRectangles[--newFreeRectanglesLastSize];
                    newFreeRectangles[newFreeRectanglesLastSize] = newFreeRectangles[newFreeRectangles.Count - 1];
                    newFreeRectangles.RemoveAt(newFreeRectangles.Count - 1);
                }
                else
                {
                    ++i;
                }
            }
            newFreeRectangles.Add(newFreeRect);
        }

        private bool SplitFreeNode(Rect freeNode, int width, int height)
        {
            if (result.x >= freeNode.x + freeNode.width || result.y + width <= freeNode.x || result.y >= freeNode.y + freeNode.height || result.y + height <= freeNode.y)
            {
                return false;
            }

            newFreeRectanglesLastSize = newFreeRectangles.Count;

            if (result.x < freeNode.x + freeNode.width && result.x + width > freeNode.x)
            {
                if (result.y > freeNode.y && result.y < freeNode.y + freeNode.height)
                {
                    Rect newNode = freeNode;
                    newNode.height = result.Y - newNode.y;
                    InsertNewFreeRectangle(newNode);
                }

                if (result.Y + height < freeNode.y + freeNode.height)
                {
                    Rect newNode = freeNode;
                    newNode.y = result.Y + height;
                    newNode.height = freeNode.y + freeNode.height - (result.Y + height);
                    InsertNewFreeRectangle(newNode);
                }
            }

            if (result.Y < freeNode.y + freeNode.height && result.Y + height > freeNode.y)
            {
                if (result.X > freeNode.x && result.X < freeNode.x + freeNode.width)
                {
                    Rect newNode = freeNode;
                    newNode.width = result.X - newNode.x;
                    InsertNewFreeRectangle(newNode);
                }

                if (result.X + width < freeNode.x + freeNode.width)
                {
                    Rect newNode = freeNode;
                    newNode.x = result.X + width;
                    newNode.width = freeNode.x + freeNode.width - (result.X + width);
                    InsertNewFreeRectangle(newNode);
                }
            }

            return true;
        }

        private void PruneFreeList()
        {
            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                for (int j = 0; j < newFreeRectangles.Count;)
                {
                    if (newFreeRectangles[j].IsContainedIn(freeRectangles[i]))
                    {
                        newFreeRectangles[j] = newFreeRectangles[newFreeRectangles.Count - 1];
                        newFreeRectangles.RemoveAt(newFreeRectangles.Count - 1);
                    }
                    else
                    {
                        Debug.Assert(!freeRectangles[i].IsContainedIn(newFreeRectangles[j]));

                        ++j;
                    }
                }
            }

            freeRectangles.AddRange(newFreeRectangles);
            newFreeRectangles.Clear();

#if DEBUG
            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                for (int j = i + 1; j < freeRectangles.Count; ++j)
                {
                    Debug.Assert(!freeRectangles[i].IsContainedIn(freeRectangles[j]));
                    Debug.Assert(!freeRectangles[j].IsContainedIn(freeRectangles[i]));
                }
            }
#endif
        }
    }

}


