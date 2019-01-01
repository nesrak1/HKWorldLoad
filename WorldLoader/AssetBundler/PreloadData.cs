using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BundleLoader
{
    public static class PreloadData
    {
        public static AssetsReplacer CreatePreloadData(ulong pathId)
        {
            byte[] metaAsset = null;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(ms))
            {
                writer.bigEndian = false;

                writer.Write(0);
                writer.Align();

                //writer.Write(gameObjects.Count);
                //
                //foreach (AssetID gameObject in gameObjects)
                //{
                //    writer.Write(gameObject.fileId);
                //    writer.Write(gameObject.pathId);
                //}
                writer.Write(1);

                writer.Write(1);
                writer.Write((long)10001);
                writer.Align();

                writer.Write(0);
                writer.Align();

                //writer.Write(0);

                metaAsset = ms.ToArray();
            }
            return new AssetsReplacerFromMemory(0, pathId, 0x96, 0xFFFF, metaAsset);
        }
    }
}