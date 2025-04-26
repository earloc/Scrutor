using System.Runtime.CompilerServices;

namespace Scrutor.Analyzers.Tests;

#pragma warning disable CA1515 // Consider making public types internal
public static class ModuleInitializer
#pragma warning restore CA1515 // Consider making public types internal
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
