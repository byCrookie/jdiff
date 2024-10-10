using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JDiff;

public static class JDiff
{
    public static JsonNode Diff(this JsonNode? left, JsonNode? right)
    {
        return DiffInternal(left, right).Node;
    }

    private static DiffNode DiffInternal(JsonNode? left, JsonNode? right)
    {
        if (left is null || right is null)
        {
            if (left is null && right is not null)
            {
                return new DiffNode(DiffSymbol.Removed, right.DeepClone());
            }

            if (right is null && left is not null)
            {
                return new DiffNode(DiffSymbol.Added, left.DeepClone());
            }

            return new DiffNode(DiffSymbol.Unchanged, new JsonObject());
        }

        if (left.GetValueKind() != right.GetValueKind())
        {
            return new DiffNode(DiffSymbol.Modified, left.DeepClone());
        }

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
            case JsonValueKind.Undefined:
                return new DiffNode(DiffSymbol.Modified, left.DeepClone());
            default:
                throw new UnreachableException(
                    $"Unexpected value kind {left.GetValueKind()}, only {string.Join(", ", Enum.GetNames<JsonValueKind>())} are allowed");
        }
    }

    private static DiffSymbol DiffPrimitive<T>(JsonNode left, JsonNode right)
    {
        if (left is not JsonValue leftValue || right is not JsonValue rightValue ||
            left.GetValueKind() != rightValue.GetValueKind())
        {
            return DiffSymbol.Modified;
        }

        if (!leftValue.TryGetValue<T>(out var leftString) || !rightValue.TryGetValue<T>(out var rightString))
        {
            throw new UnreachableException(
                $"Expected both nodes to be of type {typeof(T)}, but got {left.GetPropertyName()}:{left.GetValueKind()} and {right.GetPropertyName()}{right.GetValueKind()}");
        }

        return leftString.Equals(rightString) ? DiffSymbol.Unchanged : DiffSymbol.Modified;
    }

    private static DiffNode DiffArrays(JsonNode left, JsonNode right)
    {
        if (left is JsonArray leftArray && right is JsonArray rightArray)
        {
            var symbols = new List<DiffSymbol>();
            var diff = new JsonArray();

            for (var i = 0; i < Math.Max(leftArray.Count, rightArray.Count); i++)
            {
                var leftElement = i < leftArray.Count ? leftArray[i] : null;
                var rightElement = i < rightArray.Count ? rightArray[i] : null;

                var diffNode = DiffInternal(leftElement, rightElement);
                diff.Add(diffNode.Node);
                symbols.Add(diffNode.Symbol);
            }

            return SymbolsToDiffNode(symbols, diff);
        }

        throw new UnreachableException(
            $"Expected both nodes to be arrays, but got {left.GetPropertyName()}:{left.GetValueKind()} and {right.GetPropertyName()}{right.GetValueKind()}");
    }

    private static DiffNode DiffObjects(JsonNode left, JsonNode right)
    {
        if (left is JsonObject leftObject && right is JsonObject rightObject)
        {
            var symbols = new List<DiffSymbol>();
            var diff = new JsonObject();

            foreach (var property in leftObject.Select(l => new { Left = l, Right = rightObject[l.Key] }))
            {
                if (property.Right is null)
                {
                    diff[$"+{property.Left.Key}"] = property.Left.Value?.DeepClone();
                    symbols.Add(DiffSymbol.Added);
                }

                var diffNode = DiffInternal(property.Left.Value, property.Right);
                diff[$"{diffNode.Symbol.AsString()}{property.Left.Key}"] = diffNode.Node;
                symbols.Add(diffNode.Symbol);
            }

            foreach (var rightProperty in rightObject.Where(rightProperty => leftObject[rightProperty.Key] is null))
            {
                diff[$"-{rightProperty.Key}"] = rightProperty.Value?.DeepClone();
                symbols.Add(DiffSymbol.Removed);
            }

            return SymbolsToDiffNode(symbols, diff);
        }

        throw new UnreachableException(
            $"Expected both nodes to be objects, but got {left.GetPropertyName()}:{left.GetValueKind()} and {right.GetPropertyName()}{right.GetValueKind()}");
    }

    private static DiffNode SymbolsToDiffNode(List<DiffSymbol> symbols, JsonNode diff)
    {
        return symbols switch
        {
            _ when symbols.Any(s => s == DiffSymbol.Modified) => new DiffNode(DiffSymbol.Modified, diff),
            _ when symbols.All(s => s == DiffSymbol.Unchanged) => new DiffNode(DiffSymbol.Unchanged, diff),
            _ when symbols.All(s => s == DiffSymbol.Added) => new DiffNode(DiffSymbol.Added, diff),
            _ when symbols.All(s => s == DiffSymbol.Removed) => new DiffNode(DiffSymbol.Removed, diff),
            _ => new DiffNode(DiffSymbol.Modified, diff)
        };
    }

    private static string AsString(this DiffSymbol symbol)
    {
        return symbol switch
        {
            DiffSymbol.Added => "+",
            DiffSymbol.Removed => "-",
            DiffSymbol.Modified => "*",
            DiffSymbol.Unchanged => "",
            _ => throw new UnreachableException(
                $"Unexpected symbol {symbol}, only {string.Join(", ", Enum.GetNames<DiffSymbol>())} are allowed")
        };
    }
}
