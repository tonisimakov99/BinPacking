using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace RectangleBinPacking
{
    public class GuillotineBinPack<TId> : Algorithm<TId> where TId : struct
    {
        public GuillotineBinPack(int width, int height, bool merge, FreeRectChoiceHeuristic rectChoice, GuillotineSplitHeuristic splitMethod) : base(width, height)
        {

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
            this.merge = merge;
            this.rectChoice = rectChoice;
            this.splitMethod = splitMethod;
        }
        public override InsertResult? Insert(TId id, int width, int height)
        {
            int freeNodeIndex = 0;
            var newResult = FindPositionForNewNode(width, height, rectChoice, ref freeNodeIndex);
            if (newResult == null)
                return null;

            SplitFreeRectByHeuristic(freeRectangles[freeNodeIndex], new Rect() { x = newResult.X, y = newResult.Y, width = width, height = height }, splitMethod);
            freeRectangles.RemoveAt(freeNodeIndex);

            if (merge)
            {
                MergeFreeList();
            }

            usedRectangles.Add(new Rect() { x = newResult.X, y = newResult.Y, width = width, height = height });

#if DEBUG
            Debug.Assert(disjointRects.Add(new Rect() { x = newResult.X, y = newResult.Y, width = width, height = height }) == true);
#endif
            return newResult;
        }

        public float Occupancy()
        {
            int usedSurfaceArea = 0;
            for (int i = 0; i < usedRectangles.Count; ++i)
            {
                usedSurfaceArea += usedRectangles[i].width * usedRectangles[i].height;
            }

            return (float)usedSurfaceArea / (width * height);
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

        private List<Rect> usedRectangles = new List<Rect>();

        private List<Rect> freeRectangles = new List<Rect>();

#if DEBUG
        private DisjointRectCollection disjointRects = new DisjointRectCollection();
        private readonly bool merge;
        private readonly FreeRectChoiceHeuristic rectChoice;
        private readonly GuillotineSplitHeuristic splitMethod;
#endif

        private InsertResult? FindPositionForNewNode(int width, int height, FreeRectChoiceHeuristic rectChoice, ref int nodeIndex)
        {
            var bestResult = new InsertResult();

            int bestScore = int.MaxValue;

            for (int i = 0; i < freeRectangles.Count; ++i)
            {
                if (width == freeRectangles[i].width && height == freeRectangles[i].height)
                {
                    bestResult.X = freeRectangles[i].x;
                    bestResult.Y = freeRectangles[i].y;
                    bestScore = int.MinValue;
                    nodeIndex = (int)i;
#if DEBUG
                    Debug.Assert(disjointRects.Disjoint(new Rect() { x = bestResult.X, y = bestResult.Y, height = height, width = width }));
#endif
                    break;
                }
                else if (height == freeRectangles[i].width && width == freeRectangles[i].height)
                {
                    bestResult.X = freeRectangles[i].x;
                    bestResult.X = freeRectangles[i].y;
                    bestScore = int.MinValue;
                    nodeIndex = (int)i;
#if DEBUG
                    Debug.Assert(disjointRects.Disjoint(new Rect() { x = bestResult.X, y = bestResult.Y, height = height, width = width }));
#endif
                    break;
                }
                else if (width <= freeRectangles[i].width && height <= freeRectangles[i].height)
                {
                    int score = ScoreByHeuristic(width, height, freeRectangles[i], rectChoice);

                    if (score < bestScore)
                    {
                        bestResult.X = freeRectangles[i].x;
                        bestResult.Y = freeRectangles[i].y;
                        bestScore = score;
                        nodeIndex = (int)i;
#if DEBUG
                        Debug.Assert(disjointRects.Disjoint(new Rect() { x = bestResult.X, y = bestResult.Y, height = height, width = width }));
#endif
                    }
                }
                else if (height <= freeRectangles[i].width && width <= freeRectangles[i].height)
                {
                    int score = ScoreByHeuristic(height, width, freeRectangles[i], rectChoice);

                    if (score < bestScore)
                    {
                        bestResult.X = freeRectangles[i].x;
                        bestResult.Y = freeRectangles[i].y;
                        bestScore = score;
                        nodeIndex = (int)i;
#if DEBUG
                        Debug.Assert(disjointRects.Disjoint(new Rect() { x = bestResult.X, y = bestResult.Y, height = height, width = width }));
#endif
                    }
                }
            }
            return bestResult;
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


