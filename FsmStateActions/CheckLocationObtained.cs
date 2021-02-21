using HutongGames.PlayMaker;

namespace Randomizer.FsmStateActions
{
    internal class CheckLocationObtained : FsmStateAction
    {
        private readonly string LocId;
        private readonly string TrueEvent;
        private readonly string FalseEvent;

        public CheckLocationObtained(string locId, string trueEvent = null, string falseEvent = null)
        {
            LocId = locId;
            TrueEvent = trueEvent;
            FalseEvent = falseEvent;
        }

        public override void OnEnter()
        {
            if (RandomizerMod.Instance.ObtainedLocations.Contains(LocId))
            {
                if (TrueEvent != null)
                {
                    Fsm.Event(TrueEvent);
                }
            }
            else if (FalseEvent != null)
            {
                Fsm.Event(FalseEvent);
            }

            Finish();
        }
    }
}