using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace RectangleBinPacking
{
    public class MaxRectsBinPack
    {
        public MaxRectsBinPack(int width, int height, bool allowFlip = true)
        {
            Init(width, height, allowFlip);
        }

        private void Init(int width, int height, bool allowFlip = true)
        {
            binAllowFlip = allowFlip;
            binWidth = width;
            binHeight = height;

            Rect n = new Rect();
            n.x = 0;
            n.y = 0;
            n.width = width;
            n.height = height;

            usedRectangles.Clear();

            freeRectangles.Clear();
            freeRectangles.Add(n);
        }

        public Rect Insert(int width, int height, FreeRectChoiceHeuristic method)
        {
            Rect newNode = new Rect();
            int score1 = int.MaxValue;
            int score2 = int.MaxValue;
            switch (method)
            {
                case FreeRectChoiceHeuristic.RectBestShortSideFit:
                    newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2);
                    break;
                case FreeRectChoiceHeuristic.RectBottomLeftRule:
                    newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);
                    break;
                case FreeRectChoiceHeuristic.RectContactPointRule:
                    newNode = FindPositionForNewNodeContactPoint(width, height, ref score1);
                    break;
                case FreeRectChoiceHeuristic.RectBestLongSideFit:
                    newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1);
                    break;
                case FreeRectChoiceHeuristic.RectBestAreaFit:
                    newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2);
                    break;
            }

            if (newNode.height == 0)
            {
                return newNode;
            }

            PlaceRect(newNode);

            return newNode;
        }

        public double Occupancy()
        {
            int usedSurfaceArea = 0;
            for (int i = 0; i < usedRectangles.Count; ++i)
            {
                usedSurfaceArea += usedRectangles[i].width * usedRectangles[i].height;
            }

            return (double)usedSurfaceArea / (binWidth * binHeight);
        }

        private int binWidth;
        private int binHeight;

        private bool binAllowFlip;

        private int newFreeRectanglesLastSize;
        private List<Rect> newFreeRectangles = new List<Rect>();

        private List<Rect> usedRectangles = new List<Rect>();
        private List<Rect> freeRectangles = new List<Rect>();

        private Rect ScoreRect(int width, int height, FreeRectChoiceHeuristic method, ref int score1, ref int score2)
        {
            Rect newNode = new Rect();
            score1 = int.MaxValue;
            score2 = int.MaxValue;
            switch (method)
            {
                case FreeRectChoiceHeuristic.RectBestShortSideFit:
                    newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2);
                    break;
                case FreeRectChoiceHeuristic.RectBottomLeftRule:
                    newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);
                    break;
                case FreeRectChoiceHeuristic.RectContactPointRule:
                    newNode = FindPositionForNewNodeContactPoint(width, height, ref score1);
                    score1 = -score1; 
                    break;
                case FreeRectChoiceHeuristic.RectBestLongSideFit:
                    newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1);
                    break;
                case FreeRectChoiceHeuristic.RectBestAreaFit:
                    newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2);
                    break;
            }

            if (newNode.height == 0)
            {
                score1 = int.MaxValue;
                score2 = int.MaxValue;
            }

            return newNode;
        }

        private void PlaceRect(Rect node)
        {
            for (int i = 0; i < freeRectangles.Count;)
            {
                if (SplitFreeNode(freeRectangles[i], node))
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

            usedRectangles.Add(node);
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

            if (x == 0 || x + width == binWidth)
            {
                score += height;
            }
            if (y == 0 || y + height == binHeight)
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

        private Rect FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX)
        {
            Rect bestNode = new Rect();

            bestY = int.MaxValue;
            bestX = int.MaxValue;

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int topSideY = freeRectangles[i].y + height;
                    if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].x < bestX))
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestY = topSideY;
                        bestX = freeRectangles[i].x;
                    }
                }
                if (binAllowFlip && freeRectangles[i].width >= height && freeRectangles[i].height >= width)
                {
                    int topSideY = freeRectangles[i].y + width;
                    if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].x < bestX))
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = height;
                        bestNode.height = width;
                        bestY = topSideY;
                        bestX = freeRectangles[i].x;
                    }
                }
            }

            return bestNode;
        }

        private Rect FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
        {
            Rect bestNode = new Rect();

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
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
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
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = height;
                        bestNode.height = width;
                        bestShortSideFit = flippedShortSideFit;
                        bestLongSideFit = flippedLongSideFit;
                    }
                }
            }

            return bestNode;
        }

        private Rect FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
        {
            Rect bestNode = new Rect();

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
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
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
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = height;
                        bestNode.height = width;
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }
            }

            return bestNode;
        }

        private Rect FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit, ref int bestShortSideFit)
        {
            Rect bestNode = new Rect();

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
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
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
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = height;
                        bestNode.height = width;
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }
            }

            return bestNode;
        }

        private Rect FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore)
        {
            Rect bestNode = new Rect();

            bestContactScore = -1;

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
                {
                    int score = ContactPointScoreNode(freeRectangles[i].x, freeRectangles[i].y, width, height);
                    if (score > bestContactScore)
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestContactScore = score;
                    }
                }
                if (binAllowFlip && freeRectangles[i].width >= height && freeRectangles[i].height >= width)
                {
                    int score = ContactPointScoreNode(freeRectangles[i].x, freeRectangles[i].y, height, width);
                    if (score > bestContactScore)
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = height;
                        bestNode.height = width;
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

        private bool SplitFreeNode(Rect freeNode, Rect usedNode)
        {
            if (usedNode.x >= freeNode.x + freeNode.width || usedNode.x + usedNode.width <= freeNode.x || usedNode.y >= freeNode.y + freeNode.height || usedNode.y + usedNode.height <= freeNode.y)
            {
                return false;
            }

            newFreeRectanglesLastSize = newFreeRectangles.Count;

            if (usedNode.x < freeNode.x + freeNode.width && usedNode.x + usedNode.width > freeNode.x)
            {
                if (usedNode.y > freeNode.y && usedNode.y < freeNode.y + freeNode.height)
                {
                    Rect newNode = freeNode;
                    newNode.height = usedNode.y - newNode.y;
                    InsertNewFreeRectangle(newNode);
                }

                if (usedNode.y + usedNode.height < freeNode.y + freeNode.height)
                {
                    Rect newNode = freeNode;
                    newNode.y = usedNode.y + usedNode.height;
                    newNode.height = freeNode.y + freeNode.height - (usedNode.y + usedNode.height);
                    InsertNewFreeRectangle(newNode);
                }
            }

            if (usedNode.y < freeNode.y + freeNode.height && usedNode.y + usedNode.height > freeNode.y)
            {
                if (usedNode.x > freeNode.x && usedNode.x < freeNode.x + freeNode.width)
                {
                    Rect newNode = freeNode;
                    newNode.width = usedNode.x - newNode.x;
                    InsertNewFreeRectangle(newNode);
                }

                if (usedNode.x + usedNode.width < freeNode.x + freeNode.width)
                {
                    Rect newNode = freeNode;
                    newNode.x = usedNode.x + usedNode.width;
                    newNode.width = freeNode.x + freeNode.width - (usedNode.x + usedNode.width);
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


