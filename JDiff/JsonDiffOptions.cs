using System.Diagnostics;

namespace JDiff;

public class JsonDiffOptions
{
    public Func<DiffSymbol, string> SymbolToString { get; init; } = symbol => symbol switch
    {
        DiffSymbol.Added => "+",
        DiffSymbol.Removed => "-",
        DiffSymbol.Modified => "*",
        DiffSymbol.Unchanged => "",
        _ => throw new UnreachableException(
            $"Unexpected symbol {symbol}, only {string.Join(", ", Enum.GetNames<DiffSymbol>())} are allowed")
    };
}
