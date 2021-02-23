using System;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using Randomizer.FsmStateActions;
using Randomizer.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Randomizer
{
    public static class QoL
    {
        static QoL()
        {
            On.DialogueBox.ShowNextChar += FastText;
            On.HeroController.Start += PatchHero;
        }

        public static void PatchScene(string sceneName)
        {
            Scene scene = USceneManager.GetSceneByName(sceneName);

            switch (scene.name)
            {
                case "Crossroads_11_alt":
                case "Crossroads_ShamanTemple":
                case "Fungus1_28":
                    new[] { "Blocker", "Blocker 1", "Blocker 2" }
                        .Select(str => scene.FindGameObject(str))
                        .Where(obj => obj != null)
                        .ToList()
                        .ForEach(PatchBlocker);
                    break;
                case "Crossroads_38":
                    PatchGrubFather(scene.FindGameObject("Grub King").LocateMyFSM("King Control"));
                    break;
                case "Ruins1_05b":
                    PatchLemm(scene.FindGameObject("Relic Dealer"));
                    break;
            }
        }

        private static void PatchBlocker(GameObject obj)
        {
            obj.LocateMyFSM("health_manager_enemy").ForceTransitions("Decrement Health", "Pause");
            obj.LocateMyFSM("Blocker Control").GetState("Can Roller?").RemoveActionsOfType<IntCompare>();
        }

        private static void PatchGrubFather(PlayMakerFSM fsm)
        {
            fsm.ForceTransitions("Final Reward?", "Recheck");
        }

        private static void PatchLemm(GameObject lemm)
        {
            lemm.LocateMyFSM("npc_control")
                .GetState("Convo End")
                .AddAction(new ExecuteLambda(SellRelics));

            static void SellRelics()
            {
                PlayerData pd = PlayerData.instance;

                // Defender's crest
                if (pd.equippedCharm_10)
                {
                    return;
                }

                for (int i = 1; i <= 4; i++)
                {
                    int amount = pd.GetInt($"trinket{i}");

                    if (amount <= 0)
                    {
                        continue;
                    }

                    int price = amount * i switch
                    {
                        1 => 200,
                        2 => 450,
                        3 => 800,
                        4 => 1200,
                        _ => 0
                    };

                    pd.SetInt($"soldTrinket{i}", pd.GetInt($"soldTrinket{i}") + amount);

                    HeroController.instance.AddGeo(price);

                    pd.SetInt($"trinket{i}", 0);
                }
            }
        }

        private static void FastText(On.DialogueBox.orig_ShowNextChar orig, DialogueBox self)
        {
            self.GetField<DialogueBox, TMPro.TextMeshPro>("textMesh").maxVisibleCharacters = 9999;
        }

        private static void PatchHero(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            try
            {
                PatchWaterFSM(self.gameObject.LocateMyFSM("Surface Water"));
            }
            catch (Exception e)
            {
                RandomizerMod.Instance.Log("Error patching hero\n" + e);
            }
        }

        private static void PatchWaterFSM(PlayMakerFSM fsm)
        {
            fsm.GetState("Enter").AddAction(new SendEvent
            {
                eventTarget = FsmEventTarget.Self,
                sendEvent = FsmEvent.FindEvent("FINISHED"),
                delay = 0,
                everyFrame = false
            });
        }
    }
}
