using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JDiff;

public static class JDiff
{
    public static JsonNode Diff(this JsonNode? left, JsonNode? right, JsonDiffOptions? jsonDiffOptions = null)
    {
        return DiffInternal(left, right, jsonDiffOptions ?? new JsonDiffOptions()).Node ?? new JsonObject();
    }

    private static DiffNode DiffInternal(JsonNode? left, JsonNode? right, JsonDiffOptions jsonDiffOptions)
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

            return new DiffNode(DiffSymbol.Unchanged, null);
        }

        if (left.GetValueKind() != right.GetValueKind())
        {
            return new DiffNode(DiffSymbol.Modified, left.DeepClone());
        }

        switch (left.GetValueKind())
        {
            case JsonValueKind.Object:
                return DiffObjects(left, right, jsonDiffOptions);
            case JsonValueKind.Array:
                return DiffArrays(left, right, jsonDiffOptions);
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

    private static DiffNode DiffArrays(JsonNode left, JsonNode right, JsonDiffOptions jsonDiffOptions)
    {
        if (left is JsonArray leftArray && right is JsonArray rightArray)
        {
            var symbols = new List<DiffSymbol>();
            var diff = new JsonArray();

            for (var i = 0; i < Math.Max(leftArray.Count, rightArray.Count); i++)
            {
                var leftElement = i < leftArray.Count ? leftArray[i] : null;
                var rightElement = i < rightArray.Count ? rightArray[i] : null;

                var diffNode = DiffInternal(leftElement, rightElement, jsonDiffOptions);
                diff.Add(diffNode.Node);
                symbols.Add(diffNode.Symbol);
            }

            return new DiffNode(jsonDiffOptions.ReduceSymbols(symbols), diff);
        }

        throw new UnreachableException(
            $"Expected both nodes to be arrays, but got {left.GetPropertyName()}:{left.GetValueKind()} and {right.GetPropertyName()}{right.GetValueKind()}");
    }

    private static DiffNode DiffObjects(JsonNode left, JsonNode right, JsonDiffOptions jsonDiffOptions)
    {
        if (left is JsonObject leftObject && right is JsonObject rightObject)
        {
            var symbols = new List<DiffSymbol>();
            var diff = new JsonObject();

            foreach (var propertyZip in leftObject.Select(l =>
                         new
                         {
                             Left = l,
                             RightValue = rightObject[l.Key],
                             AttributeExistsInRight = rightObject.ContainsKey(l.Key)
                         }))
            {
                if (!propertyZip.AttributeExistsInRight)
                {
                    diff[$"{jsonDiffOptions.SymbolToString(DiffSymbol.Added)}{propertyZip.Left.Key}"] =
                        propertyZip.Left.Value?.DeepClone();
                    symbols.Add(DiffSymbol.Added);
                    continue;
                }

                var diffNode = DiffInternal(propertyZip.Left.Value, propertyZip.RightValue, jsonDiffOptions);
                diff[$"{jsonDiffOptions.SymbolToString(diffNode.Symbol)}{propertyZip.Left.Key}"] = diffNode.Node;
                symbols.Add(diffNode.Symbol);
            }

            foreach (var rightProperty in
                     rightObject.Where(rightProperty => !leftObject.ContainsKey(rightProperty.Key)))
            {
                diff[$"{jsonDiffOptions.SymbolToString(DiffSymbol.Removed)}{rightProperty.Key}"] =
                    rightProperty.Value?.DeepClone();
                symbols.Add(DiffSymbol.Removed);
            }

            return new DiffNode(jsonDiffOptions.ReduceSymbols(symbols), diff);
        }

        throw new UnreachableException(
            $"Expected both nodes to be objects, but got {left.GetPropertyName()}:{left.GetValueKind()} and {right.GetPropertyName()}{right.GetValueKind()}");
    }
}
