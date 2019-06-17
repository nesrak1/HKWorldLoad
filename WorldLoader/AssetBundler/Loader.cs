using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WorldLoader;

namespace BundleLoader
{
    public class Loader
    {
        public static byte[] CreateBundleFromLevel(AssetsManager am, /*byte[] data,*/ AssetsFileInstance inst, string sceneName, DiffFile diffFile, string bunPath)
        {
            AssetsFile file = inst.file;
            AssetsFileTable table = inst.table;

            string folderName = Path.GetDirectoryName(inst.path);
            //string sceneName = Path.GetFileNameWithoutExtension(inst.path) + "_mod";
            
            List<AssetsReplacer> assetsSA = new List<AssetsReplacer>();
            
            List<AssetFileInfoEx> infos = table.pAssetFileInfo.ToList();
            //List<int> typeIds = new List<int>();
            //foreach (AssetFileInfoEx info in infos)
            //{
            //    int typeId = (int)info.curFileType;
            //    if (!typeIds.Contains(typeId) && typeId != 0x72)
            //        typeIds.Add(typeId);
            //}
            
            assetsSA.Add(PreloadData.CreatePreloadData(1));
            assetsSA.Add(BundleMeta.CreateBundleInformation(sceneName, 2));
            
            //todo: pull from original assets file, cldb is not always update to date
            List<Type_0D> types = new List<Type_0D>();
            //foreach (int typeId in typeIds)
            //{
            //    types.Add(C2T5.Cldb2TypeTree(am.classFile, typeId));
            //}
            
            List<Type_0D> typesSA = new List<Type_0D>
            {
                C2T5.Cldb2TypeTree(am.classFile, 0x96), //PreloadData
                C2T5.Cldb2TypeTree(am.classFile, 0x8E)  //AssetBundle
            };

            const string ver = "2017.4.10f1";

            List<AssetsReplacer> replacers = new List<AssetsReplacer>();
            //UnityEngine.Debug.Log("HKWE DM " + diffFile.magic);
            //UnityEngine.Debug.Log("HKWE GC " + diffFile.changes.Count + diffFile.adds.Count + diffFile.removes.Count);
            //AssetsReplacerFromMemory mem = MoveTest.RunMoveTest(table.getAssetInfo(2642), am.GetATI(file, table.getAssetInfo(2642)).GetBaseField(), 2642) as AssetsReplacerFromMemory;
            foreach (GameObjectChange goChange in diffFile.changes)
            {
                //UnityEngine.Debug.Log("HKWE GO " + goChange.pathId);
                foreach (ComponentChangeOrAdd compChange in goChange.changes)
                {
                    AssetFileInfoEx goInfo = table.getAssetInfo((ulong)goChange.pathId);
                    AssetTypeValueField goBaseField = am.GetATI(file, goInfo).GetBaseField();

                    AssetTypeValueField compPptr = goBaseField.Get("m_Component").Get("Array")[(uint)compChange.componentIndex].Get("component");
                    AssetsManager.AssetExternal compExt = am.GetExtAsset(inst, compPptr);

                    AssetFileInfoEx compInfo = compExt.info;
                    AssetTypeValueField compBaseField = compExt.instance.GetBaseField();

                    //UnityEngine.Debug.Log("HKWE LR " + compInfo.index);
                    AssetsReplacer imAlreadyReplacer = ComponentDiffReplacer.DiffComponent(compInfo, compBaseField, am.classFile, compChange, compInfo.index);
                    replacers.Add(imAlreadyReplacer);
                }
            }
            AssetsManager amBun = new AssetsManager(); //we create a new manager because the two filenames will probably conflict
            amBun.classFile = am.classFile; //we can just reuse the classfile which is kinda hacky
            AssetsFileInstance bunInst = amBun.LoadAssetsFile(new MemoryStream(GetBundleData(bunPath, 0)), "HKWEDiffs", false); //placeholder path since we have no deps
            
            //rearrange the pathids immediately after the
            //last one from the level to keep unity happy
            ulong levelLargestPathID = 0;
            foreach (AssetFileInfoEx inf in table.pAssetFileInfo)
            {
                if (inf.index > levelLargestPathID)
                {
                    levelLargestPathID = inf.index;
                }
            }
            ReferenceCrawler.ReorderIds(amBun, bunInst, levelLargestPathID + 1);

            byte[] bunSAInst = GetBundleData(bunPath, 1);
            //HashSet<ulong> addedDeps = new HashSet<ulong>();
            foreach (AssetFileInfoEx inf in bunInst.table.pAssetFileInfo)
            {
                replacers.Add(MakeReplacer(inf.index, am, bunInst, inst, inf, bunSAInst, types));
            }
            //foreach (GameObjectInfo inf in diffFile.infos)
            //{
            //    Debug.Log("7");
            //    ulong bunPathId = GetBundlePathId(amBun, bunInst, inf);
            //    
            //    AssetFileInfoEx objInf = bunInst.table.getAssetInfo(bunPathId);
            //    replacers.Add(MakeReplacer(bunPathId, am, bunInst, inst, objInf, bunSAInst, types));
            //
            //    List<ulong> deps = ReferenceCrawler.CrawlPPtrs(amBun, bunInst, bunPathId);
            //    foreach (ulong dep in deps)
            //    {
            //        if (!addedDeps.Contains(dep))
            //        {
            //            addedDeps.Add(dep);
            //            AssetFileInfoEx depInf = bunInst.table.getAssetInfo(dep);
            //            //if (depInf.curFileType == 0x01 || depInf.curFileType == 0x04 || depInf.curFileType == 0xD4 || depInf.curFileType == 0x15 || depInf.curFileType == 0xD5)
            //            //{
            //            //    continue;
            //            //}
            //            replacers.Add(MakeReplacer(dep, am, bunInst, inst, depInf, bunSAInst, types));
            //        }
            //    }
            //    ////its possible to get a collision but very unlikely since unity already randomizes ids which are 8 bytes long
            //    ////there's nothing here to test if a collision would be created so just hope that you don't win the lottery
            //    //ulong bunPathId = GetBundlePathId(amBun, bunInst, inf);
            //    ////AssetFileInfoEx afInf = bunInst.table.getAssetInfo(bunPathId);
            //    ////replacers.Add(MakeReplacer(bunPathId, afInf, bunInst.stream));
            //    //List<ulong> deps = ReferenceCrawler.CrawlPPtrs(am, bunInst, bunPathId);
            //    ////if (info.curFileType == 0x01 || info.curFileType == 0x04 || info.curFileType == 0xD4)
            //    ////{
            //    ////    continue;
            //    ////}
            //    //foreach (ulong dep in deps)
            //    //{
            //    //    AssetFileInfoEx depInf = bunInst.table.getAssetInfo(dep);
            //    //    //MakeReplacer(dep, am, bunInst, inst, depInf, bunSAInst, types);
            //    //    AssetsReplacerFromMemory ar = MakeReplacer(dep, am, bunInst, inst, depInf, bunSAInst, types);
            //    //    //todo- I guess this was just for testing purposes to block out everything, remove this at some point
            //    //    if (depInf.curFileType == 0x01 || depInf.curFileType == 0x04 || depInf.curFileType == 0xD4 || depInf.curFileType == 0x15 || depInf.curFileType == 0xD5) //depInf.curFileType == 0x1C
            //    //    {
            //    //        continue;
            //    //    }
            //    //    replacers.Add(ar);
            //    //}
            //}

            byte[] data = null;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(ms))
            {
                //file.typeTree.hasTypeTree = true; //so we don't have to calculate hashes
                //foreach (Type_0D type in file.typeTree.pTypes_Unity5)
                //{
                //    if (!types.Any(t => t.classId == type.classId))
                //    {
                //        types.Insert(0, C2T5.Cldb2TypeTree(am.classFile, type.classId));
                //    }
                //}
                file.typeTree.pTypes_Unity5 = file.typeTree.pTypes_Unity5.Concat(types.ToArray()).ToArray();
                //file.typeTree.pTypes_Unity5 = types.ToArray();
                file.typeTree.fieldCount = (uint)file.typeTree.pTypes_Unity5.Length;
                //file.typeTree.fieldCount = (uint)types.Count;
                file.Write(writer, 0, replacers.ToArray(), 0);
                data = ms.ToArray();
            }
            //File.WriteAllBytes("_bundlefinal1.unity3d", data);

            byte[] blankDataSA = BundleCreator.CreateBlankAssets(ver, typesSA);
            AssetsFile blankFileSA = new AssetsFile(new AssetsFileReader(new MemoryStream(blankDataSA)));

            byte[] dataSA = null;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(ms))
            {
                blankFileSA.Write(writer, 0, assetsSA.ToArray(), 0);
                dataSA = ms.ToArray();
            }

            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(ms))
            {
                AssetsBundleFile bundle = BundleCreator.CreateBlankBundle(ver, data.Length, dataSA.Length, sceneName);
                bundle.Write(writer);
                writer.Write(dataSA);
                writer.Write(data);
                return ms.ToArray();
            }
        }

        public static AssetTypeValueField GetBaseField(AssetsManager am, AssetsFile file, AssetFileInfoEx info)
        {
            AssetTypeInstance ati = am.GetATI(file, info);
            return ati.GetBaseField();
        }

        public static Type_0D FixTypeTree(Type_0D tt)
        {
            //if basefield isn't 3, unity won't load the bundle
            tt.pTypeFieldsEx[0].version = 3;
            return tt;
        }

        private static ulong GetBundlePathId(AssetsManager am, AssetsFileInstance inst, GameObjectInfo inf)
        {
            string assetInBundleName = "HKWEA_" + inf.name.Substring(0, Math.Min(inf.name.Length, 8)) + "_" + inf.fileId + "_" + inf.origPathId + "_" + inf.pathId;
            return FindGameObject(am, inst, assetInBundleName).index;
        }

        private static byte[] GetBundleData(string bunPath, int index)
        {
            AssetsFileReader r = new AssetsFileReader(File.Open(bunPath, FileMode.Open, FileAccess.Read, FileShare.Read));
            AssetsBundleFile bun = new AssetsBundleFile();
            bun.Read(r, true);

            //if the bundle doesn't have this section return empty
            if (index >= bun.bundleInf6.dirInf.Length)
                return new byte[0];

            AssetsBundleDirectoryInfo06 dirInf = bun.bundleInf6.dirInf[index];
            int start = (int)(bun.bundleHeader6.GetFileDataOffset() + dirInf.offset);
            int length = (int)dirInf.decompressedSize;
            byte[] data;
            r.BaseStream.Position = start;
            data = r.ReadBytes(length);
            return data;
        }

        private static AssetFileInfoEx FindGameObject(AssetsManager am, AssetsFileInstance inst, string name)
        {
            foreach (AssetFileInfoEx info in inst.table.pAssetFileInfo)
            {
                if (info.curFileType == 0x01)
                {
                    ClassDatabaseType type = AssetHelper.FindAssetClassByID(am.classFile, info.curFileType);
                    string infoName = AssetHelper.GetAssetNameFast(inst.file, am.classFile, info);
                    if (infoName == name)
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        private static AssetsReplacerFromMemory MakeReplacer(ulong pathId, AssetsManager am, AssetsFileInstance file, AssetsFileInstance srcFile, AssetFileInfoEx inf, byte[] saData, List<Type_0D> types)
        {
            byte[] data = new byte[inf.curFileSize];
            //UnityEngine.Debug.Log("making rep for " + inf.absoluteFilePos + " => " + (inf.absoluteFilePos+inf.curFileSize) + " of " + file.stream.Length);
            int typeId = file.file.typeTree.pTypes_Unity5[inf.curFileTypeOrIndex].classId;
            if (!types.Any(t => t.classId == typeId) && !srcFile.file.typeTree.pTypes_Unity5.Any(t => t.classId == typeId))
            {
                if (!Hashes.hashes.ContainsKey(typeId))
                {
                    throw new NotImplementedException("hash not in hashtable, please add it!");
                }
                types.Add(new Type_0D()
                {
                    classId = typeId,
                    unknown16_1 = 0,
                    scriptIndex = 0xFFFF,
                    unknown1 = 0,
                    unknown2 = 0,
                    unknown3 = 0,
                    unknown4 = 0,
                    unknown5 = Hashes.hashes[typeId][0],
                    unknown6 = Hashes.hashes[typeId][1],
                    unknown7 = Hashes.hashes[typeId][2],
                    unknown8 = Hashes.hashes[typeId][3]
                });
                //types.Add(C2T5.Cldb2TypeTree(am.classFile, typeId));
            }
            switch (typeId)
            {
                case 0x1C:
                    data = FixTexture2D(am.GetATI(file.file, inf).GetBaseField(), saData);
                    break;
                case 0x15:
                    data = FixMaterial(srcFile.file, am.GetATI(file.file, inf).GetBaseField(), saData);
                    break;
                default:
                    file.stream.Position = (int)inf.absoluteFilePos;
                    file.stream.Read(data, 0, (int)inf.curFileSize);
                    break;
            }
            return new AssetsReplacerFromMemory(0, pathId, typeId, 0xFFFF, data);
        }

        //todo- not guaranteed to get texture in sharedassets
        private static byte[] FixTexture2D(AssetTypeValueField baseField, byte[] saData)
        {
            AssetTypeValueField m_StreamData = baseField.Get("m_StreamData");
            int offset = (int)m_StreamData.Get("offset").GetValue().AsUInt();
            int size = (int)m_StreamData.Get("size").GetValue().AsUInt();

            byte[] data = new byte[0];
            if (size != 0)
            {
                string path = m_StreamData.Get("path").GetValue().AsString();
                using (MemoryStream inStream = new MemoryStream(saData))
                using (MemoryStream outStream = new MemoryStream())
                {
                    long fileSize = inStream.Length;
                    data = new byte[size];
                    inStream.Position = offset;
            
                    int bytesRead;
                    var buffer = new byte[2048];
                    while ((bytesRead = inStream.Read(buffer, 0, Math.Min(2048, (offset + size) - (int)inStream.Position))) > 0)
                    {
                        outStream.Write(buffer, 0, bytesRead);
                        if (inStream.Position >= offset + size)
                        {
                            break;
                        }
                    }
                    data = outStream.ToArray();
                }
            }
            m_StreamData.Get("offset").value.value.asUInt32 = 0;
            m_StreamData.Get("size").value.value.asUInt32 = 0;
            m_StreamData.Get("path").value.value.asString = "";
            baseField.Get("image data").GetValue().type = EnumValueTypes.ValueType_ByteArray;
            baseField.Get("image data").GetValue().Set(new AssetTypeByteArray() {
                data = data,
                size = (uint)data.Length
            });
            baseField.Get("image data").templateField.valueType = EnumValueTypes.ValueType_ByteArray;
            byte[] assetData;
            using (MemoryStream memStream = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(memStream))
            {
                writer.bigEndian = false;
                baseField.Write(writer);
                assetData = memStream.ToArray();
            }
            return assetData;
        }

        private static byte[] FixMaterial(AssetsFile file, AssetTypeValueField baseField, byte[] saData)
        {
            AssetTypeValueField m_Shader = baseField.Get("m_Shader");
            if (m_Shader.Get("m_FileID").GetValue().AsInt() == 1 && //only works for 2017.4.10f1
                m_Shader.Get("m_PathID").GetValue().AsInt64() == 10753)
            {
                int ggmaIdx = Array.FindIndex(file.dependencies.pDependencies, d => Path.GetFileName(d.assetPath) == "globalgamemanagers.assets");
                if (ggmaIdx != -1)
                {
                    //Sprites-Default
                    m_Shader.Get("m_FileID").GetValue().Set(ggmaIdx + 1);
                    m_Shader.Get("m_PathID").GetValue().Set(4); //only works for this specific version of hk
                }
                else
                {
                    throw new NotImplementedException("no ggm.assets reference");
                }
            }

            byte[] assetData;
            using (MemoryStream memStream = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(memStream))
            {
                writer.bigEndian = false;
                baseField.Write(writer);
                assetData = memStream.ToArray();
            }
            return assetData;
        }
    }

    public class AssetID
    {
        public int fileId;
        public long pathId;
        public AssetID(int fileId, long pathId)
        {
            this.fileId = fileId;
            this.pathId = pathId;
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(AssetID))
                return false;
            AssetID assetID = obj as AssetID;
            if (fileId == assetID.fileId &&
                pathId == assetID.pathId)
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            int hash = 17;
            
            hash = hash * 23 + fileId.GetHashCode();
            hash = hash * 23 + pathId.GetHashCode();
            return hash;
        }
    }
}
