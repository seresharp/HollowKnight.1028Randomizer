using MonoMod;

#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

namespace Randomizer.Patches
{
    [MonoModPatch("HutongGames.PlayMaker.Actions.FlingObject")]
    public class FlingObject : HutongGames.PlayMaker.Actions.FlingObject
    {
        public extern void orig_OnEnter();

        public override void OnEnter()
        {
            try
            {
                orig_OnEnter();
            }
            catch
            {
                Finish();
            }
        }
    }
}
