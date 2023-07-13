using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;

namespace RectangleBinPacking
{
    public class SkylineBinPack<TId> : Algorithm<TId> where TId : struct
    {
        public SkylineBinPack(int width, int height, bool useWasteMap, LevelChoiceHeuristic method) : base(width, height)
        {
            this.useWasteMap = useWasteMap;
            this.method = method;

#if DEBUG
            disjointRects.Clear();
#endif

            usedSurfaceArea = 0;
            skyLine.Clear();
            SkylineNode node = new SkylineNode();
            node.x = 0;
            node.y = 0;
            node.width = width;
            skyLine.Add(node);

            if (useWasteMap)
            {
                wasteMap = new GuillotineBinPack<TId>(width, height, true, FreeRectChoiceHeuristic.RectBestShortSideFit, GuillotineSplitHeuristic.SplitMaximizeArea);
                //wasteMap.GetFreeRectangles().Clear();
            }
        }

        public override InsertResult? Insert(TId id, int width, int height)
        {
            var insertResult = wasteMap.Insert(id, width, height);
#if DEBUG
            Debug.Assert(disjointRects.Disjoint(new Rect() { x = insertResult.X, y = insertResult.Y, height = height, width = width }));
#endif
            if (insertResult.height != 0)
            {
                Rect newNode = new Rect();
                newNode.x = insertResult.x;
                newNode.y = insertResult.y;
                newNode.width = insertResult.width;
                newNode.height = insertResult.height;
                usedSurfaceArea += width * height;
#if DEBUG
                Debug.Assert(disjointRects.Disjoint(newNode));
                disjointRects.Add(newNode);
#endif
                return newNode;
            }

            switch (method)
            {
                case LevelChoiceHeuristic.LevelBottomLeft:
                    return InsertBottomLeft(width, height);
                case LevelChoiceHeuristic.LevelMinWasteFit:
                    return InsertMinWaste(width, height);
                default:
                    Debug.Assert(false);
                    return insertResult;
            }
        }

        public float Occupancy()
        {
            return (float)usedSurfaceArea / (width * height);
        }

#if DEBUG
        private DisjointRectCollection disjointRects = new DisjointRectCollection();
#endif

        private class SkylineNode
        {
            public int x;

            public int y;

            public int width;
        }

        private List<SkylineNode> skyLine = new List<SkylineNode>();

        private int usedSurfaceArea;

        private bool useWasteMap;
        private readonly LevelChoiceHeuristic method;
        private GuillotineBinPack<TId> wasteMap;

        private InsertResult? InsertBottomLeft(int width, int height)
        {
            int bestHeight = 0;
            int bestWidth = 0;
            int bestIndex = 0;
            Rect newNode = FindPositionForNewNodeBottomLeft(width, height, ref bestHeight, ref bestWidth, ref bestIndex);

            if (bestIndex != -1)
            {
#if DEBUG
                Debug.Assert(disjointRects.Disjoint(newNode));
#endif
                AddSkylineLevel(bestIndex, newNode);

                usedSurfaceArea += width * height;
#if DEBUG
                disjointRects.Add(newNode);
#endif
            }
            else
            {
                newNode = null;
            }

            return newNode;
        }

        private InsertResult? InsertMinWaste(int width, int height)
        {
            int bestHeight = 0;
            int bestWastedArea = 0;
            int bestIndex = 0;
            Rect newNode = FindPositionForNewNodeMinWaste(width, height, ref bestHeight, ref bestWastedArea, ref bestIndex);

            if (bestIndex != -1)
            {
#if DEBUG
                Debug.Assert(disjointRects.Disjoint(newNode));
#endif

                AddSkylineLevel(bestIndex, newNode);

                usedSurfaceArea += width * height;
#if DEBUG
                disjointRects.Add(newNode);
#endif
            }
            else
            {
                newNode = null;
            }

            return newNode;
        }
        private Rect FindPositionForNewNodeMinWaste(int width, int height, ref int bestHeight, ref int bestWastedArea, ref int bestIndex)
        {
            bestHeight = int.MaxValue;
            bestWastedArea = int.MaxValue;
            bestIndex = -1;
            Rect newNode = new Rect();

            for (int i = 0; i < skyLine.Count; ++i)
            {
                int y = 0;
                int wastedArea = 0;

                if (RectangleFits((int)i, width, height, ref y, ref wastedArea))
                {
                    if (wastedArea < bestWastedArea || (wastedArea == bestWastedArea && y + height < bestHeight))
                    {
                        bestHeight = y + height;
                        bestWastedArea = wastedArea;
                        bestIndex = (int)i;
                        newNode.x = skyLine[i].x;
                        newNode.y = y;
                        newNode.width = width;
                        newNode.height = height;
#if DEBUG
                        Debug.Assert(disjointRects.Disjoint(newNode));
#endif
                    }
                }
                if (RectangleFits((int)i, height, width, ref y, ref wastedArea))
                {
                    if (wastedArea < bestWastedArea || (wastedArea == bestWastedArea && y + width < bestHeight))
                    {
                        bestHeight = y + width;
                        bestWastedArea = wastedArea;
                        bestIndex = (int)i;
                        newNode.x = skyLine[i].x;
                        newNode.y = y;
                        newNode.width = height;
                        newNode.height = width;
#if DEBUG
                        Debug.Assert(disjointRects.Disjoint(newNode));
#endif
                    }
                }
            }

            return newNode;
        }

        private Rect FindPositionForNewNodeBottomLeft(int width, int height, ref int bestHeight, ref int bestWidth, ref int bestIndex)
        {
            bestHeight = int.MaxValue;
            bestIndex = -1;
            bestWidth = int.MaxValue;
            Rect newNode = new Rect();
            for (int i = 0; i < skyLine.Count; ++i)
            {
                int y = 0;
                if (RectangleFits((int)i, width, height, ref y))
                {
                    if (y + height < bestHeight || (y + height == bestHeight && skyLine[i].width < bestWidth))
                    {
                        bestHeight = y + height;
                        bestIndex = (int)i;
                        bestWidth = skyLine[i].width;
                        newNode.x = skyLine[i].x;
                        newNode.y = y;
                        newNode.width = width;
                        newNode.height = height;
#if DEBUG
                        Debug.Assert(disjointRects.Disjoint(newNode));
#endif
                    }
                }
                if (RectangleFits((int)i, height, width, ref y))
                {
                    if (y + width < bestHeight || (y + width == bestHeight && skyLine[i].width < bestWidth))
                    {
                        bestHeight = y + width;
                        bestIndex = (int)i;
                        bestWidth = skyLine[i].width;
                        newNode.x = skyLine[i].x;
                        newNode.y = y;
                        newNode.width = height;
                        newNode.height = width;
#if DEBUG
                        Debug.Assert(disjointRects.Disjoint(newNode));
#endif
                    }
                }
            }

            return newNode;
        }

        private bool RectangleFits(int skylineNodeIndex, int width, int height, ref int y)
        {
            int x = skyLine[skylineNodeIndex].x;
            if (x + width > width)
            {
                return false;
            }
            int widthLeft = width;
            int i = skylineNodeIndex;
            y = skyLine[skylineNodeIndex].y;
            while (widthLeft > 0)
            {
                y = Math.Max(y, skyLine[i].y);
                if (y + height > height)
                {
                    return false;
                }
                widthLeft -= skyLine[i].width;
                ++i;
                Debug.Assert(i < (int)skyLine.Count || widthLeft <= 0);
            }
            return true;
        }
        private bool RectangleFits(int skylineNodeIndex, int width, int height, ref int y, ref int wastedArea)
        {
            bool fits = RectangleFits(skylineNodeIndex, width, height, ref y);
            if (fits)
            {
                wastedArea = ComputeWastedArea(skylineNodeIndex, width, height, y);
            }

            return fits;
        }

        private int ComputeWastedArea(int skylineNodeIndex, int width, int height, int y)
        {
            int wastedArea = 0;
            int rectLeft = skyLine[skylineNodeIndex].x;
            int rectRight = rectLeft + width;
            for (; skylineNodeIndex < (int)skyLine.Count && skyLine[skylineNodeIndex].x < rectRight; ++skylineNodeIndex)
            {
                if (skyLine[skylineNodeIndex].x >= rectRight || skyLine[skylineNodeIndex].x + skyLine[skylineNodeIndex].width <= rectLeft)
                {
                    break;
                }

                int leftSide = skyLine[skylineNodeIndex].x;
                int rightSide = Math.Min(rectRight, leftSide + skyLine[skylineNodeIndex].width);
                Debug.Assert(y >= skyLine[skylineNodeIndex].y);
                wastedArea += (rightSide - leftSide) * (y - skyLine[skylineNodeIndex].y);
            }
            return wastedArea;
        }

        private void AddWasteMapArea(int skylineNodeIndex, int width, int height, int y)
        {
            int rectLeft = skyLine[skylineNodeIndex].x;
            int rectRight = rectLeft + width;
            for (; skylineNodeIndex < (int)skyLine.Count && skyLine[skylineNodeIndex].x < rectRight; ++skylineNodeIndex)
            {
                if (skyLine[skylineNodeIndex].x >= rectRight || skyLine[skylineNodeIndex].x + skyLine[skylineNodeIndex].width <= rectLeft)
                {
                    break;
                }

                int leftSide = skyLine[skylineNodeIndex].x;
                int rightSide = Math.Min(rectRight, leftSide + skyLine[skylineNodeIndex].width);
                Debug.Assert(y >= skyLine[skylineNodeIndex].y);

                Rect waste = new Rect();
                waste.x = leftSide;
                waste.y = skyLine[skylineNodeIndex].y;
                waste.width = rightSide - leftSide;
                waste.height = y - skyLine[skylineNodeIndex].y;
#if DEBUG
                Debug.Assert(disjointRects.Disjoint(waste));
#endif
                wasteMap.GetFreeRectangles().Add(waste);
            }
        }

        private void AddSkylineLevel(int skylineNodeIndex, Rect rect)
        {
            if (useWasteMap)
            {
                AddWasteMapArea(skylineNodeIndex, rect.width, rect.height, rect.y);
            }

            SkylineNode newNode = new SkylineNode();
            newNode.x = rect.x;
            newNode.y = rect.y + rect.height;
            newNode.width = rect.width;

            skyLine.Insert(skylineNodeIndex, newNode);

            Debug.Assert(newNode.x + newNode.width <= width);
            Debug.Assert(newNode.y <= height);

            for (int i = (skylineNodeIndex + 1); i < skyLine.Count; ++i)
            {
                Debug.Assert(skyLine[i - 1].x <= skyLine[i].x);

                if (skyLine[i].x < skyLine[i - 1].x + skyLine[i - 1].width)
                {
                    int shrink = skyLine[i - 1].x + skyLine[i - 1].width - skyLine[i].x;

                    skyLine[i].x += shrink;
                    skyLine[i].width -= shrink;

                    if (skyLine[i].width <= 0)
                    {
                        skyLine.RemoveAt(i);
                        --i;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            MergeSkylines();
        }

        private void MergeSkylines()
        {
            for (int i = 0; i < skyLine.Count - 1; ++i)
            {
                if (skyLine[i].y == skyLine[i + 1].y)
                {
                    skyLine[i].width += skyLine[i + 1].width;
                    skyLine.RemoveAt((i + 1));
                    --i;
                }
            }
        }
    }

}
