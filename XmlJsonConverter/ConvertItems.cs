using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Randomizer.Serialized;

namespace XmlConverter
{
    public static class ConvertItems
    {
        private static Dictionary<string, string[]> _additive = new()
        {
            ["Dreamer"] = new[] { "Lurien", "Monomon", "Herrah" },
            ["Fireball"] = new[] { "Vengeful_Spirit", "Shade_Soul" },
            ["Dive"] = new[] { "Desolate_Dive", "Descending_Dark" },
            ["Wraiths"] = new[] { "Howling_Wraiths", "Abyss_Shriek" },
            ["Dash"] = new[] { "Mothwing_Cloak", "Shade_Cloak" },
            ["Dream_Nail"] = new[] { "Dream_Nail", "Awoken_Dream_Nail" },
            ["Kingsoul"] = new[] { "Queen_Fragment", "King_Fragment", "Void_Heart" }
        };

        private static List<Item> _items = new();

        public static void Convert()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("items.xml");

            Dictionary<string, List<ItemStage>> additiveStages = new();
            foreach (string additiveId in _additive.Select(p => p.Key))
            {
                additiveStages[additiveId] = new List<ItemStage>();
            }

            foreach (XmlNode itemXml in xml.SelectNodes("randomizer/item"))
            {
                GetStage(itemXml, out string id, out ItemStage stage);
                if (stage == null)
                {
                    continue;
                }

                if (_additive.Any(p => p.Value.Contains(id)))
                {
                    additiveStages[_additive.First(p => p.Value.Contains(id)).Key].Add(stage);
                }
                else
                {
                    _items.Add(new Item(id, new[] { stage }));
                }
            }

            foreach (KeyValuePair<string, List<ItemStage>> pair in additiveStages)
            {
                _items.Add(new Item(pair.Key, pair.Value.ToArray()));
            }

            File.WriteAllText("items.cs", _items.ToArray().Repr(true));
        }

        private static void GetStage(XmlNode itemXml, out string id, out ItemStage stage)
        {
            id = itemXml.Attributes["name"].InnerText;
            stage = null;

            List<PlayerField<bool>> boolActions = new();
            List<PlayerField<int>> intActions = new();
            List<string> fsmEvents = new();
            List<string> randoCallbacks = new();
            int repeat = 1;

            switch (id)
            {
                case "Lurien":
                    boolActions.Add(new PlayerField<bool>(nameof(PlayerData.lurienDefeated), true));
                    intActions.Add(new PlayerField<int>(nameof(PlayerData.guardiansDefeated), 1, true));
                    break;
                case "Monomon":
                    boolActions.Add(new PlayerField<bool>(nameof(PlayerData.monomonDefeated), true));
                    intActions.Add(new PlayerField<int>(nameof(PlayerData.guardiansDefeated), 1, true));
                    break;
                case "Herrah":
                    boolActions.Add(new PlayerField<bool>(nameof(PlayerData.hegemolDefeated), true));
                    intActions.Add(new PlayerField<int>(nameof(PlayerData.guardiansDefeated), 1, true));
                    break;
                case "Vengeful_Spirit":
                case "Shade_Soul":
                    boolActions.Add(new PlayerField<bool>(nameof(PlayerData.hasSpell), true));
                    intActions.Add(new PlayerField<int>(nameof(PlayerData.fireballLevel), 1, true));
                    break;
                case "Desolate_Dive":
                case "Descending_Dark":
                    boolActions.Add(new PlayerField<bool>(nameof(PlayerData.hasSpell), true));
                    intActions.Add(new PlayerField<int>(nameof(PlayerData.quakeLevel), 1, true));
                    break;
                case "Howling_Wraiths":
                case "Abyss_Shriek":
                    boolActions.Add(new PlayerField<bool>(nameof(PlayerData.hasSpell), true));
                    intActions.Add(new PlayerField<int>(nameof(PlayerData.screamLevel), 1, true));
                    break;
                case "Isma's_Tear":
                    fsmEvents.Add("GET ACID ARMOUR");
                    goto case "DEFAULT";
                case "Cyclone_Slash":
                case "Dash_Slash":
                case "Great_Slash":
                    boolActions.Add(new(nameof(PlayerData.hasNailArt), true));
                    randoCallbacks.Add("ItemCallbacks.CheckNailArts");
                    goto case "DEFAULT";
                case "Mothwing_Cloak":
                    boolActions.Add(new(nameof(PlayerData.canDash), true));
                    goto case "DEFAULT";
                case "Shade_Cloak":
                    boolActions.Add(new(nameof(PlayerData.canShadowDash), true));
                    goto case "DEFAULT";
                case "Crystal_Heart":
                    boolActions.Add(new(nameof(PlayerData.canSuperDash), true));
                    goto case "DEFAULT";
                case "Mantis_Claw":
                    boolActions.Add(new(nameof(PlayerData.canWallJump), true));
                    goto case "DEFAULT";
                case "DEFAULT":
                default:
                    switch (GetChildText(itemXml, "action"))
                    {
                        case "Int":
                            id = id.Substring(0, id.IndexOf("-"));
                            string idCopy = id;
                            if (_items.Any(i => i.Id == idCopy))
                            {
                                return;
                            }

                            intActions.Add(new(GetChildText(itemXml, "intName") ?? throw new Exception(), 1, true));
                            repeat = id switch
                            {
                                "Simple_Key" => 3,
                                "Charm_Notch" => 3,
                                "Pale_Ore" => 6,
                                "Rancid_Egg" => 20,
                                _ => throw new Exception()
                            };

                            break;
                        case "Grub":
                            if (_items.Any(i => i.Id == "Grub"))
                            {
                                return;
                            }

                            id = "Grub";
                            intActions.Add(new(nameof(PlayerData.grubsCollected), 1, true));
                            randoCallbacks.Add("MakeGrubNoise");
                            repeat = 46;
                            break;
                        case "MaskShard":
                            if (_items.Any(i => i.Id == "Mask_Shard"))
                            {
                                return;
                            }

                            id = "Mask_Shard";
                            boolActions.Add(new(nameof(PlayerData.heartPieceCollected), true));
                            intActions.Add(new(nameof(PlayerData.heartPieces), 1, true));
                            randoCallbacks.Add("ItemCallbacks.CheckMaskShards");
                            repeat = 16;
                            break;
                        case "VesselFragment":
                            if (_items.Any(i => i.Id == "Vessel_Fragment"))
                            {
                                return;
                            }

                            id = "Vessel_Fragment";
                            boolActions.Add(new(nameof(PlayerData.vesselFragmentCollected), true));
                            intActions.Add(new(nameof(PlayerData.vesselFragments), 1, true));
                            randoCallbacks.Add("ItemCallbacks.CheckVesselFragments");
                            repeat = 9;
                            break;
                        case "SpawnGeo":
                            intActions.Add(new(nameof(PlayerData.geo), int.Parse(GetChildText(itemXml, "geo")), true));
                            randoCallbacks.Add("ItemCallbacks.UpdateGeoCounter");
                            break;
                        case "WanderersJournal":
                            if (_items.Any(i => i.Id == "Wanderer's_Journal"))
                            {
                                return;
                            }

                            id = "Wanderer's_Journal";
                            boolActions.Add(new PlayerField<bool>(nameof(PlayerData.foundTrinket1), true));
                            intActions.Add(new PlayerField<int>(nameof(PlayerData.trinket1), 1, true));
                            repeat = 14;
                            break;
                        case "HallownestSeal":
                            if (_items.Any(i => i.Id == "Hallownest_Seal"))
                            {
                                return;
                            }

                            id = "Hallownest_Seal";
                            boolActions.Add(new PlayerField<bool>(nameof(PlayerData.foundTrinket2), true));
                            intActions.Add(new PlayerField<int>(nameof(PlayerData.trinket2), 1, true));
                            repeat = 17;
                            break;
                        case "KingsIdol":
                            if (_items.Any(i => i.Id == "King's_Idol"))
                            {
                                return;
                            }

                            id = "King's_Idol";
                            boolActions.Add(new PlayerField<bool>(nameof(PlayerData.foundTrinket3), true));
                            intActions.Add(new PlayerField<int>(nameof(PlayerData.trinket3), 1, true));
                            repeat = 7;
                            break;
                        case "ArcaneEgg":
                            if (_items.Any(i => i.Id == "Arcane_Egg"))
                            {
                                return;
                            }

                            id = "Arcane_Egg";
                            boolActions.Add(new PlayerField<bool>(nameof(PlayerData.foundTrinket4), true));
                            intActions.Add(new PlayerField<int>(nameof(PlayerData.trinket4), 1, true));
                            repeat = 4;
                            break;
                        case "Essence":
                            intActions.Add(new(nameof(PlayerData.dreamOrbs), int.Parse(GetChildText(itemXml, "geo")), true));
                            fsmEvents.Add("DREAM ORB COLLECT");
                            break;
                        case "Lifeblood":
                            for (int i = 0; i < int.Parse(GetChildText(itemXml, "lifeblood")); i++)
                            {
                                fsmEvents.Add("ADD BLUE HEALTH");
                            }

                            break;
                        case "Charm":
                            boolActions.Add(new(nameof(PlayerData.hasCharm), true));
                            intActions.Add(new(nameof(PlayerData.charmsOwned), 1, true));

                            randoCallbacks.Add("ItemCallbacks.CheckNotches");
                            goto case "DEFAULT";
                        case "Kingsoul":
                            if (id == "Queen_Fragment")
                            {
                                boolActions.Add(new(nameof(PlayerData.hasCharm), true));
                                boolActions.Add(new(nameof(PlayerData.gotCharm_36), true));
                                intActions.Add(new(nameof(PlayerData.charmsOwned), 1, true));
                                intActions.Add(new(nameof(PlayerData.royalCharmState), 1));

                                randoCallbacks.Add("ItemCallbacks.CheckNotches");
                            }
                            else if (id == "King_Fragment")
                            {
                                intActions.Add(new(nameof(PlayerData.royalCharmState), 3));
                            }
                            else
                            {
                                boolActions.Add(new(nameof(PlayerData.gotShadeCharm), true));
                                intActions.Add(new(nameof(PlayerData.charmCost_36), 0));
                                intActions.Add(new(nameof(PlayerData.royalCharmState), 4));
                            }

                            break;
                        case "Stag":
                            intActions.Add(new(nameof(PlayerData.stationsOpened), 1, true));
                            goto case "DEFAULT";
                        case "DEFAULT":
                        default:
                            boolActions.Add(new PlayerField<bool>(GetChildText(itemXml, "boolName") ?? throw new Exception(), true));
                            break;
                    }

                    break;
            }

            if (boolActions.Count == 0 && intActions.Count == 0 && fsmEvents.Count == 0 && randoCallbacks.Count == 0)
            {
                Console.WriteLine(id);
                return;
            }

            stage = new ItemStage
            (
                boolActions.ToArray(),
                intActions.ToArray(),
                fsmEvents.ToArray(),
                randoCallbacks.ToArray(),
                new PopupInfo
                (
                    GetChildText(itemXml, "nameKey") ?? throw new Exception(),
                    GetChildText(itemXml, "bigSpriteKey") ?? GetChildText(itemXml, "shopSpriteKey") ?? throw new Exception(),
                    GetChildText(itemXml, "takeKey") ?? string.Empty,
                    GetChildText(itemXml, "buttonKey") ?? string.Empty,
                    GetChildText(itemXml, "descOneKey") ?? string.Empty,
                    GetChildText(itemXml, "descTwoKey") ?? string.Empty
                ),
                new ShopInfo
                (
                    GetChildText(itemXml, "nameKey") ?? throw new Exception(),
                    GetChildText(itemXml, "shopSpriteKey") ?? throw new Exception(),
                    GetChildText(itemXml, "shopDescKey") ?? throw new Exception()
                ),
                repeat
            );
        }

        private static string GetChildText(XmlNode node, string childName)
         => node.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == childName)?.InnerText;
    }
}
