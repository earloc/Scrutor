using System;

namespace Scrutor.Analyzers.Tests;

public class ServiceRegistrationsGenerator_Tests
{
    private const string BaseDirectory = "../../../ServiceRegistrationsGenerator.Tests";

    [Fact]
    public async Task Generates_Default_Implementation() => await GeneratorTesterBuilder<ServiceRegistrationsGenerator>
        .Create(BaseDirectory)
        .Build()
        .Verify()
    ;

    [Fact]
    public async Task Generates_Customized_Implementation() => await GeneratorTesterBuilder<ServiceRegistrationsGenerator>
        .Create(BaseDirectory)
        .WithSourceFile("Customized.cs")
        .Build()
        .Verify()
    ;
}
