using MonoMod;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

namespace Randomizer.Patches
{
    [MonoModPatch("global::Language.Language")]
    public class Language
    {
        public static extern string orig_Get(string key, string sheetTitle);

        public static string Get(string key, string sheetTitle)
            => Util.Lang.Get(sheetTitle, key);

        [MonoModIgnore]
        public static extern bool Has(string key, string sheetTitle);
    }
}
