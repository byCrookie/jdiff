using System.Text.Json.Nodes;

namespace JDiff;

public record DiffNode(DiffSymbol Symbol, JsonNode Node);
