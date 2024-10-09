using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace JDiff.Tests;

public static class Embedded
{
    public static async Task<JsonNode> LeftAsync([CallerMemberName] string memberName = "")
    {
        return await Parse(true, memberName);
    }

    public static async Task<JsonNode> RightAsync([CallerMemberName] string memberName = "")
    {
        return await Parse(false, memberName);
    }

    private static async Task<JsonNode> Parse(bool left = true, [CallerMemberName] string memberName = "")
    {
        var assembly = Assembly.GetExecutingAssembly();
        var direction = left ? "left" : "right";
        var resourceName = $"{assembly.GetName().Name}.{memberName}.{direction}.json";
        var resourceStream = assembly.GetManifestResourceStream(resourceName);
        if (resourceStream == null)
        {
            throw new InvalidOperationException($"Resource '{resourceName}' not found.");
        }

        using var reader = new StreamReader(resourceStream);
        var content = await reader.ReadToEndAsync();
        return JsonNode.Parse(content)!;
    }
}
