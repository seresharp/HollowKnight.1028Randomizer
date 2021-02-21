using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Randomizer.Serialized;
using Randomizer.Util;

namespace Randomizer.FsmStateActions
{
    internal class CollectItem : FsmStateAction
    {
        private readonly Location Loc;
        private readonly Item Item;

        public CollectItem(Location loc, Item item)
        {
            Loc = loc;
            Item = item;
        }

        public override void OnEnter()
        {
            Loc.Collect();

            // Attempt to give the player the next stage of the item
            if (!Item.TryCollect(out ItemStage stage))
            {
                Finish();
                return;
            }

            // Set icon/name on popup
            GetLanguageString text = State.GetActionOfType<GetLanguageString>();
            SetSpriteRendererSprite sprite = State.GetActionOfType<SetSpriteRendererSprite>();
            if (text != null)
            {
                text.convName = stage.Popup.Name;
            }

            if (sprite != null)
            {
                sprite.sprite = Sprites.Get(stage.Popup.IsBig ? stage.Shop.Sprite : stage.Popup.Sprite);
            }

            Finish();
        }
    }
}