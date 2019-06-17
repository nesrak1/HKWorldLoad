using AssetsTools.NET;
using AssetsTools.NET.Extra;
using BundleLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WorldLoader
{
    public class ReferenceCrawler
    {
        //this is going to be a slow operation, so it's probably best to move
        //this to a post process action after unity has built the bundle
        public static void ReorderIds(AssetsManager am, AssetsFileInstance inst, ulong idOffset)
        {
            List<ulong> oldIds = new List<ulong>();
            List<ulong> tempIds = new List<ulong>();
            Dictionary<ulong, ulong> oldToTemp = new Dictionary<ulong, ulong>();
            Dictionary<ulong, ulong> tempToNew = new Dictionary<ulong, ulong>();
            Dictionary<ulong, byte[]> tempData = new Dictionary<ulong, byte[]>();
            Dictionary<ulong, byte[]> newData = new Dictionary<ulong, byte[]>();

            //move 1:
            //move everything to the end of where it will go to
            //prevent from hitting collisions with their own ids
            foreach (AssetFileInfoEx inf in inst.table.pAssetFileInfo)
            {
                oldIds.Add(inf.index);
            }
            ulong nextId = idOffset + inst.table.assetFileInfoCount;
            foreach (AssetFileInfoEx inf in inst.table.pAssetFileInfo)
            {
                if (InClassBlacklist(inf.curFileType))
                    continue;
                ulong repId = nextId++;
                while (oldIds.Contains(repId))
                    repId = nextId++;
                tempIds.Add(repId);
                oldToTemp[inf.index] = repId;
            }
            foreach (AssetFileInfoEx inf in inst.table.pAssetFileInfo)
            {
                if (InClassBlacklist(inf.curFileType))
                    continue;
                AssetTypeValueField baseField = am.GetATI(inst.file, inf).GetBaseField();
                
                List<ulong> depIds = new List<ulong>();
                depIds.Add(inf.index);
                
                RecurseTypeReplace(am, inst, baseField, depIds, 0, oldToTemp);

                using (MemoryStream ms = new MemoryStream())
                using (AssetsFileWriter w = new AssetsFileWriter(ms))
                {
                    w.bigEndian = false;
                    baseField.Write(w);
                    tempData[inf.index] = ms.ToArray();
                }
            }
            //update everything to a new assets file with the first move
            List<AssetsReplacer> reps = new List<AssetsReplacer>();
            foreach (AssetFileInfoEx inf in inst.table.pAssetFileInfo)
            {
                reps.Add(new AssetsRemover(0, inf.index, (int)inf.curFileType));
                if (InClassBlacklist(inf.curFileType))
                    continue;
                reps.Add(new AssetsReplacerFromMemory(0, oldToTemp[inf.index], (int)inf.curFileType, 0xFFFF, tempData[inf.index]));
            }
            byte[] firstMoveBytes;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter w = new AssetsFileWriter(ms))
            {
                w.bigEndian = false;
                inst.file.Write(w, 0, reps.ToArray(), 0);
                firstMoveBytes = ms.ToArray();
            }
            //File.WriteAllBytes("_bundletest1.unity3d", firstMoveBytes);
            //load our modifications (btw super hacky and slow)
            inst.stream = new MemoryStream(firstMoveBytes);
            inst.file = new AssetsFile(new AssetsFileReader(inst.stream));
            inst.table = new AssetsFileTable(inst.file);

            //move 2:
            //move the assets right after the existing ones
            nextId = idOffset;
            foreach (AssetFileInfoEx inf in inst.table.pAssetFileInfo)
            {
                if (InClassBlacklist(inf.curFileType))
                    continue;
                ulong repId = nextId++;
                
                tempToNew[inf.index] = repId;
            }
            foreach (AssetFileInfoEx inf in inst.table.pAssetFileInfo)
            {
                AssetTypeValueField baseField = am.GetATI(inst.file, inf).GetBaseField();
                
                List<ulong> depIds = new List<ulong>();
                depIds.Add(inf.index);

                RecurseTypeReplace(am, inst, baseField, depIds, 0, tempToNew);

                using (MemoryStream ms = new MemoryStream())
                using (AssetsFileWriter w = new AssetsFileWriter(ms))
                {
                    w.bigEndian = false;
                    baseField.Write(w);
                    newData[inf.index] = ms.ToArray();
                }
            }
            reps.Clear();
            foreach (AssetFileInfoEx inf in inst.table.pAssetFileInfo)
            {
                reps.Add(new AssetsRemover(0, inf.index, (int)inf.curFileType));
                reps.Add(new AssetsReplacerFromMemory(0, tempToNew[inf.index], (int)inf.curFileType, 0xFFFF, newData[inf.index]));
            }
            byte[] secondMoveBytes;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter w = new AssetsFileWriter(ms))
            {
                w.bigEndian = false;
                inst.file.Write(w, 0, reps.ToArray(), 0);
                secondMoveBytes = ms.ToArray();
            }
            //File.WriteAllBytes("_bundletest2.unity3d", secondMoveBytes);
            inst.stream = new MemoryStream(secondMoveBytes);
            inst.file = new AssetsFile(new AssetsFileReader(inst.stream));
            inst.table = new AssetsFileTable(inst.file);
            Debug.Log("done");
        }

        public static List<ulong> CrawlPPtrs(AssetsManager am, AssetsFileInstance inst, ulong startingId)
        {
            List<ulong> depIds = new List<ulong>();
            depIds.Add(startingId);
            return CrawlPPtrs(am, inst, startingId, depIds);
        }
        private static List<ulong> CrawlPPtrs(AssetsManager am, AssetsFileInstance inst, ulong startingId, List<ulong> depIds)
        {
            AssetFileInfoEx info = inst.table.getAssetInfo(startingId);
            AssetTypeValueField baseField = am.GetATI(inst.file, info).GetBaseField();
            RecurseType(am, inst, baseField, depIds, 0);
            return depIds;
        }
        //we probably don't need this method anymore
        private static void RecurseType(AssetsManager am, AssetsFileInstance inst, AssetTypeValueField field, List<ulong> ids, int depth)
        {
            string p = new string(' ', depth);
            foreach (AssetTypeValueField child in field.pChildren)
            {
                //UnityEngine.Debug.Log(p + child.templateField.type + " " + child.templateField.name);
                if (!child.templateField.hasValue)
                {
                    if (child == null)
                        return;
                    string typeName = child.templateField.type;
                    if (typeName.StartsWith("PPtr<") && typeName.EndsWith(">") && child.childrenCount == 2)
                    {
                        int fileId = child.Get("m_FileID").GetValue().AsInt();
                        ulong pathId = (ulong)child.Get("m_PathID").GetValue().AsInt64();
                        //UnityEngine.Debug.Log(p + "found pptr of " + fileId + "," + pathId);
                        if (fileId == 0 && pathId != 0 && !ids.Contains(pathId)) //it's possible we could load some outside dependency, but I'd rather that not happen
                        {
                            ids.Add(pathId);
                            AssetTypeValueField depBaseField = am.GetExtAsset(inst, child).instance.GetBaseField();
                            RecurseType(am, inst, depBaseField, ids, 0);
                        }
                    }
                    RecurseType(am, inst, child, ids, depth + 1);
                }
            }
        }
        private static void RecurseTypeReplace(AssetsManager am, AssetsFileInstance inst, AssetTypeValueField field, List<ulong> ids, int depth, Dictionary<ulong, ulong> findAndReplace)
        {
            string p = new string(' ', depth);
            foreach (AssetTypeValueField child in field.pChildren)
            {
                if (!child.templateField.hasValue)
                {
                    if (child == null)
                        return;
                    string typeName = child.templateField.type;
                    if (typeName.StartsWith("PPtr<") && typeName.EndsWith(">") && child.childrenCount == 2)
                    {
                        int fileId = child.Get("m_FileID").GetValue().AsInt();
                        ulong pathId = (ulong)child.Get("m_PathID").GetValue().AsInt64();
                        if (fileId == 0 && pathId != 0)
                        {
                            if (!ids.Contains(pathId))
                            {
                                ids.Add(pathId);
                                //AssetTypeValueField depBaseField = am.GetExtAsset(inst, child).instance.GetBaseField();
                                //RecurseTypeReplace(am, inst, depBaseField, ids, 0, find, replace);
                            }
                            //if (pathId == find)
                            //{
                            //    Debug.Log("matched, setting pathid to " + replace + " from " + child.Get("m_PathID").GetValue().AsInt64());
                            //    child.Get("m_PathID").value.Set((long)replace);
                            //}
                            foreach (KeyValuePair<ulong, ulong> far in findAndReplace)
                            {
                                //Debug.Log(pathId + " == " + far.Key);
                                if (pathId == far.Key)
                                {
                                    child.Get("m_PathID").value.Set((long)far.Value);
                                }
                            }
                        }
                    }
                    RecurseTypeReplace(am, inst, child, ids, depth + 1, findAndReplace);
                }
            }
        }
        private static bool InClassBlacklist(uint classId)
        {
            //add any classids here that are only part of a bundle (prefab, assetbundle, etc)
            return classId == 0x8E; //AssetBundle
        }
    }
}
