using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Randomizer.FsmStateActions;
using Randomizer.Serialized;
using Randomizer.Util;
using UnityEngine;

using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Randomizer
{
    public static class SceneChanges
    {
        public static void PatchScene(string scene)
        {
            Dictionary<GameObject, List<string>> shopItems = new Dictionary<GameObject, List<string>>();

            foreach (KeyValuePair<string, string> pair in RandomizerMod.Instance.ItemPlacements)
            {
                string itemId = pair.Key.Substring(0, pair.Key.IndexOf('.'));
                string locId = pair.Value;

                Item item = RandoResources.Items.FirstOrDefault(i => i.Id == itemId);
                Location loc = RandoResources.Locations.FirstOrDefault(l => l.Id == locId)
                    ?? RandoResources.Shops.FirstOrDefault(s => s.Id == locId);

                if (item == null)
                {
                    RandomizerMod.Instance.Log("Failed to find item " + itemId + " in resources, skipping");
                    continue;
                }

                if (loc == null)
                {
                    RandomizerMod.Instance.Log("Failed to find location " + locId + " in resources, skipping");
                    continue;
                }

                if (loc.Scene != scene)
                {
                    continue;
                }

                RandomizerMod.Instance.Log("Patching item: " + locId + " -> " + itemId);

                loc.SceneLoaded();

                PlayMakerFSM fsm;
                if (loc is ObjectLocation objLoc)
                {
                    GameObject obj = USceneManager.GetSceneByName(scene).FindGameObject(objLoc.MainObject);
                    if (obj.name == "Shop Menu")
                    {
                        if (!shopItems.TryGetValue(obj, out List<string> objShopItems))
                        {
                            objShopItems = new List<string>();
                            shopItems[obj] = objShopItems;
                        }

                        objShopItems.Add(pair.Key);
                        continue;
                    }

                    fsm = ShinyUtil.GetShiny(obj);
                }
                else if (loc is NewLocation newLoc)
                {
                    fsm = ShinyUtil.CreateNewShiny(newLoc.X, newLoc.Y);
                }
                else
                {
                    continue;
                }

                // Begin patching shiny item fsm from vanilla -> rando
                ShinyUtil.CancelFling(fsm);
                ShinyUtil.SetLocationId(fsm, locId);
                fsm.ForceTransitions("Charm?", "Trink Flash", "Store Key");

                // Replace giving the store key with giving our new item
                FsmState giveTrinket = fsm.GetState("Store Key");
                giveTrinket.RemoveActionsOfType<SetPlayerDataBool>();
                giveTrinket.AddFirstAction(new CollectItem(loc, item));

                // Set the sprite/text on the popup to make it obvious if the above fails
                giveTrinket.GetActionOfType<GetLanguageString>().convName = "IT BROKE";
                giveTrinket.GetActionOfType<SetSpriteRendererSprite>().sprite = Sprites.Get("NullTex");

                // Add dialogue box if necessary
                YNDialogue.AddToShiny(fsm, loc, item);

                RandomizerMod.Instance.Log("Patched item: " + locId + " -> " + itemId);
            }

            // Shop items
            foreach (KeyValuePair<GameObject, List<string>> pair in shopItems)
            {
                ShopModifier.SetShopItems(pair.Key, pair.Value.ToArray());
            }

            switch (scene)
            {
                case "Room_Final_Boss_Atrium":
                    GameObject.Find("Tut_tablet_top").LocateMyFSM("Inspection").GetState("Init").ClearTransitions();
                    break;
                case "Abyss_06_Core":
                    if (PlayerData.instance.healthBlue > 0 || PlayerData.instance.joniHealthBlue > 0 || GameManager.instance.entryGateName == "left1")
                    {
                        PlayerData.instance.blueVineDoor = true;
                        PlayMakerFSM BlueDoorFSM = GameObject.Find("Blue Door").LocateMyFSM("Control");
                        BlueDoorFSM.GetState("Init").RemoveTransitionsTo("Got Charm");
                    }

                    break;
                case "Abyss_15":
                    GameObject.Find("Dream Enter Abyss").LocateMyFSM("Control").GetState("Init").RemoveTransitionsTo("Idle");
                    GameObject.Find("Dream Enter Abyss").LocateMyFSM("Control").GetState("Init").AddTransition("FINISHED", "Inactive");
                    break;
                case "Crossroads_09":
                    if (GameObject.Find("Randomizer Shiny") is GameObject mawlekShard)
                    {
                        ShinyUtil.WaitForPDBool(mawlekShard, nameof(PlayerData.killedMawlek)).RunCoroutine();
                    }

                    break;
                case "Dream_NailCollection":
                    FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish")
                        .AddAction(new ExecuteLambda(() => GameManager.instance.ChangeToScene("RestingGrounds_07", "right1", 0f)));
                    break;
                case "Fungus1_04":
                    if (!Ref.PD.hornet1Defeated)
                    {
                        UObject.Destroy(FSMUtility.LocateFSM(GameObject.Find("Camera Locks Boss"), "FSM"));
                    }

                    break;
                case "Fungus2_21":
                    FSMUtility.LocateFSM(GameObject.Find("City Gate Control"), "Conversation Control")
                        .GetState("Activate").RemoveActionsOfType<SetPlayerDataBool>();

                    FsmState gateSlam = FSMUtility.LocateFSM(GameObject.Find("Ruins_gate_main"), "Open")
                        .GetState("Slam");
                    gateSlam.RemoveActionsOfType<SetPlayerDataBool>();
                    gateSlam.RemoveActionsOfType<CallMethodProper>();
                    gateSlam.RemoveActionsOfType<SendMessage>();
                    break;
                case "RestingGrounds_04":
                    // Patch dream nail plaque to look for randomized item
                    if (!RandomizerMod.Instance.ItemPlacements.Any(p => p.Value == "Dream_Nail"))
                    {
                        break;
                    }

                    foreach (FsmState state in
                        new[]
                        {
                            new[] { "Binding Shield Activate", "FSM", "Check" },
                            new[] { "Dreamer Plaque Inspect", "Conversation Control", "End" },
                            new[] { "Dreamer Scene 2", "Control", "Init" },
                            new[] { "PreDreamnail", "FSM", "Check" },
                            new[] { "PostDreamnail", "FSM", "Check" }
                        }.Select(s => GameObject.Find(s[0]).LocateMyFSM(s[1]).GetState(s[2])))
                    {
                        PlayerDataBoolTest test = state.GetActionOfType<PlayerDataBoolTest>();
                        FsmEvent isTrue = test.isTrue;
                        FsmEvent isFalse = test.isFalse;

                        state.RemoveActionsOfType<PlayerDataBoolTest>();
                        state.AddFirstAction(new ExecuteLambda(() =>
                        {
                            if (RandomizerMod.Instance.ObtainedLocations.Contains("Dream_Nail"))
                            {
                                state.Fsm.Event(isTrue);
                            }
                            else
                            {
                                state.Fsm.Event(isFalse);
                            }
                        }));
                    }

                    break;
                case "RestingGrounds_07":
                    GameObject.Find("Dream Moth").transform.Translate(new Vector3(-5f, 0f));

                    PlayMakerFSM moth = FSMUtility.LocateFSM(GameObject.Find("Dream Moth"), "Conversation Control");

                    PlayerData.instance.dreamReward1 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 1").Value = true;  //Relic
                    PlayerData.instance.dreamReward3 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 3").Value = true;  //Pale Ore
                    PlayerData.instance.dreamReward4 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 4").Value = true;  //Charm
                    PlayerData.instance.dreamReward5 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 5").Value = true;  //Vessel Fragment
                    PlayerData.instance.dreamReward6 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 6").Value = true;  //Relic
                    PlayerData.instance.dreamReward7 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 7").Value = true;  //Mask Shard
                    PlayerData.instance.dreamReward8 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 8").Value = true;  //Skill
                    break;
                case "Room_Sly_Storeroom":
                    FsmState slyFinish = FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish");
                    slyFinish.AddAction(new ExecuteLambda(() => PlayerData.instance.gotSlyCharm = true));
                    slyFinish.AddAction(new ExecuteLambda(() => GameManager.instance.ChangeToScene("Town", "door_sly", 0f)));
                    break;
                case "Ruins1_24":
                    if (GameObject.Find("Randomizer Shiny") is GameObject desolateDive)
                    {
                        ShinyUtil.WaitForPDBool(desolateDive, nameof(PlayerData.killedMageLord)).RunCoroutine();
                    }

                    if (PlayerData.instance.killedMageLord)
                    {
                        UObject.Destroy(GameObject.Find("Battle Gate (1)"));
                    }

                    break;
                case "Room_Colosseum_01":
                    PlayerData.instance.colosseumBronzeOpened = true;
                    PlayerData.instance.colosseumSilverOpened = true;
                    PlayerData.instance.colosseumGoldOpened = true;
                    GameObject.Find("Silver Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").ClearTransitions();
                    GameObject.Find("Silver Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").AddTransition("FINISHED", "Box Up YN");
                    GameObject.Find("Gold Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").ClearTransitions();
                    GameObject.Find("Gold Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").AddTransition("FINISHED", "Box Up YN");
                    break;
                case "Room_Mansion":
                    if (PlayerData.instance.xunFlowerGiven)
                    {
                        PlayerData.instance.xunRewardGiven = true;
                    }

                    break;
                case "Room_Colosseum_Bronze":
                    GameObject.Find("Colosseum Manager").LocateMyFSM("Geo Pool").GetState("Open Gates").AddFirstAction(new ExecuteLambda(() => PlayerData.instance.colosseumBronzeCompleted = true));
                    break;
                case "Room_Colosseum_Silver":
                    GameObject.Find("Colosseum Manager").LocateMyFSM("Geo Pool").GetState("Open Gates").AddFirstAction(new ExecuteLambda(() => PlayerData.instance.colosseumSilverCompleted = true));
                    break;
                case "Town":
                    UObject.Destroy(GameObject.Find("Set Sly Basement Closed"));
                    break;
            }
        }
    }
}
