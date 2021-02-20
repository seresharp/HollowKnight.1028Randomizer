using System.Collections;
using UnityEngine;

namespace Randomizer.Util
{
    public static class IEnumeratorExtensions
    {
        public static void RunCoroutine(this IEnumerator enumerator)
            => new GameObject().AddComponent<NonBouncer>().StartCoroutine(enumerator);
    }
}
