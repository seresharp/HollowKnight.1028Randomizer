using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Randomizer.Util
{
    public static class Sprites
    {
        private static readonly Dictionary<string, Sprite> _spriteCache;

        static Sprites()
        {
            _spriteCache = new Dictionary<string, Sprite>();
            Assembly asm = Assembly.GetExecutingAssembly();

            foreach (string resource in asm.GetManifestResourceNames()
                .Where(name => name.ToLower().EndsWith(".png")))
            {
                try
                {
                    using Stream stream = asm.GetManifestResourceStream(resource);
                    if (stream == null)
                    {
                        continue;
                    }

                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    // Create texture from bytes
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(buffer, true);

                    string resName = Path.GetFileNameWithoutExtension(resource)
                        .Replace("Randomizer.Resources.", "");

                    // Create sprite from texture
                    _spriteCache.Add(resName,
                        Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load sprite '{resource}'\n{e}");
                }
            }
        }

        public static Sprite Get(string name)
            => _spriteCache.TryGetValue(name, out Sprite spr) ? spr : _spriteCache["NullTex"];
    }
}
