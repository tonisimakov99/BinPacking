#BinPacking
##Rectangle bin packing
Now shelf packing and max rects only.

Example:
```
var shelfBinPack = new ShelfBinPack<int>(400, 400, false, ShelfChoiceHeuristic.ShelfNextFit); //TId - for rect identification, in future
var result = shelfBinPack.Insert(0, 10, 50);
```

```
 var maxRectsBinPack = new MaxRectsBinPack<int>(400, 400, FreeRectChoiceHeuristic.RectBestAreaFit, true); //TId - for rect identification, in future
var result =  maxRectsBinPac.Insert(0, 10, 50);
```

```
    public class InsertResult
    {
        public int X { get; set; }         // X - position leftUp
        public int Y { get; set; }         // Y - position leftUp
        public bool Rotate { get; set; }   // True, if rotate in bin
    }
```
