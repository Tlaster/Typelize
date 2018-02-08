using System;
using System.Linq;
using Humanizer;

namespace Typelize
{
    public static class Generator
    {
        public static string ToCSharp(this TypeClass data, string nameSpace)
        {
            return
                $"namespace {nameSpace}{Environment.NewLine}{{{Environment.NewLine}public class {data.Name}{Environment.NewLine}{{{Environment.NewLine}{string.Join("", data.PropertyItems.Select(item => item.ToCSharp()))}}}{Environment.NewLine}}}";
        }

        public static string ToCSharp(this PropertyItem item)
        {
            return $"[JsonProperty(\"{item.Name}\")]{Environment.NewLine}" +
                   $"public {item.TypeInfo.ToCSharp()} {item.Name.Dehumanize()} {{ get; set; }}{Environment.NewLine}";
        }

        public static string ToCSharp(this TypeInfo typeInfo)
        {
            switch (typeInfo)
            {
                case null:
                    return "object";
                default:
                    return
                        $"{(typeInfo.Type == TypeFlags.Custom ? typeInfo.CustomTypeName : typeInfo.Type.ToCSharp())}{(typeInfo.IsGenericType ? $"<{string.Join(", ", typeInfo.GenericTypeinfos.Select(item => item.ToString()))}>" : "")}";
            }
        }

        public static string ToCSharp(this TypeFlags flags)
        {
            switch (flags)
            {
                case TypeFlags.Any:
                case TypeFlags.Null:
                    return "object";
                case TypeFlags.String:
                    return "string";
                case TypeFlags.Int:
                    return "int";
                case TypeFlags.Double:
                    return "double";
                case TypeFlags.Bool:
                    return "bool";
                case TypeFlags.List:
                    return "List";
                case TypeFlags.Map:
                    return "Dictionary";
                case TypeFlags.Custom:
                default:
                    throw new ArgumentOutOfRangeException(nameof(flags), flags, null);
            }
        }
    }
}