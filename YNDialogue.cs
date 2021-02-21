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

namespace Randomizer
{
    public class YNDialogue
    {
        public static void AddToShiny(PlayMakerFSM fsm, Location loc, Item item)
        {
            if (loc.RequiredBools.Length == 0 && loc.RequiredInts.Length == 0 && loc.RequiredCallbacks.Length == 0)
            {
                return;
            }

            AddToShiny(fsm, GetDialogueString(loc, item), loc.HasRequired);
        }

        public static void AddToShiny(PlayMakerFSM fsm, string text, Func<bool> canBuy)
        {
            FsmState noState = new FsmState(fsm.GetState("Idle"))
            {
                Name = "YN No"
            };

            noState.ClearTransitions();
            noState.RemoveActionsOfType<FsmStateAction>();

            noState.AddTransition("FINISHED", "Give Control");

            Tk2dPlayAnimationWithEvents heroUp = new Tk2dPlayAnimationWithEvents
            {
                gameObject = new FsmOwnerDefault
                {
                    OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                    GameObject = HeroController.instance.gameObject
                },
                clipName = "Collect Normal 3",
                animationTriggerEvent = null,
                animationCompleteEvent = FsmEvent.GetFsmEvent("FINISHED")
            };

            noState.AddAction(new ExecuteLambda(CloseYNDialogue));
            noState.AddAction(heroUp);

            FsmState giveControl = new FsmState(fsm.GetState("Idle"))
            {
                Name = "Give Control"
            };

            giveControl.ClearTransitions();
            giveControl.RemoveActionsOfType<FsmStateAction>();

            giveControl.AddTransition("FINISHED", "Idle");

            giveControl.AddAction(new ExecuteLambda(() => PlayMakerFSM.BroadcastEvent("END INSPECT")));

            fsm.AddState(noState);
            fsm.AddState(giveControl);

            FsmState charm = fsm.GetState("Charm?");
            string yesState = charm.Transitions[0].ToState;
            charm.ClearTransitions();

            charm.AddTransition("HERO DAMAGED", noState.Name);
            charm.AddTransition("NO", noState.Name);
            charm.AddTransition("YES", yesState);

            fsm.GetState(yesState).AddAction(new ExecuteLambda(CloseYNDialogue));

            charm.AddFirstAction(new ExecuteLambda(() => OpenYNDialogue(fsm.gameObject, text, canBuy)));
        }

        private static void OpenYNDialogue(GameObject shiny, string text, Func<bool> canBuy)
        {
            FSMUtility.LocateFSM(GameObject.Find("DialogueManager"), "Box Open YN").SendEvent("BOX UP YN");
            FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control").FsmVariables
                .GetFsmGameObject("Requester").Value = shiny;

            Lang.Add("UI", "RANDOMIZER_YN_DIALOGUE", text);
            if (!canBuy())
            {
                KillGeoText().RunCoroutine();
            }

            FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control").FsmVariables
                .GetFsmInt("Toll Cost").Value = 0;
            FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control").FsmVariables
                .GetFsmGameObject("Geo Text").Value.SetActive(true);

            GameObject.Find("Text YN").GetComponent<DialogueBox>().StartConversation("RANDOMIZER_YN_DIALOGUE", "UI");
        }

        private static void CloseYNDialogue()
        {
            FSMUtility.LocateFSM(GameObject.Find("DialogueManager"), "Box Open YN").SendEvent("BOX DOWN YN");
        }

        private static IEnumerator KillGeoText()
        {
            PlayMakerFSM ynFsm = FSMUtility.LocateFSM(GameObject.Find("Text YN"), "Dialogue Page Control");
            while (ynFsm.ActiveStateName != "Ready for Input")
            {
                yield return new WaitForEndOfFrame();
            }

            ynFsm.FsmVariables.GetFsmGameObject("Geo Text").Value.SetActive(false);
            ynFsm.FsmVariables.GetFsmInt("Toll Cost").Value = int.MaxValue;
            PlayMakerFSM.BroadcastEvent("NOT ENOUGH");
        }

        private static string GetDialogueString(Location loc, Item item)
        {
            string itemName = Lang.Get(item.Stages[0].Popup.Name, "UI");
            if (item.Stages.Length > 1)
            {
                itemName = "Additive " + itemName;
            }

            List<string> reqText = new List<string>();
            foreach (PlayerField<bool> pf in loc.RequiredBools)
            {
                string t = pf.Value
                    ? "Have "
                    : "Don't have ";

                ItemStage reqItem = RandoResources.Items
                    .SelectMany(i => i.Stages)
                    .FirstOrDefault(s => s.BoolActions.Any(b => b.FieldName == pf.FieldName && b.Value == pf.Value));

                t += reqItem == null
                    ? pf.FieldName
                    : Lang.Get(reqItem.Popup.Name, "UI");

                reqText.Add(t);
            }

            foreach (PlayerField<int> pf in loc.RequiredInts)
            {
                string t = pf.Value.ToString();
                switch (pf.FieldName)
                {
                    case nameof(PlayerData.geo):
                        t += " Geo";
                        break;
                    case nameof(PlayerData.dreamOrbs):
                        t += " Essence";
                        break;
                    case nameof(PlayerData.grubsCollected):
                        t += " Grubs";
                        break;
                    case nameof(PlayerData.simpleKeys):
                        if (pf.Value == 1)
                        {
                            t = "Simple Key";
                        }
                        else
                        {
                            t += " Simple Keys";
                        }

                        break;
                    default:
                        ItemStage reqItem = RandoResources.Items
                            .SelectMany(i => i.Stages)
                            .FirstOrDefault(s => s.IntActions.Any(i => i.FieldName == pf.FieldName && i.Value == pf.Value));

                        t = "Have " + (reqItem == null
                            ? pf.Value + " " + pf.FieldName
                            : Lang.Get(reqItem.Popup.Name, "UI"));

                        break;
                }

                reqText.Add(t);
            }

            MethodInfo[] reqCalls = loc.RequiredCallbacks
                .Select(s => Type.GetType("Randomizer." + s.Substring(0, s.IndexOf('.'))).GetMethod(s.Substring(s.IndexOf('.') + 1)))
                .ToArray();

            foreach (MethodInfo call in reqCalls)
            {
                reqText.Add(call
                    .GetCustomAttributes(typeof(NameAttribute), false)
                    .Cast<NameAttribute>()
                    .FirstOrDefault()?.Name
                    ?? call.Name);
            }

            return string.Join(", ", reqText.ToArray()) + ": " + itemName;
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class NameAttribute : Attribute
        {
            public readonly string Name;

            public NameAttribute(string name)
                => Name = name;
        }
    }
}
