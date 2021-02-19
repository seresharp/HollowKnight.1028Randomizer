using System;
using System.Linq;
using System.Text;

namespace Randomizer.Serialized
{
    public interface IReproduceable
    {
        string Repr();
    }

    public static class ReprExtensions
    {
        public static string Repr(this string str)
            => str == null ? "null" : $"\"{str}\"";

        public static string Repr(this bool b)
            => b ? "true" : "false";

        public static string Repr(this float f)
            => f + "f";

        public static string Repr<T>(this T[] array, bool newLines = false) where T : IReproduceable
        {
            if (array == null)
            {
                return "null";
            }

            if (array.Length == 0)
            {
                return $"new {typeof(T).ReprName()}[0]";
            }

            return $"new {typeof(T).ReprName()}[] {{{(newLines ? "\n    " : " ")}{string.Join($", {(newLines ? "\n    " : "")}", array.Select(item => item.Repr()).ToArray())}{(newLines ? "\n" : " ")}}}";
        }

        public static string Repr(this string[] array)
        {
            if (array == null)
            {
                return "null";
            }

            if (array.Length == 0)
            {
                return $"new string[0]";
            }

            return $"new string[] {{ {string.Join(", ", array.Select(item => item.Repr()).ToArray())} }}";
        }

        public static string ReprName(this Type t)
        {
            if (!t.IsGenericType)
            {
                return t.Name switch
                {
                    "Int32" => "int",
                    "Boolean" => "bool",
                    _ => t.Name
                };
            }

            StringBuilder repr = new StringBuilder();
            repr.Append(t.Name.Substring(0, t.Name.IndexOf("`")));
            repr.Append("<");
            repr.Append(string.Join(", ", t.GetGenericArguments().Select(arg => arg.ReprName()).ToArray()));
            repr.Append(">");

            return repr.ToString();
        }
    }
}
