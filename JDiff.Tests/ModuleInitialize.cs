using System.Runtime.CompilerServices;

namespace JDiff.Tests;

public static class ModuleInitialize
{
    [ModuleInitializer]
    public static void Init() =>
        VerifySystemJson.Initialize();
}
