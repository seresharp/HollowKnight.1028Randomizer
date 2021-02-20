using System.Collections;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Randomizer.FsmStateActions;
using UnityEngine;

using UObject = UnityEngine.Object;

namespace Randomizer.Util
{
    public static class ShinyUtil
    {
        public static PlayMakerFSM GetShiny(GameObject obj)
        {
            PlayMakerFSM fsm = obj.LocateMyFSM("Shiny Control");
            if (fsm == null || fsm.FsmStates.Length != ObjectCache.ShinyStateCount)
            {
                fsm = ReplaceWithShiny(obj);
            }

            return fsm;
        }

        public static PlayMakerFSM ReplaceWithShiny(GameObject obj)
        {
            GameObject shiny = ObjectCache.Shiny;
            if (obj.transform.parent != null)
            {
                shiny.transform.SetParent(obj.transform.parent);
            }

            shiny.transform.position = obj.transform.position;
            shiny.transform.localPosition = obj.transform.localPosition;
            shiny.SetActive(obj.activeSelf);

            UObject.Destroy(obj);

            return shiny.LocateMyFSM("Shiny Control");
        }

        public static PlayMakerFSM CreateNewShiny(float x, float y)
        {
            GameObject obj = ObjectCache.Shiny;
            obj.transform.SetPosition2D(x, y);
            obj.SetActive(true);

            return obj.LocateMyFSM("Shiny Control");
        }

        public static void CancelFling(PlayMakerFSM fsm)
        {
            FsmState fling = fsm.GetState("Fling?");
            fling.ClearTransitions();
            fling.AddTransition("FINISHED", "Fling R");
            FlingObject flingObj = fsm.GetState("Fling R").GetActionsOfType<FlingObject>()[0];
            flingObj.angleMin = flingObj.angleMax = 270;
            flingObj.speedMin = flingObj.speedMax = 0.1f;
        }

        public static void SetLocationId(PlayMakerFSM fsm, string locId)
        {
            // Remove vanilla save checks
            fsm.GetState("PD Bool?").RemoveActionsOfType<PlayerDataBoolTest>();
            fsm.GetState("Init").RemoveActionsOfType<BoolTest>();
            fsm.GetState("Finish").RemoveActionsOfType<SetBoolValue>();

            // Add our own check to stop the shiny from being grabbed twice
            fsm.GetState("PD Bool?").AddFirstAction(new CheckLocationObtained(locId, "COLLECTED"));
        }

        public static IEnumerator WaitForPDBool(GameObject shiny, string pdBool)
        {
            Vector3 pos = shiny.transform.position;
            shiny.transform.SetPosition2D(-3000, -3000);
            shiny.SetActive(false);
            
            while (!PlayerData.instance.GetBool(pdBool))
            {
                yield return null;
            }

            shiny.transform.position = pos;
            shiny.SetActive(true);
        }
    }
}
