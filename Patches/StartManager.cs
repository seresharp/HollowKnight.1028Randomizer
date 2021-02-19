using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoMod;
using UnityEngine;
using UnityEngine.SceneManagement;

using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Randomizer.Patches
{
    [MonoModPatch("global::StartManager")]
    public class StartManager : global::StartManager
    {
        [MonoModReplace]
        private void Start()
        {
            StartCoroutine(Preload());
        }

        private IEnumerator Preload()
        {
            progressIndicator.gameObject.SetActive(true);
            progressIndicator.minValue = 0;
            progressIndicator.maxValue = ObjectCache.Preloads.Count + 1;

            int preloadIdx = 0;
            foreach (string sceneName in ObjectCache.Preloads.Keys)
            {
                AsyncOperation loadop = USceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                while (!loadop.isDone)
                {
                    progressIndicator.value = preloadIdx + loadop.progress / .9f;
                    yield return null;
                }

                Scene scene = USceneManager.GetSceneByName(sceneName);
                GameObject[] rootObjects = scene.GetRootGameObjects();
                Dictionary<string, string> objNames = ObjectCache.Preloads[sceneName];
                Dictionary<string, GameObject> objs = new Dictionary<string, GameObject>();

                foreach (string objName in objNames.Keys)
                {
                    string objPath = objNames[objName];
                    string rootName;
                    string childName;
                    if (objPath.Contains('/'))
                    {
                        int slash = objPath.IndexOf('/');
                        rootName = objPath.Substring(0, slash);
                        childName = objPath.Substring(slash + 1);
                    }
                    else
                    {
                        rootName = objPath;
                        childName = null;
                    }

                    GameObject obj = rootObjects.First(o => o.name == rootName);
                    if (childName != null)
                    {
                        obj = obj.transform.Find(childName).gameObject;
                    }

                    obj = Instantiate(obj);
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);

                    ObjectCache.HandlePreload(objName, obj);
                }

                USceneManager.UnloadScene(scene);
                preloadIdx++;
            }

            AsyncOperation menuLoadop = USceneManager.LoadSceneAsync(Constants.MENU_SCENE);
            while (!menuLoadop.isDone)
            {
                progressIndicator.value = preloadIdx + menuLoadop.progress / .9f;
                yield return null;
            }
        }
    }
}
