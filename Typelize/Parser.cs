using System;
using System.Collections.Generic;
using System.Linq;
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

        public TypeInfo(string type)
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
        }

        public virtual TypeFlags Type { get; }
        public string CustomTypeName { get; }

        public override string ToString()
        {
            return Type != TypeFlags.Custom ? Type.ToString() : CustomTypeName;
        }
    }

    public class ListTypeInfo : TypeInfo
    {
        public ListTypeInfo(string type)
        {
            if (Enum.TryParse<TypeFlags>(type, out var result))
            {
                ListType = result;
            }
            else
            {
                ListType = TypeFlags.Custom;
                CustomListTypeName = type;
            }
        }

        public override TypeFlags Type { get; } = TypeFlags.List;
        public TypeFlags ListType { get; }
        public string CustomListTypeName { get; }

        public override string ToString()
        {
            return $"List<{(ListType == TypeFlags.Custom ? CustomListTypeName : ListType.ToString())}>";
        }
    }

    public class MapTypeInfo : TypeInfo
    {
        public MapTypeInfo(string key, string value)
        {
            if (Enum.TryParse<TypeFlags>(key, out var keyType))
            {
                KeyType = keyType;
            }
            else
            {
                KeyType = TypeFlags.Custom;
                CustomKeyTypeName = key;
            }

            if (Enum.TryParse<TypeFlags>(value, out var valueType))
            {
                ValueType = valueType;
            }
            else
            {
                ValueType = TypeFlags.Custom;
                CustomValueTypeName = value;
            }
        }

        public override TypeFlags Type { get; } = TypeFlags.Map;
        public TypeFlags KeyType { get; }
        public TypeFlags ValueType { get; }
        public string CustomKeyTypeName { get; }
        public string CustomValueTypeName { get; }

        public override string ToString()
        {
            return
                $"Map<{(KeyType == TypeFlags.Custom ? CustomKeyTypeName : KeyType.ToString())}, {(ValueType == TypeFlags.Custom ? CustomValueTypeName : ValueType.ToString())}>";
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
        private static readonly Parser<TypeInfo> MapParser =
            from pref in Parse.String("Map")
            from openBk in Parse.Char('<').Token()
            from key in Parse.LetterOrDigit.Many().Text().Token()
            from sp in Parse.Char(',').Token()
            from value in Parse.LetterOrDigit.Many().Text().Token()
            from closeBk in Parse.Char('>')
            select new MapTypeInfo(key, value);

        private static readonly Parser<TypeInfo> ListParser =
            from pref in Parse.String("List")
            from openBk in Parse.Char('<').Token()
            from type in Parse.LetterOrDigit.Many().Text().Token()
            from closeBk in Parse.Char('>').Token()
            select new ListTypeInfo(type);

        private static readonly Parser<TypeInfo> CommonTypeParser =
            from typeSub in Parse.LetterOrDigit.Many().Text().Token()
            select new TypeInfo(typeSub);

        private static readonly Parser<TypeInfo> PropertyTypeParser =
            MapParser.Or(ListParser).Or(CommonTypeParser);

        private static readonly Parser<PropertyItem> PropertyParser =
            from propNameSub in Parse.LetterOrDigit.Many().Text().Token()
            from isNullable in Parse.Char('?').Optional().Token()
            from m in Parse.Char(':').Once().Token()
            from type in PropertyTypeParser
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