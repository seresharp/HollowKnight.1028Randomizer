using System.Linq;
using HutongGames.PlayMaker.Actions;
using Randomizer.Serialized;
using Randomizer.Util;
using UnityEngine;

using UObject = UnityEngine.Object;

namespace Randomizer
{
    public static class LocationCallbacks
    {
        public static void PatchStagStation(Location location)
        {
            foreach (GameObject go in UObject.FindObjectsOfType<GameObject>())
            {
                if (go.name.Contains("Station Bell"))
                {
                    go.LocateMyFSM("Stag Bell").GetState("Init").RemoveActionsOfType<PlayerDataBoolTest>();
                    go.LocateMyFSM("Stag Bell").GetState("Init").AddTransition("FINISHED", "Opened");
                }
                else if (go.name.Contains("Stag") && go.LocateMyFSM("Stag Control") is PlayMakerFSM fsm)
                {
                    fsm.GetState("Open Grate").RemoveActionsOfType<SetPlayerDataBool>();
                    fsm.GetState("Open Grate").RemoveActionsOfType<SetBoolValue>();
                    if (!PlayerData.instance.GetBool(fsm.FsmVariables.StringVariables.First(v => v.Name == ("Station Opened Bool")).Value))
                    {
                        fsm.FsmVariables.IntVariables.First(v => v.Name == "Station Position Number").Value = 0;
                        fsm.GetState("Current Location Check").RemoveActionsOfType<IntCompare>();
                    }
                }
            }
        }

        [YNDialogue.Name("Complete the root")]
        public static bool CheckWhisperingRoot(Location location)
            => PlayerData.instance.scenesEncounteredDreamPlantC.Contains(location.Scene);
    }
}
