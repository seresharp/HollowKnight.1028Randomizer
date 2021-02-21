using UnityEngine;

using UObject = UnityEngine.Object;

namespace Randomizer
{
    public static class ObjectCache
    {
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
