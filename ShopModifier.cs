﻿using System.Collections.Generic;
using System.Linq;
using Randomizer.Serialized;
using Randomizer.Util;
using UnityEngine;

using PD = Randomizer.Patches.PlayerData;
using UObject = UnityEngine.Object;

namespace Randomizer
{
    public static class ShopModifier
    {
        public static void SetShopItems(GameObject shopObj, string[] items)
        {
            ShopMenuStock shop = shopObj.GetComponent<ShopMenuStock>();
            GameObject itemPrefab = UObject.Instantiate(shop.stock[0]);

            // Remove all charm type items from the store
            List<GameObject> newStock = new List<GameObject>();

            Debug.Log($"[Randomizer] New shop contents ({shopObj.scene.name}) - ");
            foreach (string itemId in items)
            {
                Item item = RandoResources.Items.First(i => i.Id == itemId.Substring(0, itemId.IndexOf('.')));
                if (!item.TryGetNextStage(out ItemStage stage))
                {
                    continue;
                }

                // Create a new shop item for this item def
                GameObject newItemObj = UObject.Instantiate(itemPrefab);

                // Apply all the stored values
                ShopItemStats stats = newItemObj.GetComponent<ShopItemStats>();
                stats.playerDataBoolName = PD.instance.itemPlacements[itemId] + "." + itemId;
                stats.nameConvo = stage.Shop.Name;
                stats.descConvo = stage.Shop.Description;
                stats.requiredPlayerDataBool = PD.instance.itemPlacements[itemId] == "Sly_(Key)" ? "hasSlykey" : "";
                stats.notchCostBool = string.Empty;
                stats.cost = PD.instance.itemCosts[itemId];

                // Need to set all these to make sure the item doesn't break in one of various ways
                stats.priceConvo = stats.cost.ToString();
                stats.specialType = 2;
                stats.charmsRequired = 0;
                stats.relicNumber = 0;

                // Apply the sprite for the UI
                stats.transform.Find("Item Sprite").gameObject.GetComponent<SpriteRenderer>().sprite =
                    Sprites.Get(stage.Shop.Sprite);

                newStock.Add(newItemObj);
                Debug.Log("[Randomizer] " + itemId);
            }

            foreach (GameObject item in shop.stock)
            {
                // Update normal stock (specialType: 0 = lantern, elegant key, quill; 1 = mask, 2 = charm, 3 = vessel, 4-7 = relics, 8 = notch, 9 = map, 10 = simple key, 11 = egg, 12-14 = repair fragile, 15 = salubra blessing, 16 = map pin, 17 = map marker)
                if (item.GetComponent<ShopItemStats>().specialType == 9 || item.GetComponent<ShopItemStats>().playerDataBoolName == "hasQuill")
                {
                    continue;
                }

                string shopBool = item.GetComponent<ShopItemStats>().playerDataBoolName;
                if (!RandoResources.Items.Any(i => i.Stages.Any(s => s.BoolActions.Any(b => b.FieldName == shopBool))) && !shopBool.StartsWith("salubraNotch"))
                {
                    // LogicManager doesn't know about this shop item, which means it's never potentially randomized. Put it back!
                    newStock.Add(item);
                }
            }

            shop.stock = newStock.ToArray();

            // Update alt stock; Sly only
            if (shop.stockAlt != null)
            {
                // Save unchanged list for potential alt stock
                List<GameObject> altStock = new List<GameObject>();
                altStock.AddRange(newStock);

                foreach (GameObject item in shop.stockAlt)
                {
                    string shopBool = item.GetComponent<ShopItemStats>().playerDataBoolName;
                    if (!RandoResources.Items.Any(i => i.Stages.Any(s => s.BoolActions.Any(b => b.FieldName == shopBool))) && !newStock.Contains(item))
                    {
                        altStock.Add(item);
                    }
                }

                shop.stockAlt = altStock.ToArray();
            }

            FSMUtility.SendEventToGameObject(GameObject.Find("Item List"), "RESET");
        }
    }
}