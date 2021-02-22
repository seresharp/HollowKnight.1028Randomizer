using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker.Actions;
using Modding;
using MonoMod.RuntimeDetour;
using Randomizer.Serialized;
using Randomizer.Util;
using UnityEngine;

namespace Randomizer
{
    public class RandomizerMod : Mod
    {
        public static RandomizerMod Instance { get; private set; }

        public readonly SerializableStringDictionary ItemPlacements = new();
        public readonly SerializableStringList ObtainedLocations = new();
        public readonly SerializableIntDictionary ItemObtainedCounts = new();
        public readonly SerializableIntDictionary ItemCosts = new();

        public RandomizerMod() : base("Randomizer Mod")
        {
            Instance = this;

            On.PlayerData.GetBool += HookGetBool;
            On.PlayerData.SetBool += HookSetBool;
            On.GameManager.BeginScene += HookSceneChanges;
            On.HutongGames.PlayMaker.Actions.FlingObject.OnEnter += CatchFlingErrors;

            Lang.OrigLangGet = new Hook
            (
                typeof(Language.Language).GetMethod(nameof(Language.Language.Get), new Type[] { typeof(string), typeof(string) }),
                typeof(Lang).GetMethod(nameof(Lang.Get), new Type[] { typeof(string), typeof(string) })
            ).GenerateTrampoline<Func<string, string, string>>();
        }

        public override void NewGame()
        {
            ItemPlacements.Clear();
            ObtainedLocations.Clear();
            ItemObtainedCounts.Clear();
            ItemCosts.Clear();

            PlayerData.instance.hasCharm = true;
            PlayerData.instance.unchainedHollowKnight = true;
            PlayerData.instance.infectedKnightEncountered = true;
            PlayerData.instance.mageLordEncountered = true;
            PlayerData.instance.mageLordEncountered_2 = true;

            Randomization.RandomizeItems(ItemPlacements, ItemCosts);
        }

        public override ModSettings GetSaveSettings()
        {
            ModSettings settings = new ModSettings();

            settings.StringValues[nameof(ItemPlacements)] = JsonUtility.ToJson(ItemPlacements);
            settings.StringValues[nameof(ObtainedLocations)] = JsonUtility.ToJson(ObtainedLocations);
            settings.StringValues[nameof(ItemObtainedCounts)] = JsonUtility.ToJson(ItemObtainedCounts);
            settings.StringValues[nameof(ItemCosts)] = JsonUtility.ToJson(ItemCosts);

            return settings;
        }

        public override void SetSaveSettings(ModSettings settings)
        {
            ItemPlacements.Clear();
            ObtainedLocations.Clear();
            ItemObtainedCounts.Clear();
            ItemCosts.Clear();

            if (settings.StringValues.TryGetValue(nameof(ItemPlacements), out string itemPlacementsJson))
            {
                ItemPlacements.AddRange(JsonUtility.FromJson<SerializableStringDictionary>(itemPlacementsJson));
            }

            if (settings.StringValues.TryGetValue(nameof(ObtainedLocations), out string obtainedLocationsJson))
            {
                ObtainedLocations.AddRange(JsonUtility.FromJson<SerializableStringList>(obtainedLocationsJson));
            }

            if (settings.StringValues.TryGetValue(nameof(ItemObtainedCounts), out string itemObtainedCountsJson))
            {
                ItemObtainedCounts.AddRange(JsonUtility.FromJson<SerializableIntDictionary>(itemObtainedCountsJson));
            }

            if (settings.StringValues.TryGetValue(nameof(ItemCosts), out string itemCostsJson))
            {
                ItemCosts.AddRange(JsonUtility.FromJson<SerializableIntDictionary>(itemCostsJson));
            }

            Log("Loaded save");
            Log("Obtained items:");
            foreach (string str in ObtainedLocations)
            {
                Log(str + " - " + ItemPlacements.First(i => i.Value == str).Key);
            }
        }

        public override List<(string, string, string)> GetPreloads()
            => new()
            {
                (Constants.TUTORIAL_LEVEL, "_Props/Chest/Item/Shiny Item (1)", nameof(ObjectCache.Shiny))
            };

        public override void SetPreload(string id, GameObject obj)
            => ObjectCache.HandlePreload(id, obj);

        private bool HookGetBool(On.PlayerData.orig_GetBool orig, PlayerData self, string boolName)
        {
            if (string.IsNullOrEmpty(boolName))
            {
                return false;
            }

            if (boolName.StartsWith("CheckHasRequired."))
            {
                string locId = boolName.Substring(boolName.IndexOf('.') + 1);
                Location loc = RandoResources.Locations.FirstOrDefault(l => l.Id == locId)
                    ?? RandoResources.Shops.FirstOrDefault(l => l.Id == locId);

                return loc?.HasRequired() ?? false;
            }

            foreach (Location shop in RandoResources.Shops)
            {
                if (!boolName.StartsWith(shop.Id + "."))
                {
                    continue;
                }

                return ObtainedLocations.Contains(boolName);
            }

            return orig(self, boolName);
        }

        private void HookSetBool(On.PlayerData.orig_SetBool orig, PlayerData self, string boolName, bool value)
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

                    ObtainedLocations.Add(boolName);
                }
                else
                {
                    ObtainedLocations.Remove(boolName);
                }

                return;
            }

            orig(self, boolName, value);
        }

        private void HookSceneChanges(On.GameManager.orig_BeginScene orig, GameManager self)
        {
            try
            {
                SceneChanges.PatchScene(self.sceneName);
            }
            catch (Exception e)
            {
                Log($"Error patching scene '{self.sceneName}'\n{e}");
            }

            orig(self);
        }

        private void CatchFlingErrors(On.HutongGames.PlayMaker.Actions.FlingObject.orig_OnEnter orig, FlingObject self)
        {
            try
            {
                orig(self);
            }
            catch
            {
                self.Finish();
            }
        }
    }
}
