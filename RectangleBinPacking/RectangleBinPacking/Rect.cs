namespace RectangleBinPacking
{
    public class Rect
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public bool IsContainedIn(Rect b)
        {
            return x >= b.x && y >= b.y
                && x + width <= b.x + b.width
                && y + height <= b.y + b.height;
        }
    }
}

