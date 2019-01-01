using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BundleLoader
{
    public static class BundleCreator
    {
        public static byte[] CreateBlankAssets(string engineVersion, List<Type_0D> types)
        {
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(ms))
            {
                AssetsFileHeader header = new AssetsFileHeader()
                {
                    metadataSize = 0,
                    fileSize = 0x1000,
                    format = 0x11,
                    offs_firstFile = 0x1000,
                    endianness = 0,
                    unknown = new byte[] { 0, 0, 0 }
                };
                TypeTree typeTree = new TypeTree()
                {
                    unityVersion = engineVersion,
                    version = 0x5,
                    hasTypeTree = true,
                    fieldCount = (uint)types.Count(),
                    pTypes_Unity5 = types.ToArray()
                };


                header.Write(writer.Position, writer);
                writer.bigEndian = false;
                typeTree.Write(writer.Position, writer, 0x11);
                writer.Write((uint)0);
                writer.Align();
                //preload table and dependencies
                writer.Write((uint)0);
                writer.Write((uint)0);

                //due to a write bug in at.net we have to pad to 0x1000
                while (ms.Position < 0x1000)
                {
                    writer.Write((byte)0);
                }

                return ms.ToArray();
            }
        }
        public static AssetsBundleFile CreateBlankBundle(string engineVersion, int contentSize, int contentSizeSA, string sceneName)
        {
            uint infSize = (uint)(0x7B + sceneName.Length * 2);
            AssetsBundleHeader06 header = new AssetsBundleHeader06()
            {
                signature = "UnityFS",
                fileVersion = 6,
                minPlayerVersion = "5.x.x",
                fileEngineVersion = engineVersion,
                totalFileSize = (ulong)(0x82 + engineVersion.Length + contentSize + contentSizeSA),
                compressedSize = infSize,//0x5B,
                decompressedSize = infSize,//0x5B,
                flags = 0x40
            };
            AssetsBundleBlockInfo06 blockInfSharedAssets = new AssetsBundleBlockInfo06
            {
                decompressedSize = (uint)contentSizeSA,
                compressedSize = (uint)contentSizeSA,
                flags = 0x0040
            };
            AssetsBundleDirectoryInfo06 dirInfSharedAssets = new AssetsBundleDirectoryInfo06
            {
                offset = 0,
                decompressedSize = (uint)contentSizeSA,
                flags = 4,
                name = "BuildPlayer-" + sceneName + ".sharedAssets"
            };
            AssetsBundleBlockInfo06 blockInf = new AssetsBundleBlockInfo06
            {
                decompressedSize = (uint)contentSize,
                compressedSize = (uint)contentSize,
                flags = 0x0040
            };
            AssetsBundleDirectoryInfo06 dirInf = new AssetsBundleDirectoryInfo06
            {
                offset = (ulong)contentSizeSA,
                decompressedSize = (uint)contentSize,
                flags = 4,
                name = "BuildPlayer-" + sceneName
            };
            AssetsBundleBlockAndDirectoryList06 info = new AssetsBundleBlockAndDirectoryList06()
            {
                checksumLow = 0,
                checksumHigh = 0,
                blockCount = 2,
                blockInf = new AssetsBundleBlockInfo06[]
                {
                    blockInfSharedAssets,
                    blockInf
                },
                directoryCount = 2,
                dirInf = new AssetsBundleDirectoryInfo06[]
                {
                    dirInfSharedAssets,
                    dirInf
                }
            };
            AssetsBundleFile bundle = new AssetsBundleFile()
            {
                bundleHeader6 = header,
                bundleInf6 = info
            };
            return bundle;
        }

        private static string GenerateCabName()
        {
            string alphaNum = "0123456789abcdef";
            string output = "CAB-";
            Random rand = new Random();
            for (int i = 0; i < 32; i++)
            {
                output += alphaNum[rand.Next(0, alphaNum.Length)];
            }
            return output;
        }
    }
}
