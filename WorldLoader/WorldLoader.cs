using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorldLoader
{
    public class WorldLoader : Mod
    {
        public static Dictionary<string, DiffFile> diffDatabase;
        public override void Initialize()
        {
            //UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;

            if (!Directory.Exists("HKWEDiffs"))
                Directory.CreateDirectory("HKWEDiffs");

            SceneManager.LoadSceneNames();

            if (diffDatabase == null)
            {
                diffDatabase = new Dictionary<string, DiffFile>();
            }

            foreach (string diffFile in Directory.GetFiles("HKWEDiffs"))
            {
                if (File.Exists(diffFile))
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(diffFile);
                    TextAsset diffAsset = bundle.LoadAsset<TextAsset>("HKWEDiffData");
                    if (diffAsset == null)
                    {
                        Debug.Log("HKWE NA");
                        continue;
                    }
                    byte[] diffData = diffAsset.bytes;

                    using (MemoryStream ms = new MemoryStream(diffData))
                    using (BinaryReader r = new BinaryReader(ms))
                    {
                        DiffFile file = new DiffFile();
                        file.Read(r);
                        
                        string levelName = Path.GetFileName(diffFile);
                        string realName = levelName;
                        if (realName.StartsWith("level"))
                        {
                            realName = realName.Substring(5);
                        }

                        int levelIndex = -1;
                        if (int.TryParse(realName, out levelIndex))
                        {
                            Debug.Log("HKWE LI " + levelIndex);
                            realName = SceneManager.GetSceneName(int.Parse(realName));
                            diffDatabase[realName] = file;
                            Debug.Log("loaded scene " + realName);
                        }
                        else
                        {
                            Debug.Log("Couldn't load index " + realName);
                        }
                    }
                }
            }
            PatchSceneLoad psl = new PatchSceneLoad();
            psl.Patch();
        }

        private void SceneChanged(Scene from, Scene to)
        {
            //if (!Directory.Exists("HKWEDiffs"))
            //    Directory.CreateDirectory("HKWEDiffs");
            //
            //string targetFile = Path.Combine("HKWEDiffs", "level" + to.buildIndex + ".dif");
            //if (File.Exists(targetFile))
            //{
            //    AssetBundle bundle = AssetBundle.LoadFromFile(targetFile);
            //    TextAsset diffAsset = bundle.LoadAsset<TextAsset>("HKWEDiffData");
            //    byte[] diffData = diffAsset.bytes;
            //
            //    using (MemoryStream ms = new MemoryStream(diffData))
            //    using (BinaryReader r = new BinaryReader(ms))
            //    {
            //        DiffFile file = new DiffFile();
            //        file.Read(r);
            //    }
            //
            //    //string path = "deep_fg_3/bone_deep_0113_g";
            //    //GameObject gObj = bundle.LoadAllAssets()[bundle.GetAllAssetNames().ToList().IndexOf(path)] as GameObject;
            //}
        }

        private void Achievement_ctor(On.Achievement.orig_ctor orig, Achievement self)
        {
            throw new NotImplementedException();
        }

        public override string GetVersion()
        {
            return "0.1.0";
        }
    }
}
