using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor.Analyzers;

public class ScriptContext
{
    public ITypeSourceSelector? Scan { get; set; }
}


public interface IAbstraction
{

}

public class Implementation : IAbstraction
{

}

[Generator(LanguageNames.CSharp)]
public sealed class ServiceRegistrationsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var hookProvider = context.CompilationProvider.Select((compilation, cancel) =>
        {
            
            var dateOnlyType = compilation.GetTypeByMetadataName("System.DateOnly");

            var configure = compilation.GetSymbolsWithName("Configure", SymbolFilter.Member, cancel).FirstOrDefault();

            // var member = fooType?.GetMembers().Where(x => x.GetAttributes().Any(y => y?.AttributeClass?.Name == nameof(CLSCompliantAttribute))).FirstOrDefault();
            var methodSyntax = configure as IMethodSymbol;
            var userProvidedString = "empty";
            if (methodSyntax is not null)
            {
               var result = Execute(methodSyntax).Result;
               userProvidedString = result ?? "none";
            }

            return userProvidedString;
        });

        context.RegisterSourceOutput(hookProvider, (ctxt, source) =>
        {
            var builder = new StringBuilder();
            
            builder.Append($$"""
                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using System.Threading.Tasks;

                namespace Scrutor.Analyzers
                {
                    public class Foo
                    {
                        public string Config { get; set; } = "{{source}}";
                    }
                }
            """);

            ctxt.AddSource($"Scrutor.ServiceRegistrations.g.cs", builder.ToString());
        });
    }

    private async Task<string?> Execute(IMethodSymbol methodSymbol)
    {
        var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null)
        {
            return null;
        }

        var syntaxNode = syntaxReference.GetSyntax();
        if (syntaxNode is not MethodDeclarationSyntax methodDeclaration)
        {
            return null;
        }

        var body = methodDeclaration.Body?.ToFullString().Replace("{", "").Replace("}", "");
        if (body == null)
        {
            return null;
        }

        try
        {
            var options = ScriptOptions.Default
                .AddReferences(typeof(ScriptContext).Assembly)
                .AddImports("System", "Scrutor", "Scrutor.Analyzers");

            var services = new ServiceCollection();
            var selector = new TypeSourceSelector(); // TODO: replace it with roslyn-implementation

                var context = new ScriptContext()
                {
                    Scan = selector
                };
                
                // TODO: would be better, if we'd have a 'Task IServiceColection.ScanAsync(Action<ITypeSourceSelector>, CancellationToken cancel)' method
                CSharpScript
                    .EvaluateAsync(body, options, context)
                    .GetAwaiter()
                    .GetResult()
                ;

            selector.Populate(services, RegistrationStrategy.Append);

            return services.Count.ToString();
        }
        catch (CompilationErrorException ex)
        {
            Debug.WriteLine($"Script compilation failed: {ex.Message}");
            return null;
        }
    }
}
