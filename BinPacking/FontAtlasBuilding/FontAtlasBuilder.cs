using FreeTypeBinding;
using Microsoft.Extensions.Logging;
using RectangleBinPacking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace FontAtlasBuilding
{
    public unsafe class FontAtlasBuilder : IDisposable
    {
        public const string UpperEnglishAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string LowerEnglishAlphabet = "abcdefghijklmnopqrstuvwxyz";
        public const string UpperRussianAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
        public const string LowerRussianAlphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
        public const string Numbers = "1234567890";
        public const string PunctuationMarks = "`~!@\"\'$#%^:;&?*().,/\\[]{} ";
        private readonly ILogger logger;
        private readonly float scaleSizeFactor;
        FT_LibraryRec_* libraryRec;

        public FontAtlasBuilder(ILogger logger, float scaleSizeFactor = 1.02f)
        {
            fixed (FT_LibraryRec_** pointer = &libraryRec)
            {
                var error = FT.FT_Init_FreeType(pointer);
                logger.LogDebug("FontAtlasBuilder создан с результатом {error}", error);
            }

            this.logger = logger;
            this.scaleSizeFactor = scaleSizeFactor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fontData"></param>
        /// <param name="size"></param>
        /// <param name="set">Строка со всеми символами в атласе, если null - используется встроенный</param>
        /// <returns></returns>
        public FontAtlas BuildAtlas(byte[] fontData, float size, string? set = null)
        {
            var defaultSet = set;
            if (defaultSet == null)
                defaultSet = UpperEnglishAlphabet + LowerEnglishAlphabet + UpperRussianAlphabet + LowerRussianAlphabet + Numbers + PunctuationMarks;

            FT_FaceRec_* faceRec = default;

            var bitmaps = new Dictionary<char, byte[,]>();

            fixed (byte* pointer = &fontData[0])
            {
                var error = FT.FT_New_Memory_Face(libraryRec, pointer, fontData.Length, 0, &faceRec);
                error = FT.FT_Set_Char_Size(faceRec, (int)(size * 64), (int)(size * 64), 96, 96);

                foreach (var chr in defaultSet)
                {
                    var index = FT.FT_Get_Char_Index(faceRec, chr);
                    error = FT.FT_Load_Glyph(faceRec, index, FT_LOAD.FT_LOAD_DEFAULT);
                    error = FT.FT_Render_Glyph(faceRec->glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);

                    var bitmap = new byte[faceRec->glyph->bitmap.width, faceRec->glyph->bitmap.rows];

                    for (var i = 0; i != faceRec->glyph->bitmap.width; i++)
                    {
                        for (var j = 0; j != faceRec->glyph->bitmap.rows; j++)
                        {
                            bitmap[i, j] = faceRec->glyph->bitmap.buffer[j * faceRec->glyph->bitmap.pitch + i];
                        }
                    }

                    bitmaps[chr] = bitmap;
                }
            }

            FT.FT_Done_Face(faceRec);

            var totalArea = bitmaps.Values.Select(t => t.GetLength(0) * t.GetLength(1)).Aggregate((a, b) => a + b);
            var totalSize = MathF.Sqrt(totalArea);
            while (true)
            {
                if (TryBuildAtlas((int)totalSize, bitmaps, out var fontAtlas))
                {
                    var bytes = new byte[(int)totalSize, (int)totalSize];

                    foreach (var chr in fontAtlas.Positions.Keys)
                    {
                        var data = fontAtlas.Data[chr];
                        var pos = fontAtlas.Positions[chr];

                        if (pos.Rotate)
                        {
                            for (var i = 0; i != data.GetLength(1); i++)
                            {
                                for (var j = 0; j != data.GetLength(0); j++)
                                {
                                    bytes[pos.X + i, pos.Y + j] = data[j, i];
                                }
                            }
                        }
                        else
                        {
                            for (var i = 0; i != data.GetLength(0); i++)
                            {
                                for (var j = 0; j != data.GetLength(1); j++)
                                {
                                    bytes[pos.X + i, pos.Y + j] = data[i, j];
                                }
                            }
                        }
                    }

                    fontAtlas.Atlas = bytes;

                    return fontAtlas;
                }
                else
                {
                    logger.LogDebug("Retry with size: {size}", totalSize);
                    totalSize *= scaleSizeFactor;
                }
            }
        }

        private bool TryBuildAtlas(int size, Dictionary<char, byte[,]> bitmaps, out FontAtlas fontAtlas)
        {
            fontAtlas = default;
            var maxRectsPacking = new MaxRectsBinPack<int>(size, size, FreeRectChoiceHeuristic.RectBestAreaFit);
            var idx = 0;
            var results = new Dictionary<char, InsertResult>();
            foreach (var key in bitmaps.Keys)
            {
                var insertResult = maxRectsPacking.Insert(idx, bitmaps[key].GetLength(0), bitmaps[key].GetLength(1));
                if (insertResult == null)
                    return false;
                results.Add(key, insertResult);
                idx++;
            }

            fontAtlas = new FontAtlas(bitmaps, results);
            return true;
        }

        public void Dispose()
        {
            var error = FT.FT_Done_FreeType(libraryRec);
            logger.LogDebug("Разрушение FontAtlasBuilder с результатом {error}", error);
        }
    }
}
