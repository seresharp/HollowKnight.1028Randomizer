using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Randomizer.Util;

using PD = Randomizer.Patches.PlayerData;

namespace Randomizer.Serialized
{
    public class Item : IReproduceable
    {
        public readonly string Id;
        public readonly ItemStage[] Stages;

        public Item(string id, ItemStage[] stages)
        {
            Id = id;
            Stages = stages;
        }

        public bool TryGetNextStage(out ItemStage stage)
        {
            if (!PD.instance.itemObtainedCounts.TryGetValue(Id, out int itemCount))
            {
                itemCount = 0;
            }

            int stageIdx = 0;
            int count = 0;
            for (int i = 0; i < itemCount; i++)
            {
                count++;
                if (count >= Stages[stageIdx].Repeat)
                {
                    stageIdx++;
                    if (stageIdx >= Stages.Length)
                    {
                        stage = null;
                        return false;
                    }
                }
            }

            stage = Stages[stageIdx];
            return true;
        }

        public bool TryCollect(out ItemStage givenStage)
        {
            if (!TryGetNextStage(out givenStage))
            {
                return false;
            }

            // Execute stage actions
            foreach (PlayerField<bool> pf in givenStage.BoolActions)
            {
                pf.ApplyValue();
            }

            foreach (PlayerField<int> pf in givenStage.IntActions)
            {
                pf.ApplyValue();
            }

            // Increment obtained counter
            if (!PD.instance.itemObtainedCounts.TryGetValue(Id, out int itemCount))
            {
                itemCount = 0;
            }

            PD.instance.itemObtainedCounts[Id] = itemCount + 1;

            // Item fsm events
            static IEnumerator SendEvents(string[] events)
            {
                foreach (string e in events)
                {
                    PlayMakerFSM.BroadcastEvent(e);
                    yield return null;
                }
            }

            SendEvents(givenStage.FsmEvents).RunCoroutine();

            // Code callbacks
            foreach (string callback in givenStage.RandoCallbacks)
            {
                int dot = callback.IndexOf('.');
                Type t = Type.GetType(nameof(Randomizer) + "." + callback.Substring(0, dot));
                MethodInfo method = t.GetMethod(callback.Substring(dot + 1));

                method.Invoke(null, new[] { givenStage });
            }

            return true;
        }

        public string Repr()
        {
            StringBuilder repr = new StringBuilder();
            repr.Append("new ");
            repr.Append(nameof(Item));
            repr.Append("(");
            repr.Append(Id.Repr());
            repr.Append(", ");
            repr.Append(Stages.Repr());
            repr.Append(")");

            return repr.ToString();
        }
    }

    public class ItemStage : IReproduceable
    {
        public readonly PlayerField<bool>[] BoolActions;
        public readonly PlayerField<int>[] IntActions;
        public readonly string[] FsmEvents;
        public readonly string[] RandoCallbacks;
        public readonly PopupInfo Popup;
        public readonly ShopInfo Shop;
        public readonly int Repeat;

        public ItemStage(PlayerField<bool>[] boolActions, PlayerField<int>[] intActions, string[] fsmEvents,
            string[] randoCallbacks, PopupInfo popup, ShopInfo shop, int repeat)
        {
            BoolActions = boolActions;
            IntActions = intActions;
            FsmEvents = fsmEvents;
            RandoCallbacks = randoCallbacks;
            Popup = popup;
            Shop = shop;
            Repeat = repeat;
        }

        public string Repr()
        {
            StringBuilder repr = new StringBuilder();
            repr.Append("new ");
            repr.Append(nameof(ItemStage));
            repr.Append("(");
            repr.Append(BoolActions.Repr());
            repr.Append(", ");
            repr.Append(IntActions.Repr());
            repr.Append(", ");
            repr.Append(FsmEvents.Repr());
            repr.Append(", ");
            repr.Append(RandoCallbacks.Repr());
            repr.Append(", ");
            repr.Append(Popup.Repr());
            repr.Append(", ");
            repr.Append(Shop.Repr());
            repr.Append(", ");
            repr.Append(Repeat);
            repr.Append(")");

            return repr.ToString();
        }
    }

    public class PopupInfo : IReproduceable
    {
        public bool IsBig => !string.IsNullOrEmpty(Take) || !string.IsNullOrEmpty(Button)
            || !string.IsNullOrEmpty(Description1) || !string.IsNullOrEmpty(Description2);

        public readonly string Name;
        public readonly string Sprite;
        public readonly string Take;
        public readonly string Button;
        public readonly string Description1;
        public readonly string Description2;

        public PopupInfo(string name, string sprite, string take, string button, string desc1, string desc2)
        {
            Name = name;
            Sprite = sprite;
            Take = take;
            Button = button;
            Description1 = desc1;
            Description2 = desc2;
        }

        public string Repr()
        {
            StringBuilder repr = new StringBuilder();
            repr.Append("new ");
            repr.Append(nameof(PopupInfo));
            repr.Append("(");
            repr.Append(Name.Repr());
            repr.Append(", ");
            repr.Append(Sprite.Repr());
            repr.Append(", ");
            repr.Append(Take.Repr());
            repr.Append(", ");
            repr.Append(Button.Repr());
            repr.Append(", ");
            repr.Append(Description1.Repr());
            repr.Append(", ");
            repr.Append(Description2.Repr());
            repr.Append(")");

            return repr.ToString();
        }
    }

    public class ShopInfo : IReproduceable
    {
        public readonly string Name;
        public readonly string Sprite;
        public readonly string Description;

        public ShopInfo(string name, string sprite, string desc)
        {
            Name = name;
            Sprite = sprite;
            Description = desc;
        }

        public string Repr()
        {
            StringBuilder repr = new StringBuilder();
            repr.Append("new ");
            repr.Append(nameof(ShopInfo));
            repr.Append("(");
            repr.Append(Name.Repr());
            repr.Append(", ");
            repr.Append(Sprite.Repr());
            repr.Append(", ");
            repr.Append(Description.Repr());
            repr.Append(")");

            return repr.ToString();
        }
    }
}
