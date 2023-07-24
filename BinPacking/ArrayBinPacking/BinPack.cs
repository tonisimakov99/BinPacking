using System.Collections.Generic;

namespace ArrayBinPacking
{
    public class BinPack<TId> where TId : struct
    {

        public BinPack(int size)
        {
            freeSegments.Add(new Segment(0, size));
        }

        private List<Segment> freeSegments = new();
        private Dictionary<TId, Segment> segments = new();
        public int? Insert(TId id, int width)
        {
            var freeIndex = SelectFreeWidth(width);
            if (!freeIndex.HasValue)
                return null;

            var freeSegment = freeSegments[freeIndex.Value];
            freeSegments.RemoveAt(freeIndex.Value);

            freeSegments.Add(new Segment(freeSegment.Position + freeSegment.Length - width, freeSegment.Length - width));
            segments[id] = new Segment(freeSegment.Position, width);

            return freeSegment.Position;
        }

        public void FreeSegment(TId id)
        {
            var segment = segments[id];
            segments.Remove(id);
            freeSegments.Add(segment);
        }

        private int? SelectFreeWidth(int width)
        {
            for (var i = 0; i < freeSegments.Count; i++)
            {
                if (freeSegments[i].Length >= width)
                    return i;
            }
            return null;
        }
    }
}