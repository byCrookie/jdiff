namespace JDiff.Tests;

public class JDiffTests
{
    [Fact]
    public async Task Diff_WhenHasBeenModified_MarkAsModified()
    {
        var left = await Embedded.LeftAsync();
        var right = await Embedded.RightAsync();
        
        var diff = left.Diff(right);
        
        await Verify(diff);
    }
}
