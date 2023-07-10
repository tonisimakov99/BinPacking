using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RectangleBinPacking
{
    public class SkylineBinPack
    {

        public SkylineBinPack(int width, int height, bool useWasteMap)
        {
            Init(width, height, useWasteMap);
        }

        private void Init(int width, int height, bool useWasteMap_)
        {
            binWidth = width;
            binHeight = height;

            useWasteMap = useWasteMap_;

#if DEBUG
            disjointRects.Clear();
#endif

            usedSurfaceArea = 0;
            skyLine.Clear();
            SkylineNode node = new SkylineNode();
            node.x = 0;
            node.y = 0;
            node.width = binWidth;
            skyLine.Add(node);

            if (useWasteMap)
            {
                wasteMap = new GuillotineBinPack(width, height);
                wasteMap.GetFreeRectangles().Clear();
            }
        }

        public Rect Insert(int width, int height, LevelChoiceHeuristic method)
        {
            Rect node = wasteMap.Insert(width, height, true, FreeRectChoiceHeuristic.RectBestShortSideFit, GuillotineSplitHeuristic.SplitMaximizeArea);
#if DEBUG
            Debug.Assert(disjointRects.Disjoint(node));
#endif
            if (node.height != 0)
            {
                Rect newNode = new Rect();
                newNode.x = node.x;
                newNode.y = node.y;
                newNode.width = node.width;
                newNode.height = node.height;
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
                    return node;
            }
        }

        public float Occupancy()
        {
            return (float)usedSurfaceArea / (binWidth * binHeight);
        }

        private int binWidth;
        private int binHeight;

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
        private GuillotineBinPack wasteMap;

        private Rect InsertBottomLeft(int width, int height)
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

        private Rect InsertMinWaste(int width, int height)
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
            if (x + width > binWidth)
            {
                return false;
            }
            int widthLeft = width;
            int i = skylineNodeIndex;
            y = skyLine[skylineNodeIndex].y;
            while (widthLeft > 0)
            {
                y = Math.Max(y, skyLine[i].y);
                if (y + height > binHeight)
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

            Debug.Assert(newNode.x + newNode.width <= binWidth);
            Debug.Assert(newNode.y <= binHeight);

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
