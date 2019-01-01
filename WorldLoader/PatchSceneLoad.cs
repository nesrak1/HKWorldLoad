using AssetsTools.NET.Extra;
using BundleLoader;
using Modding;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManagement = UnityEngine.SceneManagement;

namespace WorldLoader
{
    public class PatchSceneLoad
    {
        public void Patch()
        {
            Type t = typeof(SceneManagement.SceneManager);
            
            if (t == null)
                return;

            Hook hook = new Hook
            (
                t.GetMethod("LoadSceneAsync", new Type[] { typeof(string), typeof(LoadSceneMode) }),
                GetType().GetMethod("LoadSceneAsync", new Type[] { typeof(Func<string, LoadSceneMode, AsyncOperation>), typeof(string), typeof(LoadSceneMode) })
            );
            //Hook hook2 = new Hook
            //(
            //    t.GetMethod("UnloadScene", new Type[] { typeof(string) }),
            //    GetType().GetMethod("UnloadScene", new Type[] { typeof(Func<string, bool>), typeof(string) })
            //);
        }
        private static List<string> loadedScenes;
        public static AsyncOperation LoadSceneAsync(Func<string, LoadSceneMode, AsyncOperation> orig, string sceneName, LoadSceneMode mode)
        {
            Debug.Log("loading scene " + sceneName);
            if (WorldLoader.diffDatabase.ContainsKey(sceneName) || sceneName.EndsWith("_mod"))
            {
                Debug.Log("intercepting scene!");
                if (loadedScenes == null)
                {
                    loadedScenes = new List<string>();
                }
                string realSceneName = sceneName;
                if (realSceneName.EndsWith("_mod"))
                {
                    realSceneName = realSceneName.Substring(0, realSceneName.Length - 4);
                    Debug.Log("removing _mod");
                }
                if (realSceneName.StartsWith("level")) //compat purposes
                {
                    string oldName = realSceneName;
                    realSceneName = SceneManager.GetSceneName(int.Parse(realSceneName.Substring(5)));
                    Debug.Log("setting name from " + oldName + " to " + realSceneName);
                }
                string levelFileName = "level" + SceneManager.GetSceneIndex(realSceneName);
                Debug.Log("getting file " + levelFileName + " from " + realSceneName);
                if (!loadedScenes.Contains(realSceneName))
                {
                    AssetsManager am = new AssetsManager();
                    am.LoadClassPackage("cldb.dat");
                    string filePath = Path.Combine("hollow_knight_Data", levelFileName);
                    AssetsFileInstance inst = am.LoadAssetsFile(filePath, false);
                    byte[] bunDat = Loader.CreateBundleFromLevel(am, inst, realSceneName + "_mod", WorldLoader.diffDatabase[realSceneName]);
                    AssetBundle bun = AssetBundle.LoadFromMemory(bunDat);
                    loadedScenes.Add(realSceneName);
                }
                string moddedSceneName = realSceneName + "_mod";
                Debug.Log("loading " + moddedSceneName);
                return orig(moddedSceneName, mode);
            }
            else
            {
                return orig(sceneName, mode);
            }
            //Debug.Log("checking for scene " + sceneName);
            //if (sceneName.Contains("GG_Waterways") || (sceneName.Contains("level421_mod")))
            //{
            //    if (!loadedModYet)
            //    {
            //        loadedModYet = true;
            //        Debug.Log("INTERCEPTING GG_Waterways, LOADING BUNDLE");
            //
            //        string levelName = "level421";
            //        Debug.Log("found level as " + levelName);
            //        AssetsManager am = new AssetsManager();
            //        am.LoadClassPackage("cldb.dat");
            //        string filePath = Path.Combine("hollow_knight_Data", levelName);
            //        byte[] origDat = File.ReadAllBytes(filePath);
            //        AssetsFileInstance inst = am.LoadAssetsFile(filePath, false);
            //        byte[] bunDat = Loader.CreateBundleFromLevel(am, /*origDat,*/ inst);
            //        //File.WriteAllBytes("bun2.unity3d", bunDat);
            //        //AssetBundle bun = AssetBundle.LoadFromMemory(bunDat);
            //        AssetBundle bun = AssetBundle.LoadFromMemory(bunDat/*Loader.CreateBundleFromLevel(am, inst)*/);
            //        Debug.Log("loading level");
            //    }
            //    //AsyncOperation op = SceneManagement.SceneManager.LoadSceneAsync("level421_mod", mode);
            //    //op.completed += (AsyncOperation obj) =>
            //    //{
            //    //    
            //    //};
            //    return /*SceneManagement.SceneManager.LoadSceneAsync*/orig("level421_mod", mode);
            //}
            //else
            //{
            //    return orig(sceneName, mode);
            //    //SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            //}
        }
        //private static ManualResetEvent sigEvent = new ManualResetEvent(false);
        //public static bool UnloadScene(Func<string, bool> orig, string sceneName)
        //{
        //    if (sceneName == "level421_mod")
        //    {
        //        //Thread thread = new Thread(delegate() {
        //        //    UnloadSceneAsync(sceneName);
        //        //});
        //        //thread.Start();
        //        //sigEvent.WaitOne();
        //        //sigEvent.Reset();
        //        return true;
        //    }
        //    else
        //    {
        //        return orig(sceneName);
        //    }
        //}
        //public static void UnloadSceneAsync(string sceneName)
        //{
        //    try
        //    {
        //        AsyncOperation op = SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        //        op.completed += (AsyncOperation obj) =>
        //        {
        //            sigEvent.Set();
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Log("CRASH " + ex.Message);
        //        sigEvent.Set();
        //    }
        //}
    }
}
