using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor.Analyzers;

//TODO: move

public class ScriptContext
{
    public ITypeSourceSelector? Scan { get; set; }
}

//TODO: get rid of it
public interface IAbstraction
{

}

//TODO: get rid of it

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
            var configure = compilation.GetSymbolsWithName("Configure", SymbolFilter.Member, cancel).FirstOrDefault();

            // var member = fooType?.GetMembers().Where(x => x.GetAttributes().Any(y => y?.AttributeClass?.Name == nameof(CLSCompliantAttribute))).FirstOrDefault();
            var methodSyntax = configure as IMethodSymbol;
            if (methodSyntax is not null)
            {
               return  DiscoverServicesAsync(methodSyntax).GetAwaiter().GetResult();
            }

            return [];
        });

        context.RegisterSourceOutput(hookProvider, (ctxt, serviceDescriptors) =>
        {
            var registrations = new StringBuilder();
            var indent = "                ";
            foreach (var serviceDescriptor in serviceDescriptors)
            {
                registrations.AppendLine($$"""

                    {{indent}}services.Add{{serviceDescriptor.Lifetime}}<{{serviceDescriptor.ServiceType}}, {{serviceDescriptor.ImplementationType}}>();
                    """
                );
            }

            var builder = new StringBuilder();
            
            builder.Append($$"""

                using System;
                using System.Collections.Generic;
                using System.Linq;

                namespace Scrutor.Analyzers
                {
                    public class ServiceCollectionExtensions
                    {
                        public static IServiceCollection AddServicesScrutor(this IServiceCollection services)
                        {
                            {{registrations}}
                            return services;
                        }
                    }
                }
            """);

            ctxt.AddSource($"Scrutor.ServiceRegistrations.g.cs", builder.ToString());
        });
    }

    private async Task<ServiceDescriptor[]> DiscoverServicesAsync(IMethodSymbol methodSymbol)
    {
        var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null)
        {
            return [];
        }

        var syntaxNode = syntaxReference.GetSyntax();
        if (syntaxNode is not MethodDeclarationSyntax methodDeclaration)
        {
            return [];
        }

        var body = methodDeclaration.Body?.ToFullString().Replace("{", "").Replace("}", "");
        if (body == null)
        {
            return [];
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
                
                await CSharpScript
                    .EvaluateAsync(body, options, context)
                ;

            selector.Populate(services, RegistrationStrategy.Append);

            return services.ToArray();
        }
        catch (CompilationErrorException ex)
        {
            //TODO: report diagnostics
            Debug.WriteLine($"Script compilation failed: {ex.Message}");
            return [];
        }
    }
}
