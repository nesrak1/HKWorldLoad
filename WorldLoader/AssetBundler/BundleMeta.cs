using AssetsTools.NET;
using BundleLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BundleLoader
{
    public static class BundleMeta
    {
        public static AssetsReplacer CreateBundleInformation(string sceneName, ulong pathId)
        {
            byte[] metaAsset = null;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(ms))
            {
                writer.bigEndian = false;
                writer.WriteCountStringInt32(sceneName + ".unity3d");
                writer.Align();

                writer.Write(0);
                writer.Align();

                writer.Write(1);
                writer.WriteCountStringInt32(sceneName + ".unity");
                writer.Align();

                writer.Write(0);
                writer.Write(0);

                writer.Write(0);
                writer.Write((long)0);

                writer.Align();

                writer.Write(0);
                writer.Write(0);

                writer.Write(0);
                writer.Write((long)0);

                writer.Write((uint)1);

                writer.Write(0);
                writer.Align();

                writer.Write(0);
                writer.Align();

                writer.Write((uint)1);

                writer.Write(0);

                writer.Write(3);

                writer.Write(0);

                metaAsset = ms.ToArray();
            }
            return new AssetsReplacerFromMemory(0, pathId, 0x8E, 0xFFFF, metaAsset);
            //byte[] metaAsset = null;
            //using (MemoryStream ms = new MemoryStream())
            //using (AssetsFileWriter writer = new AssetsFileWriter(ms))
            //{
            //    writer.bigEndian = false;
            //    writer.Write(0);
            //    writer.Write((uint)(files.Count + 1));
            //
            //    writer.Write(0);
            //    writer.Write((ulong)1);
            //    foreach (KeyValuePair<AssetID, ulong> file in files)
            //    {
            //        writer.Write(0);
            //        writer.Write(file.Value);
            //    }
            //    writer.Align();
            //
            //    int index = 1;
            //    writer.Write((uint)files.Count);
            //    foreach (KeyValuePair<AssetID, ulong> file in files)
            //    {
            //        //writer.WriteCountStringInt32("");
            //        writer.WriteCountStringInt32(file.Key.fileId + "/" + file.Key.pathId + ".dat");
            //        writer.Align();
            //        writer.Write(index++);
            //        writer.Write(1);
            //        writer.Write(0);
            //        writer.Write(file.Value);
            //    }
            //    writer.Align();
            //
            //    writer.Write(0);
            //    writer.Write(1);
            //    writer.Write(0);
            //    writer.Write((ulong)2);
            //
            //    writer.Write((uint)1);
            //
            //    writer.Write(0);
            //
            //    writer.Write(0);
            //
            //    writer.Write((uint)0);
            //
            //    writer.Write((uint)0);
            //
            //    writer.Write(7);
            //
            //    writer.Write(0);
            //
            //    metaAsset = ms.ToArray();
            //}
            //return new AssetsReplacerFromMemory(0, pathId, 0x8E, 0xFFFF, metaAsset);
        }
    }
}
