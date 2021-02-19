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
            => UObject.Instantiate(_shiny);

        public static void HandlePreload(string name, GameObject obj)
        {
            switch (name)
            {
                case nameof(Shiny):
                    obj.name = "Randomizer Shiny";
                    _shiny = obj;
                    break;
            }
        }
    }
}
