using System.Collections.Generic;
using MonoMod;
using Randomizer.Serialized;

namespace Randomizer.Patches
{
    [MonoModPatch("global::SaveGameData")]
    public class SaveGameData : global::SaveGameData
    {
        [MonoModIgnore] public new PlayerData playerData;

        public SerializableStringDictionary itemPlacements;
        public List<string> obtainedLocations;
        public SerializableIntDictionary itemObtainedCounts;
        public SerializableIntDictionary itemCosts;

        [MonoModConstructor]
        public SaveGameData(PlayerData pd, SceneData sd): base(pd, sd)
        {
            itemPlacements = pd.itemPlacements;
            obtainedLocations = pd.obtainedLocations;
            itemObtainedCounts = pd.itemObtainedCounts;
            itemCosts = pd.itemCosts;
        }
    }
}
