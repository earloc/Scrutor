using System.Runtime.CompilerServices;

namespace Scrutor.Analyzers.Tests;

internal interface IVerifiable
{
    Task Verify([CallerMemberName] string caller = "");
}
