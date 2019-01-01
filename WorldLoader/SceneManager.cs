using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorldLoader
{
    public static class SceneManager
    {
        private static List<string> sceneNames;
        public static void LoadSceneNames()
        {
            int nextIndex = 0;
            string lastName = "";
            sceneNames = new List<string>();
            while (true)
            {
                //unity has no way of checking only scene name
                //so we build a list of paths and reverse search
                lastName = SceneUtility.GetScenePathByBuildIndex(nextIndex++);
                if (lastName == "")
                    break;
                string sceneName = lastName;
                if (sceneName.Contains("/"))
                {
                    sceneName = lastName.Substring(lastName.LastIndexOf('/') + 1);
                }
                sceneName = sceneName.Substring(0, sceneName.IndexOf('.'));
                sceneNames.Add(sceneName);
            }
        }
        public static int GetSceneIndex(string name)
        {
            return sceneNames.IndexOf(name);
        }
        public static string GetSceneName(int index)
        {
            return sceneNames[index];
        }
    }
}
