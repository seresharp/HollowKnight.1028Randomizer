using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Serialized;
using UnityEngine;

using Random = System.Random;

namespace Randomizer
{
    public static class Randomization
    {
        public static void RandomizeItems(Dictionary<string, string> itemPlacements, Dictionary<string, int> itemCosts)
        {
            int seed = Environment.TickCount;
            Random rnd = new Random(seed);

            Debug.Log("[Randomizer] Randomizing with seed " + seed);

            List<string> locs = RandoResources.Locations.Select(l => l.Id).ToList();
            List<string> items = new List<string>();
            foreach (Item item in RandoResources.Items)
            {
                int num = item.Stages.Select(s => s.Repeat).Sum();
                for (int i = 1; i <= num; i++)
                {
                    items.Add(item.Id + "." + i);
                }
            }

            while (locs.Count > 0)
            {
                int locIdx = rnd.Next(locs.Count);
                int itemIdx = rnd.Next(items.Count);
                itemPlacements[items[itemIdx]] = locs[locIdx];

                Debug.Log("[Randomizer] Placing " + items[itemIdx] + " at " + locs[locIdx]);

                locs.RemoveAt(locIdx);
                items.RemoveAt(itemIdx);
            }

            while (items.Count > 0)
            {
                int shopIdx = rnd.Next(RandoResources.Shops.Length);
                int itemIdx = rnd.Next(items.Count);
                itemPlacements[items[itemIdx]] = RandoResources.Shops[shopIdx].Id;

                // Make anything that gives geo cost only 1
                Item i = RandoResources.Items.First(i => i.Id == items[itemIdx].Substring(0, items[itemIdx].IndexOf('.')));
                if (i.Stages.Any(s => s.IntActions.Any(ia => ia.FieldName == nameof(PlayerData.geo))))
                {
                    itemCosts[items[itemIdx]] = 1;
                }
                else
                {
                    itemCosts[items[itemIdx]] = rnd.Next(100, 500);
                }

                Debug.Log("[Randomizer] Placing " + items[itemIdx] + " at " + RandoResources.Shops[shopIdx].Id + " (" + itemCosts[items[itemIdx]] + " geo)");

                items.RemoveAt(itemIdx);
            }

            Debug.Log("[Randomizer] All items placed");
        }
    }
}
