using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Nodes;
using JDiff;

var leftOption = new Option<FileInfo>(
    name: "--left",
    description: "The file to compare to the base file.");
leftOption.AddAlias("-l");
leftOption.IsRequired = true;

var rightOption = new Option<FileInfo>(
    name: "--right",
    description: "The base file to compare to.");
rightOption.AddAlias("-r");
rightOption.IsRequired = true;

var rootCommand = new RootCommand("jdiff");
rootCommand.AddOption(leftOption);
rootCommand.AddOption(rightOption);

rootCommand.SetHandler(async (left, right) => await DiffAsync(left, right), leftOption, rightOption);

return await rootCommand.InvokeAsync(args);

static async Task DiffAsync(FileInfo left, FileInfo right)
{
    if (!left.Exists)
    {
        Console.WriteLine($"The file '{left.FullName}' does not exist.");
        return;
    }

    if (!right.Exists)
    {
        Console.WriteLine($"The file '{right.FullName}' does not exist.");
        return;
    }

    var leftJson = await File.ReadAllTextAsync(left.FullName);
    var rightJson = await File.ReadAllTextAsync(right.FullName);

    var leftNode = JsonNode.Parse(leftJson);
    var rightNode = JsonNode.Parse(rightJson);

    if (leftNode is null)
    {
        Console.WriteLine($"The file '{left.FullName}' is not valid JSON.");
        return;
    }

    if (rightNode is null)
    {
        Console.WriteLine($"The file '{right.FullName}' is not valid JSON.");
        return;
    }

    var diff = leftNode.Diff(rightNode);

    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    Console.WriteLine(diff.ToJsonString(options));
}
