using System;
using System.Text;
using Randomizer.Util;
using UnityEngine;

namespace Randomizer.Serialized
{
    public class PlayerField<TField> : IReproduceable
    {
        public readonly string FieldName;
        public readonly TField Value;
        public readonly bool Increment;

        public PlayerField(string fieldName, TField value = default, bool increment = false)
        {
            FieldName = fieldName;
            Value = value;
            Increment = increment;
        }

        public bool CheckValue(bool allowGreater = false)
        {
            switch (Value)
            {
                case bool b:
                    return Ref.PD.GetBool(FieldName) == b;
                case int i:
                    return allowGreater
                        ? Ref.PD.GetInt(FieldName) >= i
                        : Ref.PD.GetInt(FieldName) == i;
                case float f:
                    return allowGreater
                        ? Ref.PD.GetFloat(FieldName) >= f
                        : Ref.PD.GetFloat(FieldName) == f;
                case string s:
                    return Ref.PD.GetString(FieldName) == s;
                case Vector3 v:
                    return Ref.PD.GetVector3(FieldName) == v;
                default:
                    throw new InvalidOperationException("Unsupported field type: " + typeof(TField).FullName);

            }
        }

        public void ApplyValue()
        {
            switch (Value)
            {
                case bool b:
                    Ref.PD.SetBool(FieldName, b);
                    break;
                case int i:
                    if (Increment)
                    {
                        i += Ref.PD.GetInt(FieldName);
                    }

                    Ref.PD.SetInt(FieldName, i);
                    break;
                case float f:
                    if (Increment)
                    {
                        f += Ref.PD.GetFloat(FieldName);
                    }

                    Ref.PD.SetFloat(FieldName, f);
                    break;
                case string s:
                    Ref.PD.SetString(FieldName, s);
                    break;
                case Vector3 v:
                    if (Increment)
                    {
                        v += Ref.PD.GetVector3(FieldName);
                    }

                    Ref.PD.SetVector3(FieldName, v);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported field type: " + typeof(TField).FullName);

            }
        }

        public string Repr()
        {
            StringBuilder repr = new StringBuilder();
            repr.Append("new ");
            repr.Append(GetType().ReprName());
            repr.Append("(");
            repr.Append(FieldName.Repr());
            repr.Append(", ");

            repr.Append(Value switch
            {
                IReproduceable r => r.Repr(),
                string s => s.Repr(),
                bool b => b.Repr(),
                _ => Value
            });

            repr.Append(", ");
            repr.Append(Increment.Repr());
            repr.Append(")");

            return repr.ToString();
        }
    }
}
