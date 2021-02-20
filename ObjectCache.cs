using System.Collections.Generic;
using UnityEngine;

using UObject = UnityEngine.Object;

namespace Randomizer
{
    public static class ObjectCache
    {
        public static readonly Dictionary<string, Dictionary<string, string>> Preloads = new Dictionary<string, Dictionary<string, string>>
        {
            [Constants.TUTORIAL_LEVEL] = new Dictionary<string, string>
            {
                [nameof(Shiny)] = "_Props/Chest/Item/Shiny Item (1)"
            }
        };

        private static GameObject _shiny;
        public static GameObject Shiny
        {
            get
            {
                GameObject obj = UObject.Instantiate(_shiny);
                obj.name = "Randomizer Shiny";
                return obj;
            }
        }

        public static int ShinyStateCount { get; private set; }

        public static void HandlePreload(string name, GameObject obj)
        {
            switch (name)
            {
                case nameof(Shiny):
                    obj.name = "Randomizer Shiny Prefab";
                    _shiny = obj;
                    ShinyStateCount = _shiny.LocateMyFSM("Shiny Control").FsmStates.Length;
                    break;
            }
        }
    }
}
