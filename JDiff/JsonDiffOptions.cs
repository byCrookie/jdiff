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
    
    public Func<List<DiffSymbol>, DiffSymbol> ReduceSymbols { get; init; } = symbols =>
    {
        return symbols switch
        {
            _ when symbols.Any(s => s == DiffSymbol.Modified) => DiffSymbol.Modified,
            _ when symbols.All(s => s == DiffSymbol.Unchanged) => DiffSymbol.Unchanged,
            _ when symbols.All(s => s == DiffSymbol.Added) => DiffSymbol.Added,
            _ when symbols.All(s => s == DiffSymbol.Removed) => DiffSymbol.Removed,
            _ => DiffSymbol.Modified
        };
    };
}
