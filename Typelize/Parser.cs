using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sprache;

namespace Typelize
{
    public enum TypeFlags
    {
        Any,
        Null,
        String,
        Int,
        Bool,
        Double,
        List,
        Map,
        Custom
    }

    public class TypeClass
    {
        public string Name { get; internal set; }

        public List<PropertyItem> PropertyItems { get; internal set; }

        public override string ToString()
        {
            return $"class {Name} {{{Environment.NewLine}\t{string.Join("\t", PropertyItems)}}}";
        }
    }

    public class TypeInfo
    {
        protected TypeInfo()
        {
        }

        public TypeInfo(string type, IEnumerable<TypeInfo> genes = null)
        {
            if (Enum.TryParse<TypeFlags>(type, out var result))
            {
                Type = result;
            }
            else
            {
                Type = TypeFlags.Custom;
                CustomTypeName = type;
            }

            GenericTypeinfos = genes?.ToArray();
        }

        public TypeInfo(JTokenType valueType)
        {
            switch (valueType)
            {
                case JTokenType.Integer:
                    Type = TypeFlags.Int;
                    break;
                case JTokenType.Float:
                    Type = TypeFlags.Double;
                    break;
                case JTokenType.String:
                    Type = TypeFlags.String;
                    break;
                case JTokenType.Boolean:
                    Type = TypeFlags.Bool;
                    break;
                case JTokenType.Undefined:
                case JTokenType.Null:
                    Type = TypeFlags.Null;
                    break;
                default:
                    Type = TypeFlags.Custom;
                    CustomTypeName = valueType.ToString();
                    break;
            }
        }

        public TypeFlags Type { get; }
        public string CustomTypeName { get; }
        public bool IsGenericType => GenericTypeinfos != null && GenericTypeinfos.Any();
        public TypeInfo[] GenericTypeinfos { get; }

        public override string ToString()
        {
            return
                $"{(Type != TypeFlags.Custom ? Type.ToString() : CustomTypeName)}{(IsGenericType ? $"<{string.Join(", ", GenericTypeinfos.Select(item => item.ToString()))}>" : "")}";
        }
    }

    public class PropertyItem
    {
        public string Name { get; internal set; }
        public bool Nullable { get; internal set; }
        public TypeInfo TypeInfo { get; internal set; }

        public override string ToString()
        {
            return $"{Name}{(Nullable ? "?" : "")}: {TypeInfo}{Environment.NewLine}";
        }
    }

    public static class TypeParser
    {
        private static readonly Parser<TypeInfo> GenericParser =
            from pref in Parse.LetterOrDigit.Many().Text().Token()
            from genes in GenericTypeParser.Optional()
            select new TypeInfo(pref, genes.GetOrDefault());

        private static readonly Parser<IEnumerable<TypeInfo>> GenericTypeParser =
            from openBk in Parse.Char('<').Token()
            from genes in Parse.Ref(() =>
                from type in GenericParser from spliter in Parse.Char(',').Optional().Token() select type).Many()
            from closeBk in Parse.Char('>').Token()
            select genes;

        private static readonly Parser<PropertyItem> PropertyParser =
            from propNameSub in Parse.LetterOrDigit.Many().Text().Token()
            from isNullable in Parse.Char('?').Optional().Token()
            from m in Parse.Char(':').Once().Token()
            from type in GenericParser
            select new PropertyItem
            {
                Name = propNameSub,
                TypeInfo = type,
                Nullable = isNullable.IsDefined
            };

        private static readonly Parser<IEnumerable<PropertyItem>> Menbers =
            PropertyParser.Many();

        private static readonly Parser<TypeClass> TypeClassParser =
            from classdef in Parse.String("class").Token()
            from classNameRes in Parse.LetterOrDigit.Many().Text()
            from openBk in Parse.Char('{').Once().Token()
            from props in Menbers.Optional()
            from closeBk in Parse.Char('}').Once().Token()
            select new TypeClass
            {
                Name = classNameRes,
                PropertyItems = props.GetOrDefault()?.ToList() ?? new List<PropertyItem>()
            };

        public static readonly Parser<IEnumerable<TypeClass>> Parser =
            TypeClassParser.AtLeastOnce();
    }
}