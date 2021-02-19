using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Randomizer.Serialized;

namespace XmlConverter
{
    public static class ConvertLocations
    {
        private static List<Location> _locations = new();

        public static void Convert()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("items.xml");

            foreach (XmlNode itemXml in xml.SelectNodes("randomizer/item"))
            {
                string id = itemXml.Attributes["name"].InnerText;
                string scene = GetChildText(itemXml, "sceneName") ?? throw new Exception();
                string obj = GetChildText(itemXml, "objectName");

                if (obj == "Shop Menu")
                {
                    continue;
                }

                string[] destroyObjects = id switch
                {
                    "Vessel_Fragment-Basin" => new[] { "Fountain Donation" },
                    "Mask_Shard-5_Grubs" => new[] { "Reward 5" },
                    "Grubsong" => new[] { "Reward 10" },
                    "Rancid_Egg-Grubs" => new[] { "Reward 16" },
                    "Hallownest_Seal-Grubs" => new[] { "Reward 23" },
                    "Pale_Ore-Grubs" => new[] { "Reward 31" },
                    "King's_Idol-Grubs" => new[] { "Reward 38" },
                    "Grubberfly's_Elegy" => new[] { "Reward 46" },
                    "Vengeful_Spirit" => new[] { "Bone Gate" },
                    "Herrah" => new[] { "Dreamer Hegemol", "Dream Enter", "Dream Impact", "Shield" },
                    "Mothwing_Cloak" => new[] { "Dreamer Scene 1", "Hornet Saver", "Cutscene Dreamer", "Dream Scene Activate" },
                    "Monomon" => new[] { "Quirrel Wounded", "Quirrel", "Monomon", "Dream Enter", "Dream Impact", "Shield" },
                    "Desolate_Dive" => new[] { "Roof Collider Battle" },
                    "Lurien" => new[] { "Dreamer Lurien", "Dream Enter", "Dream Impact", "Shield" },
                    "King's_Brand" => new[] { "Avalanche End" },
                    "Resting_Grounds_Stag" => new[] { "Ruins Lever" },
                    "Grub-Collector_1" => new[] { "Grub Bottle" },
                    "Grub-Collector_2" => new[] { "Grub Bottle (1)" },
                    "Grub-Collector_3" => new[] { "Grub Bottle (2)" },
                    "Deepnest_Map-Right_[Gives_Quill]" => new[] { "Cornifer Deepnest" },
                    "Resting_Grounds_Map" => new[] { "Cornifer Card" },
                    _ when new[]
                    {
                        "Crossroads_Map", "Greenpath_Map", "Fog_Canyon_Map", "Fungal_Wastes_Map", "Deepnest_Map-Upper",
                        "Ancient_Basin_Map", "Kingdom's_Edge_Map", "City_of_Tears_Map", "Royal_Waterways_Map", "Howling_Cliffs_Map",
                        "Crystal_Peak_Map", "Queen's_Gardens_Map"
                    }.Contains(id) => new[] { "Cornifer", "Cornifer Card" },
                    _ => new string[0]
                };

                string[] fsmEvents = id switch
                {
                    "Mothwing_Cloak" => new[] { "BG OPEN" },
                    "Desolate_Dive" => new[] { "BG OPEN" },
                    _ => new string[0]
                };

                string[] randoCallbacks = id switch
                {
                    _ when new[]
                    {
                        "Crossroads_Stag", "Greenpath_Stag", "Queen's_Station_Stag", "Queen's_Gardens_Stag",
                        "City_Storerooms_Stag", "King's_Station_Stag", "Resting_Grounds_Stag", "Distant_Village_Stag",
                        "Hidden_Station_Stag", "Stag_Nest_Stag"
                    }.Contains(id) => new[] { "LocationCallbacks.PatchStagStation" },
                    _ => new string[0]
                };

                PlayerField<int>[] requiredInts = new PlayerField<int>[0];
                PlayerField<bool>[] requiredBools = new PlayerField<bool>[0];
                string[] requiredCallbacks = new string[0];

                string costType = GetChildText(itemXml, "costType");
                if (costType != null)
                {
                    switch (costType)
                    {
                        case "Dreamnail":
                            requiredBools = new[] { new PlayerField<bool>(nameof(PlayerData.hasDreamNail), true) };
                            break;
                        case "Essence":
                            requiredInts = new[] { new PlayerField<int>(nameof(PlayerData.dreamOrbs), int.Parse(GetChildText(itemXml, "cost"))) };
                            break;
                        case "Wraiths":
                            requiredInts = new[] { new PlayerField<int>(nameof(PlayerData.screamLevel), 1) };
                            break;
                        case "Grub":
                            requiredInts = new[] { new PlayerField<int>(nameof(PlayerData.grubsCollected), int.Parse(GetChildText(itemXml, "cost"))) };
                            break;
                        case "whisperingRoot":
                            requiredCallbacks = new[] { "LocationCallbacks.CheckWhisperingRoot" };
                            break;
                        default:
                            throw new Exception(id + " - " + costType);
                    }
                }

                if (obj != null)
                {
                    _locations.Add(new ObjectLocation(id, scene, destroyObjects, fsmEvents, randoCallbacks, requiredInts, requiredBools, requiredCallbacks, obj));
                    continue;
                }

                float x = float.Parse(GetChildText(itemXml, "x"));
                float y = float.Parse(GetChildText(itemXml, "y"));
                _locations.Add(new NewLocation(id, scene, destroyObjects, fsmEvents, randoCallbacks, requiredInts, requiredBools, requiredCallbacks, x, y));
            }

            Location[] shops = new Location[]
            {
                new ObjectLocation("Sly", "Room_Shop", new string[0], new string[0], new string[0], new PlayerField<int>[0], new PlayerField<bool>[0], new string[0], "Shop Menu"),
                new ObjectLocation("Sly_(Key)", "Room_Shop", new string[0], new string[0], new string[0], new PlayerField<int>[0], new PlayerField<bool>[] { new("gaveSlyKey", true) }, new string[0], "Shop Menu"),
                new ObjectLocation("Iselda", "Room_mapper", new string[0], new string[0], new string[0], new PlayerField<int>[0], new PlayerField<bool>[0], new string[0], "Shop Menu"),
                new ObjectLocation("Salubra", "Room_Charm_Shop", new string[0], new string[0], new string[0], new PlayerField<int>[0], new PlayerField<bool>[0], new string[0], "Shop Menu"),
                new ObjectLocation("Leg_Eater", "Fungus2_26", new string[0], new string[0], new string[0], new PlayerField<int>[0], new PlayerField<bool>[0], new string[0], "Shop Menu")
            };

            File.WriteAllText("locations.cs", _locations.Select(loc => (IReproduceable)loc).ToArray().Repr(true));
            File.WriteAllText("shops.cs", shops.Select(loc => (IReproduceable)loc).ToArray().Repr(true));
        }

        private static string GetChildText(XmlNode node, string childName)
         => node.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == childName)?.InnerText;
    }
}
