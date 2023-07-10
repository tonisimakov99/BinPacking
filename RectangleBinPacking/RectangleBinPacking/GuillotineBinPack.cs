using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace RectangleBinPacking
{
    public class GuillotineBinPack
    {
        public GuillotineBinPack(int width, int height)
        {
            Init(width, height);
        }

        private void Init(int width, int height)
        {
            binWidth = width;
            binHeight = height;

#if DEBUG
            disjointRects.Clear();
#endif

            usedRectangles.Clear();

            Rect n = new Rect();
            n.x = 0;
            n.y = 0;
            n.width = width;
            n.height = height;

            freeRectangles.Clear();
            freeRectangles.Add(n);
        }

        public Rect Insert(int width, int height, bool merge, FreeRectChoiceHeuristic rectChoice, GuillotineSplitHeuristic splitMethod)
        {
            int freeNodeIndex = 0;
            Rect newRect = FindPositionForNewNode(width, height, rectChoice, ref freeNodeIndex);
            if (newRect.height == 0)
            {
                return newRect;
            }

            SplitFreeRectByHeuristic(freeRectangles[freeNodeIndex], newRect, splitMethod);
            freeRectangles.RemoveAt(freeNodeIndex);

            if (merge)
            {
                MergeFreeList();
            }

            usedRectangles.Add(newRect);

#if DEBUG
            Debug.Assert(disjointRects.Add(newRect) == true);
#endif
            return newRect;
        }

        public void Insert(List<RectSize> rects, bool merge, FreeRectChoiceHeuristic rectChoice, GuillotineSplitHeuristic splitMethod)
        {
            int bestFreeRect = 0;
            int bestRect = 0;
            bool bestFlipped = false;

            while (rects.Count > 0)
            {
                int bestScore = int.MaxValue;

                for (int i = 0; i < freeRectangles.Count; ++i)
                {
                    for (int j = 0; j < rects.Count; ++j)
                    {
                        if (rects[j].width == freeRectangles[i].width && rects[j].height == freeRectangles[i].height)
                        {
                            bestFreeRect = (int)i;
                            bestRect = (int)j;
                            bestFlipped = false;
                            bestScore = int.MinValue;
                            i = freeRectangles.Count;
                            break;
                        }
                        else if (rects[j].height == freeRectangles[i].width && rects[j].width == freeRectangles[i].height)
                        {
                            bestFreeRect = (int)i;
                            bestRect = (int)j;
                            bestFlipped = true;
                            bestScore = int.MinValue;
                            i = freeRectangles.Count;
                            break;
                        }
                        else if (rects[j].width <= freeRectangles[i].width && rects[j].height <= freeRectangles[i].height)
                        {
                            int score = ScoreByHeuristic(rects[j].width, rects[j].height, freeRectangles[i], rectChoice);
                            if (score < bestScore)
                            {
                                bestFreeRect = (int)i;
                                bestRect = (int)j;
                                bestFlipped = false;
                                bestScore = score;
                            }
                        }
                        else if (rects[j].height <= freeRectangles[i].width && rects[j].width <= freeRectangles[i].height)
                        {
                            int score = ScoreByHeuristic(rects[j].height, rects[j].width, freeRectangles[i], rectChoice);
                            if (score < bestScore)
                            {
                                bestFreeRect = (int)i;
                                bestRect = (int)j;
                                bestFlipped = true;
                                bestScore = score;
                            }
                        }
                    }
                }

                if (bestScore == int.MaxValue)
                {
                    return;
                }

                Rect newNode = new Rect();
                newNode.x = freeRectangles[bestFreeRect].x;
                newNode.y = freeRectangles[bestFreeRect].y;
                newNode.width = rects[bestRect].width;
                newNode.height = rects[bestRect].height;

                if (bestFlipped)
                {
                    var swap = newNode.width;
                    newNode.width = newNode.height;
                    newNode.height = swap;
                }

                SplitFreeRectByHeuristic(freeRectangles[bestFreeRect], newNode, splitMethod);
                freeRectangles.RemoveAt(bestFreeRect);

                rects.RemoveAt(bestRect);

                if (merge)
                {
                    MergeFreeList();
                }

                usedRectangles.Add(newNode);

#if DEBUG
                Debug.Assert(disjointRects.Add(newNode) == true);
#endif
            }
        }

        public float Occupancy()
        {
            int usedSurfaceArea = 0;
            for (int i = 0; i < usedRectangles.Count; ++i)
            {
                usedSurfaceArea += usedRectangles[i].width * usedRectangles[i].height;
            }

            return (float)usedSurfaceArea / (binWidth * binHeight);
        }

        public List<Rect> GetFreeRectangles()
        {
            return new List<Rect>(freeRectangles);
        }

        public List<Rect> GetUsedRectangles()
        {
            return new List<Rect>(usedRectangles);
        }

        public void MergeFreeList()
        {
#if DEBUG
            DisjointRectCollection test = new DisjointRectCollection();
            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                Debug.Assert(test.Add(freeRectangles[i]) == true);
            }
#endif

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                for (int j = i + 1; j < freeRectangles.Count; ++j)
                {
                    if (freeRectangles[i].width == freeRectangles[j].width && freeRectangles[i].x == freeRectangles[j].x)
                    {
                        if (freeRectangles[i].y == freeRectangles[j].y + freeRectangles[j].height)
                        {
                            freeRectangles[i].y -= freeRectangles[j].height;
                            freeRectangles[i].height += freeRectangles[j].height;
                            freeRectangles.RemoveAt(j);
                            --j;
                        }
                        else if (freeRectangles[i].y + freeRectangles[i].height == freeRectangles[j].y)
                        {
                            freeRectangles[i].height += freeRectangles[j].height;
                            freeRectangles.RemoveAt(j);
                            --j;
                        }
                    }
                    else if (freeRectangles[i].height == freeRectangles[j].height && freeRectangles[i].y == freeRectangles[j].y)
                    {
                        if (freeRectangles[i].x == freeRectangles[j].x + freeRectangles[j].width)
                        {
                            freeRectangles[i].x -= freeRectangles[j].width;
                            freeRectangles[i].width += freeRectangles[j].width;
                            freeRectangles.RemoveAt(j);
                            --j;
                        }
                        else if (freeRectangles[i].x + freeRectangles[i].width == freeRectangles[j].x)
                        {
                            freeRectangles[i].width += freeRectangles[j].width;
                            freeRectangles.RemoveAt(j);
                            --j;
                        }
                    }
                }
            }

#if DEBUG
            test.Clear();
            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                Debug.Assert(test.Add(freeRectangles[i]) == true);
            }
#endif
        }

        private int binWidth;
        private int binHeight;

        private List<Rect> usedRectangles = new List<Rect>();

        private List<Rect> freeRectangles = new List<Rect>();

#if DEBUG
        private DisjointRectCollection disjointRects = new DisjointRectCollection();
#endif

        private Rect FindPositionForNewNode(int width, int height, FreeRectChoiceHeuristic rectChoice, ref int nodeIndex)
        {
            Rect bestNode = new Rect();

            int bestScore = int.MaxValue;

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                if (width == freeRectangles[i].width && height == freeRectangles[i].height)
                {
                    bestNode.x = freeRectangles[i].x;
                    bestNode.y = freeRectangles[i].y;
                    bestNode.width = width;
                    bestNode.height = height;
                    bestScore = int.MinValue;
                    nodeIndex = (int)i;
#if DEBUG
                    Debug.Assert(disjointRects.Disjoint(bestNode));
#endif
                    break;
                }
                else if (height == freeRectangles[i].width && width == freeRectangles[i].height)
                {
                    bestNode.x = freeRectangles[i].x;
                    bestNode.y = freeRectangles[i].y;
                    bestNode.width = height;
                    bestNode.height = width;
                    bestScore = int.MinValue;
                    nodeIndex = (int)i;
#if DEBUG
                    Debug.Assert(disjointRects.Disjoint(bestNode));
#endif
                    break;
                }
                else if (width <= freeRectangles[i].width && height <= freeRectangles[i].height)
                {
                    int score = ScoreByHeuristic(width, height, freeRectangles[i], rectChoice);

                    if (score < bestScore)
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = width;
                        bestNode.height = height;
                        bestScore = score;
                        nodeIndex = (int)i;
#if DEBUG
                        Debug.Assert(disjointRects.Disjoint(bestNode));
#endif
                    }
                }
                else if (height <= freeRectangles[i].width && width <= freeRectangles[i].height)
                {
                    int score = ScoreByHeuristic(height, width, freeRectangles[i], rectChoice);

                    if (score < bestScore)
                    {
                        bestNode.x = freeRectangles[i].x;
                        bestNode.y = freeRectangles[i].y;
                        bestNode.width = height;
                        bestNode.height = width;
                        bestScore = score;
                        nodeIndex = (int)i;
#if DEBUG
                        Debug.Assert(disjointRects.Disjoint(bestNode));
#endif
                    }
                }
            }
            return bestNode;
        }

        private static int ScoreByHeuristic(int width, int height, Rect freeRect, FreeRectChoiceHeuristic rectChoice)
        {
            switch (rectChoice)
            {
                case FreeRectChoiceHeuristic.RectBestAreaFit:
                    return ScoreBestAreaFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectBestShortSideFit:
                    return ScoreBestShortSideFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectBestLongSideFit:
                    return ScoreBestLongSideFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectWorstAreaFit:
                    return ScoreWorstAreaFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectWorstShortSideFit:
                    return ScoreWorstShortSideFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectWorstLongSideFit:
                    return ScoreWorstLongSideFit(width, height, freeRect);
                default:
                    Debug.Assert(false);
                    return int.MaxValue;
            }
        }

        private static int ScoreBestAreaFit(int width, int height, Rect freeRect)
        {
            return freeRect.width * freeRect.height - width * height;
        }

        private static int ScoreBestShortSideFit(int width, int height, Rect freeRect)
        {
            int leftoverHoriz = Math.Abs(freeRect.width - width);
            int leftoverVert = Math.Abs(freeRect.height - height);
            int leftover = Math.Min(leftoverHoriz, leftoverVert);
            return leftover;
        }

        private static int ScoreBestLongSideFit(int width, int height, Rect freeRect)
        {
            int leftoverHoriz = Math.Abs(freeRect.width - width);
            int leftoverVert = Math.Abs(freeRect.height - height);
            int leftover = Math.Max(leftoverHoriz, leftoverVert);
            return leftover;
        }

        private static int ScoreWorstAreaFit(int width, int height, Rect freeRect)
        {
            return -ScoreBestAreaFit(width, height, freeRect);
        }

        private static int ScoreWorstShortSideFit(int width, int height, Rect freeRect)
        {
            return -ScoreBestShortSideFit(width, height, freeRect);
        }

        private static int ScoreWorstLongSideFit(int width, int height, Rect freeRect)
        {
            return -ScoreBestLongSideFit(width, height, freeRect);
        }

        private void SplitFreeRectByHeuristic(Rect freeRect, Rect placedRect, GuillotineSplitHeuristic method)
        {
            int w = freeRect.width - placedRect.width;
            int h = freeRect.height - placedRect.height;

            bool splitHorizontal;
            switch (method)
            {
                case GuillotineSplitHeuristic.SplitShorterLeftoverAxis:
                    splitHorizontal = (w <= h);
                    break;
                case GuillotineSplitHeuristic.SplitLongerLeftoverAxis:
                    splitHorizontal = (w > h);
                    break;
                case GuillotineSplitHeuristic.SplitMinimizeArea:
                    splitHorizontal = (placedRect.width * h > w * placedRect.height);
                    break;
                case GuillotineSplitHeuristic.SplitMaximizeArea:
                    splitHorizontal = (placedRect.width * h <= w * placedRect.height);
                    break;
                case GuillotineSplitHeuristic.SplitShorterAxis:
                    splitHorizontal = (freeRect.width <= freeRect.height);
                    break;
                case GuillotineSplitHeuristic.SplitLongerAxis:
                    splitHorizontal = (freeRect.width > freeRect.height);
                    break;
                default:
                    splitHorizontal = true;
                    Debug.Assert(false);
                    break;
            }

            SplitFreeRectAlongAxis(freeRect, placedRect, splitHorizontal);
        }

        private void SplitFreeRectAlongAxis(Rect freeRect, Rect placedRect, bool splitHorizontal)
        {
            Rect bottom = new Rect();
            bottom.x = freeRect.x;
            bottom.y = freeRect.y + placedRect.height;
            bottom.height = freeRect.height - placedRect.height;

            Rect right = new Rect();
            right.x = freeRect.x + placedRect.width;
            right.y = freeRect.y;
            right.width = freeRect.width - placedRect.width;

            if (splitHorizontal)
            {
                bottom.width = freeRect.width;
                right.height = placedRect.height;
            }
            else 
            {
                bottom.width = placedRect.width;
                right.height = freeRect.height;
            }

            if (bottom.width > 0 && bottom.height > 0)
            {
                freeRectangles.Add(bottom);
            }
            if (right.width > 0 && right.height > 0)
            {
                freeRectangles.Add(right);
            }
#if DEBUG
            Debug.Assert(disjointRects.Disjoint(bottom));
            Debug.Assert(disjointRects.Disjoint(right));
#endif
        }
    }

}


