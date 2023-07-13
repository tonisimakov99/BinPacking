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
            InsertResult? newResult = null;
            int score1 = int.MaxValue;
            int score2 = int.MaxValue;
            switch (method)
            {
                case FreeRectChoiceHeuristic.RectBestShortSideFit:
                    newResult = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2);
                    break;
                case FreeRectChoiceHeuristic.RectBottomLeftRule:
                    newResult = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);
                    break;
                case FreeRectChoiceHeuristic.RectContactPointRule:
                    newResult = FindPositionForNewNodeContactPoint(width, height, ref score1);
                    break;
                case FreeRectChoiceHeuristic.RectBestLongSideFit:
                    newResult = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1);
                    break;
                case FreeRectChoiceHeuristic.RectBestAreaFit:
                    newResult = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2);
                    break;
            }

            if (newResult == null)
                return null;
            PlaceRect(newResult, width, height);

            return newResult;
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

        private void PlaceRect(InsertResult result, int width, int height)
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

            usedRectangles.Add(new Rect() { x = result.X, y = result.Y, height = height, width = width });
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

        private InsertResult? FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX)
        {
            InsertResult? bestResult = null;

            bestY = int.MaxValue;
            bestX = int.MaxValue;

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int topSideY = freeRectangles[i].y + height;
                    if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].x < bestX))
                    {
                        bestResult.X = freeRectangles[i].x;
                        bestResult.Y = freeRectangles[i].y;
                        bestY = topSideY;
                        bestX = freeRectangles[i].x;
                    }
                }
                if (binAllowFlip && freeRectangles[i].width >= height && freeRectangles[i].height >= width)
                {
                    int topSideY = freeRectangles[i].y + width;
                    if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].x < bestX))
                    {
                        bestResult.X = freeRectangles[i].x;
                        bestResult.Y = freeRectangles[i].y;
                        bestY = topSideY;
                        bestX = freeRectangles[i].x;
                    }
                }
            }

            return bestResult;
        }

        private InsertResult? FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
        {
            InsertResult? bestResult = null;

            bestShortSideFit = int.MaxValue;
            bestLongSideFit = int.MaxValue;

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
                        bestResult.X = freeRectangles[i].x;
                        bestResult.Y = freeRectangles[i].y;
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
                        bestResult.X = freeRectangles[i].x;
                        bestResult.Y = freeRectangles[i].y;
                        bestShortSideFit = flippedShortSideFit;
                        bestLongSideFit = flippedLongSideFit;
                    }
                }
            }

            return bestResult;
        }

        private InsertResult? FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
        {
            InsertResult? bestNode = null;

            bestShortSideFit = int.MaxValue;
            bestLongSideFit = int.MaxValue;

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
                        bestNode.X = freeRectangles[i].x;
                        bestNode.Y = freeRectangles[i].y;
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
                        bestNode.X = freeRectangles[i].x;
                        bestNode.Y = freeRectangles[i].y;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }
            }

            return bestNode;
        }

        private InsertResult? FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit, ref int bestShortSideFit)
        {
            InsertResult? bestNode = null;

            bestAreaFit = int.MaxValue;
            bestShortSideFit = int.MaxValue;

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
                        bestNode.X = freeRectangles[i].x;
                        bestNode.Y = freeRectangles[i].y;
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
                        bestNode.X = freeRectangles[i].x;
                        bestNode.Y = freeRectangles[i].y;
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }
            }

            return bestNode;
        }

        private InsertResult? FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore)
        {
            InsertResult? bestNode = null;

            bestContactScore = -1;

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int score = ContactPointScoreNode(freeRectangles[i].x, freeRectangles[i].y, width, height);
                    if (score > bestContactScore)
                    {
                        bestNode.X = freeRectangles[i].x;
                        bestNode.Y = freeRectangles[i].y;
                        bestContactScore = score;
                    }
                }
                if (binAllowFlip && freeRectangles[i].width >= height && freeRectangles[i].height >= width)
                {
                    int score = ContactPointScoreNode(freeRectangles[i].x, freeRectangles[i].y, height, width);
                    if (score > bestContactScore)
                    {
                        bestNode.X = freeRectangles[i].x;
                        bestNode.Y = freeRectangles[i].y;
                        bestContactScore = score;
                    }
                }
            }

            return bestNode;
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

        private bool SplitFreeNode(Rect freeNode, InsertResult result, int width, int height)
        {
            if (result.X >= freeNode.x + freeNode.width || result.X + width <= freeNode.x || result.Y >= freeNode.y + freeNode.height || result.Y + height <= freeNode.y)
            {
                return false;
            }

            newFreeRectanglesLastSize = newFreeRectangles.Count;

            if (result.X < freeNode.x + freeNode.width && result.X + width > freeNode.x)
            {
                if (result.Y > freeNode.y && result.Y < freeNode.y + freeNode.height)
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


