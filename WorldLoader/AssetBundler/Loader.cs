using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorldLoader;

namespace BundleLoader
{
    public class Loader
    {
        public static byte[] CreateBundleFromLevel(AssetsManager am, /*byte[] data,*/ AssetsFileInstance inst, string sceneName, DiffFile diffFile)
        {
            AssetsFile file = inst.file;
            AssetsFileTable table = inst.table;

            string folderName = Path.GetDirectoryName(inst.path);
            //string sceneName = Path.GetFileNameWithoutExtension(inst.path) + "_mod";
            
            List<AssetsReplacer> assetsSA = new List<AssetsReplacer>();

            List<AssetFileInfoEx> infos = table.pAssetFileInfo.ToList();
            List<int> typeIds = new List<int>();
            foreach (AssetFileInfoEx info in infos)
            {
                int typeId = (int)info.curFileType;
                if (!typeIds.Contains(typeId) && typeId != 0x72)
                    typeIds.Add(typeId);
            }

            assetsSA.Add(PreloadData.CreatePreloadData(1));
            assetsSA.Add(BundleMeta.CreateBundleInformation(sceneName, 2));

            //todo: pull from original assets file, cldb is not always update to date
            List<Type_0D> types = new List<Type_0D>();
            foreach (int typeId in typeIds)
            {
                types.Add(FixTypeTree(C2T5.Cldb2TypeTree(am.classFile, typeId)));
            }

            List<Type_0D> typesSA = new List<Type_0D>
            {
                FixTypeTree(C2T5.Cldb2TypeTree(am.classFile, 0x96)), //PreloadData
                FixTypeTree(C2T5.Cldb2TypeTree(am.classFile, 0x8E))  //AssetBundle
            };

            const string ver = "2017.4.10f1";

            List<AssetsReplacer> changedAssets = new List<AssetsReplacer>();
            UnityEngine.Debug.Log("HKWE DM " + diffFile.magic);
            UnityEngine.Debug.Log("HKWE GC " + diffFile.changes.Count + diffFile.adds.Count + diffFile.removes.Count);
            //AssetsReplacerFromMemory mem = MoveTest.RunMoveTest(table.getAssetInfo(2642), am.GetATI(file, table.getAssetInfo(2642)).GetBaseField(), 2642) as AssetsReplacerFromMemory;
            foreach (GameObjectChange goChange in diffFile.changes)
            {
                UnityEngine.Debug.Log("HKWE GO " + goChange.pathId);
                foreach (ComponentChangeOrAdd compChange in goChange.changes)
                {
                    AssetFileInfoEx goInfo = table.getAssetInfo((ulong)goChange.pathId);
                    AssetTypeValueField goBaseField = am.GetATI(file, goInfo).GetBaseField();

                    AssetTypeValueField compPptr = goBaseField.Get("m_Component").Get("Array")[(uint)compChange.componentIndex].Get("component");
                    AssetsManager.AssetExternal compExt = am.GetExtAsset(inst, compPptr);

                    AssetFileInfoEx compInfo = compExt.info;
                    AssetTypeValueField compBaseField = compExt.instance.GetBaseField();

                    UnityEngine.Debug.Log("HKWE LR " + compInfo.index);
                    AssetsReplacer imAlreadyReplacer = ComponentDiffReplacer.DiffComponent(compInfo, compBaseField, am.classFile, compChange, compInfo.index);
                    changedAssets.Add(imAlreadyReplacer);
                }
            }

            byte[] data = null;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(ms))
            {
                file.WriteFix(writer, 0, changedAssets.ToArray(), 0);
                data = ms.ToArray();
            }

            byte[] blankDataSA = BundleCreator.CreateBlankAssets(ver, typesSA);
            AssetsFile blankFileSA = new AssetsFile(new AssetsFileReader(new MemoryStream(blankDataSA)));

            byte[] dataSA = null;
            using (MemoryStream ms = new MemoryStream())
            using (AssetsFileWriter writer = new AssetsFileWriter(ms))
            {
                blankFileSA.WriteFix(writer, 0, assetsSA.ToArray(), 0);
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
