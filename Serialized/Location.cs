using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using Randomizer.Util;
using UnityEngine;

using PD = Randomizer.Patches.PlayerData;
using UObject = UnityEngine.Object;

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

        public bool HasRequired()
        {
            foreach (PlayerField<bool> pf in RequiredBools)
            {
                if (!pf.CheckValue())
                {
                    return false;
                }
            }

            foreach (PlayerField<int> pf in RequiredInts)
            {
                if (!pf.CheckValue(true))
                {
                    return false;
                }
            }

            foreach (string callName in RequiredCallbacks)
            {
                int dot = callName.IndexOf('.');
                Type t = Type.GetType(nameof(Randomizer) + "." + callName.Substring(0, dot));
                MethodInfo method = t.GetMethod(callName.Substring(dot + 1));

                if (!(bool)method.Invoke(null, new[] { this }))
                {
                    return false;
                }
            }

            return true;
        }

        public void SceneLoaded()
        {
            foreach (string objName in DestroyObjects)
            {
                static IEnumerator DeleteObject(string objName)
                {
                    while (true)
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

                DeleteObject(objName).RunCoroutine();
            }

            foreach (string callback in RandoCallbacks)
            {
                int dot = callback.IndexOf('.');
                Type t = Type.GetType(nameof(Randomizer) + "." + callback.Substring(0, dot));
                MethodInfo method = t.GetMethod(callback.Substring(dot + 1));

                method.Invoke(null, new[] { this });
            }
        }

        public void Collect()
        {
            // Set obtained
            PD.instance.obtainedLocations.Add(Id);

            // Run events
            static IEnumerator SendEvents(string[] events)
            {
                foreach (string e in events)
                {
                    PlayMakerFSM.BroadcastEvent(e);
                    yield return null;
                }
            }

            SendEvents(FsmEvents).RunCoroutine();

            // Remove geo
            int geo = RequiredInts
                .Where(i => i.FieldName == nameof(PlayerData.geo))
                .Select(i => i.Value)
                .Sum();

            if (geo > 0)
            {
                HeroController.instance.TakeGeo(geo);
            }
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
