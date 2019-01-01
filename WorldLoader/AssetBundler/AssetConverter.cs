using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BundleLoader
{
    public static class AssetConverter
    {
        public static AssetsReplacer ConvertAsset(AssetFileInfoEx info, AssetsFile file, BinaryReader r, ulong pathId)
        {
            ushort monoType = file.typeTree.pTypes_Unity5[info.curFileTypeOrIndex].scriptIndex;
            r.BaseStream.Position = (long)info.absoluteFilePos;
            byte[] data = r.ReadBytes((int)info.curFileSize);
            return new AssetsReplacerFromMemory(0, pathId, 0x4, monoType, data);
        }
    }
}
