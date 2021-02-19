using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Randomizer.FsmStateActions;
using Randomizer.Serialized;
using Randomizer.Util;
using UnityEngine;

using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using PD = Randomizer.Patches.PlayerData;

namespace Randomizer
{
    public static class SceneChanges
    {
        public static void PatchScene(string scene)
        {
            Dictionary<GameObject, List<string>> shopItems = new Dictionary<GameObject, List<string>>();

            foreach (KeyValuePair<string, string> pair in PD.instance.itemPlacements)
            {
                string itemId = pair.Key.Substring(0, pair.Key.IndexOf('.'));
                string locId = pair.Value;

                Item item = RandoResources.Items.First(i => i.Id == itemId);
                Location loc = RandoResources.Locations.FirstOrDefault(l => l.Id == locId)
                    ?? RandoResources.Shops.First(s => s.Id == locId);

                if (loc.Scene != scene)
                {
                    continue;
                }

                Debug.Log("[Randomizer] Patching item: " + locId + " -> " + itemId);

                foreach (string objName in loc.DestroyObjects)
                {
                    IEnumerator DeleteObject()
                    {
                        string oldScene = scene;
                        while (GameManager.instance.sceneName == oldScene)
                        {
                            yield return null;
                            GameObject obj = GameObject.Find(objName);
                            if (obj != null)
                            {
                                UObject.Destroy(obj);
                                break;
                            }
                        }
                    }

                    GameManager.instance.StartCoroutine(DeleteObject());
                }

                foreach (string callback in loc.RandoCallbacks)
                {
                    int dot = callback.IndexOf('.');
                    Type t = Type.GetType(nameof(Randomizer) + "." + callback.Substring(0, dot));
                    MethodInfo method = t.GetMethod(callback.Substring(dot + 1));

                    method.Invoke(null, new[] { loc });
                }

                GameObject obj;
                PlayMakerFSM fsm;
                if (loc is ObjectLocation objLoc)
                {
                    obj = USceneManager.GetSceneByName(scene).FindGameObject(objLoc.MainObject);
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

                    fsm = obj.LocateMyFSM("Shiny Control");
                    if (fsm == null)
                    {
                        GameObject shiny = ObjectCache.Shiny;
                        shiny.name = "Randomizer Shiny";
                        if (obj.transform.parent != null)
                        {
                            shiny.transform.SetParent(obj.transform.parent);
                        }

                        shiny.transform.position = obj.transform.position;
                        shiny.transform.localPosition = obj.transform.localPosition;
                        shiny.SetActive(obj.activeSelf);

                        UObject.Destroy(obj);

                        obj = shiny;
                        fsm = shiny.LocateMyFSM("Shiny Control");
                    }
                }
                else if (loc is NewLocation newLoc)
                {
                    obj = ObjectCache.Shiny;
                    obj.name = "Randomizer Shiny";
                    obj.transform.SetPosition2D(newLoc.X, newLoc.Y);
                    obj.SetActive(true);

                    fsm = obj.LocateMyFSM("Shiny Control");
                }
                else
                {
                    continue;
                }

                FsmState fling = fsm.GetState("Fling?");
                fling.ClearTransitions();
                fling.AddTransition("FINISHED", "Fling R");
                FlingObject flingObj = fsm.GetState("Fling R").GetActionsOfType<FlingObject>()[0];
                flingObj.angleMin = flingObj.angleMax = 270;
                flingObj.speedMin = flingObj.speedMax = 0.1f;

                // COPY/PASTED ChangeShinyIntoItem
                FsmState pdBool = fsm.GetState("PD Bool?");
                FsmState charm = fsm.GetState("Charm?");
                FsmState trinkFlash = fsm.GetState("Trink Flash");
                FsmState giveTrinket = fsm.GetState("Store Key"); // This path works well for our changes

                // Remove actions that stop shiny from spawning
                pdBool.RemoveActionsOfType<PlayerDataBoolTest>();
                pdBool.RemoveActionsOfType<StringCompare>();

                // Add our own check to stop the shiny from being grabbed twice
                pdBool.AddAction(new RandomizerExecuteLambda(() =>
                {
                    if (PD.instance.obtainedLocations.Contains(locId))
                    {
                        fsm.SendEvent("COLLECTED");
                    }
                }));

                // Force the FSM to follow the path for the correct trinket
                charm.ClearTransitions();
                charm.AddTransition("FINISHED", "Trink Flash");
                trinkFlash.ClearTransitions();
                fsm.GetState("Trinket Type").ClearTransitions();
                trinkFlash.AddTransition("FINISHED", "Store Key");

                giveTrinket.RemoveActionsOfType<SetPlayerDataBool>();
                giveTrinket.AddFirstAction(new RandomizerExecuteLambda(() =>
                {
                    PD.instance.obtainedLocations.Add(locId);

                    IEnumerator SendEvents(string[] events)
                    {
                        foreach (string e in events)
                        {
                            PlayMakerFSM.BroadcastEvent(e);
                            yield return null;
                        }
                    }

                    GameManager.instance.StartCoroutine(SendEvents(loc.FsmEvents));

                    // Get next stage
                    if (!item.TryCollect(out ItemStage stage))
                    {
                        return;
                    }

                    // Set icon/name on fsm
                    giveTrinket.GetActionsOfType<GetLanguageString>().First().convName = stage.Popup.Name;
                    giveTrinket.GetActionsOfType<SetSpriteRendererSprite>().First().sprite = Sprites.Get(stage.Popup.IsBig ? stage.Shop.Sprite : stage.Popup.Sprite);
                }));

                giveTrinket.GetActionsOfType<GetLanguageString>().First().convName = "IT BROKE";
                giveTrinket.GetActionsOfType<SetSpriteRendererSprite>().First().sprite = Sprites.Get("NullTex");

                if (loc.RequiredBools.Length > 0 || loc.RequiredInts.Length > 0 || loc.RequiredCallbacks.Length > 0)
                {
                    MethodInfo[] reqCalls = loc.RequiredCallbacks
                        .Select(s => Type.GetType("Randomizer." + s.Substring(0, s.IndexOf('.'))).GetMethod(s.Substring(s.IndexOf('.') + 1)))
                        .ToArray();

                    string[] reqText = loc.RequiredBools.Select(b => b.FieldName + " = " + b.Value)
                        .Concat(loc.RequiredInts.Select(i => i.FieldName + " >= " + i.Value))
                        .Concat(reqCalls.Select(c => c.Name))
                        .ToArray();

                    YNDialogue.AddToShiny(fsm, item.Id + ": " + string.Join(", ", reqText), () =>
                    {
                        foreach (PlayerField<bool> pf in loc.RequiredBools)
                        {
                            if (!pf.CheckValue())
                            {
                                return false;
                            }
                        }

                        foreach (PlayerField<int> pf in loc.RequiredInts)
                        {
                            if (!pf.CheckValue(true))
                            {
                                return false;
                            }
                        }

                        foreach (MethodInfo call in reqCalls)
                        {
                            if (!(bool)call.Invoke(null, new[] { loc }))
                            {
                                return false;
                            }
                        }

                        return true;
                    });
                }

                Debug.Log("[Randomizer] Patched item: " + locId + " -> " + itemId);
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
                    if (PD.instance.healthBlue > 0 || PD.instance.joniHealthBlue > 0 || GameManager.instance.entryGateName == "left1")
                    {
                        PD.instance.blueVineDoor = true;
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
                        mawlekShard.transform.SetPositionY(100f);
                        mawlekShard.SetActive(false);
                        IEnumerator MawlekDead()
                        {
                            yield return new WaitUntil(() => PD.instance.killedMawlek);
                            mawlekShard.transform.SetPositionY(10f);
                            mawlekShard.transform.SetPositionX(61.5f);
                            mawlekShard.SetActive(true);
                        }

                        new GameObject().AddComponent<NonBouncer>().StartCoroutine(MawlekDead());
                    }

                    break;
                case "Dream_NailCollection":
                    FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish")
                        .AddAction(new RandomizerExecuteLambda(() => GameManager.instance.ChangeToScene("RestingGrounds_07", "right1", 0f)));
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
                    if (!PD.instance.itemPlacements.Any(p => p.Value == "Dream_Nail"))
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
                        state.AddFirstAction(new RandomizerExecuteLambda(() =>
                        {
                            if (PD.instance.obtainedLocations.Contains("Dream_Nail"))
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

                    PD.instance.dreamReward1 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 1").Value = true;  //Relic
                    PD.instance.dreamReward3 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 3").Value = true;  //Pale Ore
                    PD.instance.dreamReward4 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 4").Value = true;  //Charm
                    PD.instance.dreamReward5 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 5").Value = true;  //Vessel Fragment
                    PD.instance.dreamReward6 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 6").Value = true;  //Relic
                    PD.instance.dreamReward7 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 7").Value = true;  //Mask Shard
                    PD.instance.dreamReward8 = true;
                    moth.FsmVariables.GetFsmBool("Got Reward 8").Value = true;  //Skill
                    break;
                case "Room_Sly_Storeroom":
                    FsmState slyFinish = FSMUtility.LocateFSM(GameObject.Find("Randomizer Shiny"), "Shiny Control").GetState("Finish");
                    slyFinish.AddAction(new RandomizerExecuteLambda(() => PD.instance.gotSlyCharm = true));
                    slyFinish.AddAction(new RandomizerExecuteLambda(() => GameManager.instance.ChangeToScene("Town", "door_sly", 0f)));
                    break;
                case "Ruins1_24":
                    if (GameObject.Find("Randomizer Shiny") is GameObject desolateDive)
                    {
                        desolateDive.transform.SetPositionY(100f);
                        desolateDive.SetActive(false);

                        IEnumerator SoulMasterDead()
                        {
                            yield return new WaitUntil(() => PD.instance.killedMageLord);
                            desolateDive.transform.SetPositionY(24.6f);
                            desolateDive.transform.SetPositionX(13.5f);
                            desolateDive.SetActive(true);
                        }

                        new GameObject().AddComponent<NonBouncer>().StartCoroutine(SoulMasterDead());
                    }

                    break;
                case "Room_Colosseum_01":
                    PD.instance.colosseumBronzeOpened = true;
                    PD.instance.colosseumSilverOpened = true;
                    PD.instance.colosseumGoldOpened = true;
                    GameObject.Find("Silver Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").ClearTransitions();
                    GameObject.Find("Silver Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").AddTransition("FINISHED", "Box Up YN");
                    GameObject.Find("Gold Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").ClearTransitions();
                    GameObject.Find("Gold Trial Board").LocateMyFSM("Conversation Control").GetState("Hero Anim").AddTransition("FINISHED", "Box Up YN");
                    break;
                case "Room_Mansion":
                    if (PD.instance.xunFlowerGiven)
                    {
                        PD.instance.xunRewardGiven = true;
                    }

                    break;
                case "Room_Colosseum_Bronze":
                    GameObject.Find("Colosseum Manager").LocateMyFSM("Geo Pool").GetState("Open Gates").AddFirstAction(new RandomizerExecuteLambda(() => PD.instance.colosseumBronzeCompleted = true));
                    break;
                case "Room_Colosseum_Silver":
                    GameObject.Find("Colosseum Manager").LocateMyFSM("Geo Pool").GetState("Open Gates").AddFirstAction(new RandomizerExecuteLambda(() => PD.instance.colosseumSilverCompleted = true));
                    break;
                case "Town":
                    UObject.Destroy(GameObject.Find("Set Sly Basement Closed"));
                    break;
            }
        }
    }
}
