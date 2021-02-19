using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Randomizer.Util
{
    public static class SceneExtensions
    {
        public static GameObject FindGameObjectInChildren(this GameObject gameObject, string name)
        {
            if (gameObject == null)
            {
                return null;
            }

            return gameObject.GetComponentsInChildren<Transform>(true)
                .Where(t => t.name == name)
                .Select(t => t.gameObject).FirstOrDefault();
        }

        public static GameObject FindGameObject(this Scene scene, string name)
        {
            if (!scene.IsValid())
            {
                return null;
            }

            try
            {
                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    if (go == null)
                    {
                        continue;
                    }

                    GameObject found = go.FindGameObjectInChildren(name);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("FindGameObject failed:\n" + e.Message);
            }

            return null;
        }
    }
}
