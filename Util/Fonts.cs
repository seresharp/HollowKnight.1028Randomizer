using System.Collections.Generic;
using UnityEngine;

namespace Randomizer.Util
{
    public static class Fonts
    {
        private static readonly Dictionary<string, Font> FontCache;
        private static readonly Font Perpetua;

        static Fonts()
        {
            FontCache = new Dictionary<string, Font>();
            foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (FontCache.ContainsKey(f.name))
                {
                    continue;
                }

                FontCache.Add(f.name, f);

                if (Perpetua == null && f.name == "Perpetua")
                {
                    Perpetua = f;
                }
            }
        }

        public static Font Get(string name)
        {
            if (FontCache.TryGetValue(name, out Font font))
            {
                return font;
            }

            Debug.LogWarning($"Non-existent font \"{name}\" requested");

            // Default to perpetua if the name doesn't exist
            return Perpetua;
        }
    }
}
