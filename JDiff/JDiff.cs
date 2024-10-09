using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JDiff;

public static class JDiff
{
    public static JsonNode Diff(this JsonNode left, JsonNode right)
    {
        var diffNode = DiffInternal(left, right);
        return diffNode.Node;
    }

    public static DiffNode DiffInternal(JsonNode left, JsonNode right)
    {
        switch (left.GetValueKind())
        {
            case JsonValueKind.Object:
                return DiffObjects(left, right);
            case JsonValueKind.Array:
                return DiffArrays(left, right);
            case JsonValueKind.String:
                var ds = DiffPrimitive<string>(left, right);
                return new DiffNode(ds, left.DeepClone());
            case JsonValueKind.Number:
                var dn = DiffPrimitive<decimal>(left, right);
                return new DiffNode(dn, left.DeepClone());
            case JsonValueKind.True:
            case JsonValueKind.False:
                var db = DiffPrimitive<bool>(left, right);
                return new DiffNode(db, left.DeepClone());
            case JsonValueKind.Null:
                var dnu = DiffPrimitive<object>(left, right);
                return new DiffNode(dnu, left.DeepClone());
            default:
                throw new UnreachableException();
        }
    }

    private static DiffSymbol DiffPrimitive<T>(JsonNode left, JsonNode right)
    {
        if (left is not JsonValue leftValue || right is not JsonValue rightValue ||
            left.GetValueKind() != rightValue.GetValueKind())
        {
            throw new UnreachableException();
        }

        if (!leftValue.TryGetValue<T>(out var leftString) || !rightValue.TryGetValue<T>(out var rightString))
        {
            throw new UnreachableException();
        }

        return leftString.Equals(rightString) ? DiffSymbol.Unchanged : DiffSymbol.Modified;
    }

    private static DiffNode DiffArrays(JsonNode left, JsonNode right)
    {
        throw new NotImplementedException();
    }

    private static DiffNode DiffObjects(JsonNode left, JsonNode right)
    {
        if (left is JsonObject leftObject && right is JsonObject rightObject)
        {
            var symbols = new List<DiffSymbol>();
            var diff = new JsonObject();

            foreach (var leftProperty in leftObject.ToList())
            {
                var rightProperty = rightObject[leftProperty.Key];

                if (rightProperty is null)
                {
                    diff[$"+{leftProperty.Key}"] = leftProperty.Value?.DeepClone();
                    symbols.Add(DiffSymbol.Added);
                }

                var diffNode = DiffInternal(leftProperty.Value!, rightProperty!);
                diff[$"{diffNode.Symbol.S()}{leftProperty.Key}"] = diffNode.Node;
                symbols.Add(diffNode.Symbol);
            }

            foreach (var rightProperty in rightObject.ToList())
            {
                if (leftObject[rightProperty.Key] is null)
                {
                    diff[$"-{rightProperty.Key}"] = rightProperty.Value?.DeepClone();
                    symbols.Add(DiffSymbol.Removed);
                }
            }

            return symbols switch
            {
                _ when symbols.Any(s => s == DiffSymbol.Modified) => new DiffNode(DiffSymbol.Modified, diff),
                _ when symbols.All(s => s == DiffSymbol.Added) => new DiffNode(DiffSymbol.Added, diff),
                _ when symbols.All(s => s == DiffSymbol.Removed) => new DiffNode(DiffSymbol.Removed, diff),
                _ => new DiffNode(DiffSymbol.Unchanged, diff)
            };
        }

        throw new UnreachableException();
    }

    private static string S(this DiffSymbol symbol)
    {
        return symbol switch
        {
            DiffSymbol.Added => "+",
            DiffSymbol.Removed => "-",
            DiffSymbol.Modified => "*",
            DiffSymbol.Unchanged => "",
            _ => throw new UnreachableException()
        };
    }
}
