using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace JDiff.Tests;

public static class Embedded
{
    public static async Task<JsonNode> LeftAsync([CallerFilePath] string sourceFile = "", [CallerMemberName] string memberName = "")
    {
        return await Parse(true, sourceFile, memberName);
    }

    public static async Task<JsonNode> RightAsync([CallerFilePath] string sourceFile = "", [CallerMemberName] string memberName = "")
    {
        return await Parse(false, sourceFile, memberName);
    }

    private static async Task<JsonNode> Parse(bool left, string sourceFile, string memberName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var direction = left ? "left" : "right";
        var typeName = Path.GetFileNameWithoutExtension(sourceFile);
        var resourceName = $"{assembly.GetName().Name}.{typeName}.{memberName}.{direction}.json";
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
