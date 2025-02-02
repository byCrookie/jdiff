namespace JDiff.Tests;

public class JDiffTests
{
    [Fact]
    public async Task Diff_WhenHasBeenModified_MarkAsModified()
    {
        var left = await Embedded.LeftAsync();
        var right = await Embedded.RightAsync();

        var diff = left.Diff(right, new JsonDiffOptions
        {
            SymbolToString = s => s switch
            {
                DiffSymbol.Added => "+",
                DiffSymbol.Removed => "-",
                DiffSymbol.Modified => "*",
                DiffSymbol.Unchanged => "",
                _ => "~"
            }
        });

        await Verify(diff);
    }

    [Fact]
    public async Task Diff_WhenDeep_Mark()
    {
        var left = await Embedded.LeftAsync();
        var right = await Embedded.RightAsync();

        var diff = left.Diff(right);

        await Verify(diff);
    }
}
