using System;
using System.Collections.Generic;
using System.Linq;
using MonoMod;
using Randomizer.Serialized;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

namespace Randomizer.Patches
{
    [MonoModPatch("global::PlayerData")]
    public class PlayerData : global::PlayerData
    {
        [MonoModIgnore]
        public static new PlayerData instance { get; set; }

        [NonSerialized] public SerializableStringDictionary itemPlacements;
        [NonSerialized] public List<string> obtainedLocations;
        [NonSerialized] public SerializableIntDictionary itemObtainedCounts;
        [NonSerialized] public SerializableIntDictionary itemCosts;

        [MonoModConstructor]
        protected PlayerData()
        {
            itemPlacements = new SerializableStringDictionary();
            obtainedLocations = new List<string>();
            itemObtainedCounts = new SerializableIntDictionary();
            itemCosts = new SerializableIntDictionary();

            hasCharm = true;
            unchainedHollowKnight = true;
            encounteredMimicSpider = true;
            infectedKnightEncountered = true;
            mageLordEncountered = true;
            mageLordEncountered_2 = true;
        }

        public void RandomizeItems()
        {
            Randomization.RandomizeItems(itemPlacements, itemCosts);
        }

        public extern bool orig_GetBool(string boolName);
        public extern void orig_SetBool(string boolName, bool value);

        public new bool GetBool(string boolName)
        {
            if (string.IsNullOrEmpty(boolName))
            {
                return false;
            }

            foreach (Location shop in RandoResources.Shops)
            {
                if (!boolName.StartsWith(shop.Id + "."))
                {
                    continue;
                }

                return obtainedLocations.Contains(boolName);
            }

            return orig_GetBool(boolName);
        }

        public new void SetBool(string boolName, bool value)
        {
            if (string.IsNullOrEmpty(boolName))
            {
                return;
            }

            foreach (Location shop in RandoResources.Shops)
            {
                if (!boolName.StartsWith(shop.Id + "."))
                {
                    continue;
                }

                if (value)
                {
                    string itemId = boolName.Replace(shop.Id + ".", "");
                    itemId = itemId.Substring(0, itemId.IndexOf('.'));
                    RandoResources.Items.First(i => i.Id == itemId).TryCollect(out _);

                    obtainedLocations.Add(boolName);
                }
                else
                {
                    obtainedLocations.Remove(boolName);
                }

                return;
            }

            orig_SetBool(boolName, value);
        }
    }
}
