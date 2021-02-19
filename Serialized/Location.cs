using System.Text;

namespace Randomizer.Serialized
{
    public abstract class Location
    {
        public readonly string Id;
        public readonly string Scene;
        public readonly string[] DestroyObjects;
        public readonly string[] FsmEvents;
        public readonly string[] RandoCallbacks;
        public readonly PlayerField<int>[] RequiredInts;
        public readonly PlayerField<bool>[] RequiredBools;
        public readonly string[] RequiredCallbacks;

        public Location(string id, string scene, string[] destroyObjects, string[] fsmEvents, string[] randoCallbacks,
            PlayerField<int>[] requiredInts, PlayerField<bool>[] requiredBools, string[] requiredCallbacks)
        {
            Id = id;
            Scene = scene;
            DestroyObjects = destroyObjects;
            FsmEvents = fsmEvents;
            RandoCallbacks = randoCallbacks;
            RequiredInts = requiredInts;
            RequiredBools = requiredBools;
            RequiredCallbacks = requiredCallbacks;
        }
    }

    public class ObjectLocation : Location, IReproduceable
    {
        public readonly string MainObject;

        public ObjectLocation(string id, string scene, string[] destroyObjects,
            string[] fsmEvents, string[] randoCallbacks, PlayerField<int>[] requiredInts,
            PlayerField<bool>[] requiredBools, string[] requiredCallbacks, string mainObject)
            : base(id, scene, destroyObjects, fsmEvents, randoCallbacks, requiredInts, requiredBools, requiredCallbacks)
        {
            MainObject = mainObject;
        }

        public string Repr()
        {
            StringBuilder repr = new StringBuilder();
            repr.Append("new ");
            repr.Append(nameof(ObjectLocation));
            repr.Append("(");
            repr.Append(Id.Repr());
            repr.Append(", ");
            repr.Append(Scene.Repr());
            repr.Append(", ");
            repr.Append(DestroyObjects.Repr());
            repr.Append(", ");
            repr.Append(FsmEvents.Repr());
            repr.Append(", ");
            repr.Append(RandoCallbacks.Repr());
            repr.Append(", ");
            repr.Append(RequiredInts.Repr());
            repr.Append(", ");
            repr.Append(RequiredBools.Repr());
            repr.Append(", ");
            repr.Append(RequiredCallbacks.Repr());
            repr.Append(", ");
            repr.Append(MainObject.Repr());
            repr.Append(")");

            return repr.ToString();
        }
    }

    public class NewLocation : Location, IReproduceable
    {
        public readonly float X;
        public readonly float Y;

        public NewLocation(string id, string scene, string[] destroyObjects, string[] fsmEvents,
            string[] randoCallbacks, PlayerField<int>[] requiredInts, PlayerField<bool>[] requiredBools,
            string[] requiredCallbacks, float x, float y)
            : base(id, scene, destroyObjects, fsmEvents, randoCallbacks, requiredInts, requiredBools, requiredCallbacks)
        {
            X = x;
            Y = y;
        }

        public string Repr()
        {
            StringBuilder repr = new StringBuilder();
            repr.Append("new ");
            repr.Append(nameof(NewLocation));
            repr.Append("(");
            repr.Append(Id.Repr());
            repr.Append(", ");
            repr.Append(Scene.Repr());
            repr.Append(", ");
            repr.Append(DestroyObjects.Repr());
            repr.Append(", ");
            repr.Append(FsmEvents.Repr());
            repr.Append(", ");
            repr.Append(RandoCallbacks.Repr());
            repr.Append(", ");
            repr.Append(RequiredInts.Repr());
            repr.Append(", ");
            repr.Append(RequiredBools.Repr());
            repr.Append(", ");
            repr.Append(RequiredCallbacks.Repr());
            repr.Append(", ");
            repr.Append(X.Repr());
            repr.Append(", ");
            repr.Append(Y.Repr());
            repr.Append(")");

            return repr.ToString();
        }
    }
}
