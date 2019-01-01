using AssetsTools.NET;
using BundleLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WorldLoader
{
    public class MoveTest
    {
        public static AssetsReplacer RunMoveTest(AssetFileInfoEx info, AssetTypeValueField baseField, ulong pathId)
        {
            //GameObject 609->Transform 2642
            AssetTypeValueField m_LocalPosition = baseField.Get("m_LocalPosition");
            
            m_LocalPosition.Get("x").GetValue().Set(89.93f);
            m_LocalPosition.Get("y").GetValue().Set(25.56f);
            m_LocalPosition.Get("z").GetValue().Set(48.73f);

            byte[] moveAsset;
            using (MemoryStream memStream = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(memStream))
            {
                writer.bigEndian = false;
                baseField.Write(writer);
                moveAsset = memStream.ToArray();
            }

            return new AssetsReplacerFromMemory(0, pathId, 0x04, 0xFFFF, moveAsset);
        }
    }
}
