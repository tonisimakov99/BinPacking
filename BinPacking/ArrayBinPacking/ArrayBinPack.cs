using System;
using System.Collections.Generic;

namespace ArrayBinPacking
{
    public class ArrayBinPack<TId> where TId : struct
    {

        public ArrayBinPack(int binSize, int binCount)
        {
            this.binSize = binSize;
            this.binCount = binCount;
            //freeSegments.Add(new Segment(0, binSize));
        }

        private Dictionary<int, List<Segment>> bins = new();
        private Dictionary<TId, Segment> segments = new();
        private readonly int binSize;
        private readonly int binCount;
        private int currentBinId = -1;
        public InsertResult? Insert(TId id, int width)
        {
            var freeIndex = SelectFreeWidth(width);
            if (!freeIndex.HasValue)
            {
                if (currentBinId + 1 >= binCount)
                    return null;

                currentBinId++;
                if (!bins.ContainsKey(currentBinId))
                {
                    var list = new List<Segment>
                    {
                        new Segment(0, binSize, currentBinId)
                    };
                    bins.Add(currentBinId, list);
                }
                freeIndex = SelectFreeWidth(width);
                if (!freeIndex.HasValue)
                    return null;
            }

            var freeSegment = bins[currentBinId][freeIndex.Value];
            bins[currentBinId].RemoveAt(freeIndex.Value);

            bins[currentBinId].Add(new Segment(freeSegment.Position + width, freeSegment.Length - width, currentBinId));
            segments[id] = new Segment(freeSegment.Position, width, currentBinId);

            return new InsertResult() { BinId = currentBinId, Offset = freeSegment.Position };
        }

        public void FreeSegment(TId id)
        {
            var segment = segments[id];
            segments.Remove(id);
            bins[segment.BinId].Add(segment);
        }

        private int? SelectFreeWidth(int width)
        {
            if(!bins.ContainsKey(currentBinId))
                return null;
            for (var i = 0; i < bins[currentBinId].Count; i++)
            {
                if (bins[currentBinId][i].Length >= width)
                    return i;
            }
            return null;
        }
    }
}