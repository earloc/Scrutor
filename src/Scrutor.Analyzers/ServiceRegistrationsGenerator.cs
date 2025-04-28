using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor.Analyzers;

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
               return DiscoverServicesAsync(methodSyntax, compilation).GetAwaiter().GetResult();
            }

            return ([], []);
        });

        context.RegisterSourceOutput(hookProvider, (ctxt, result) =>
        {

            foreach (var diagnostic in result.Diagnostics)
            {
                ctxt.ReportDiagnostic(diagnostic);
            }


            var registrations = new StringBuilder();
            var indent = "                ";
            foreach (var serviceDescriptor in result.DiscoveredServices)
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

                namespace Microsoft.Extensions.DependencyInjection
                {
                    public static class ScrutorServiceCollectionExtensions
                    {
                        public static IServiceCollection AddScannedServices(this IServiceCollection services)
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

    private async Task<(ServiceDescriptor[] DiscoveredServices, Diagnostic[] Diagnostics)> DiscoverServicesAsync(IMethodSymbol methodSymbol, Compilation compilation)
    {
        var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null)
        {
            return ([], []);
        }

        var syntaxNode = syntaxReference.GetSyntax();
        if (syntaxNode is not MethodDeclarationSyntax methodDeclaration)
        {
            return ([], []);
        }

        var declaringSyntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();

        var syntaxTree = syntaxReference.SyntaxTree;
        if (syntaxTree == null)
        {
            return ([], []);
        }

        var root = syntaxTree.GetRoot() as CompilationUnitSyntax;
        if (root == null)
        {
            return ([], []);
        }

        // Extract all namespace declarations
        var usingDirectives = root.DescendantNodes()
            .OfType<UsingDirectiveSyntax>()
            .Select(x => x.NamespaceOrType.ToString())
            .ToArray();

        var namespaceDeclarations = root.DescendantNodes()
            .OfType<NamespaceDeclarationSyntax>()
            .Select(x => x.Name.ToString())
            .ToArray();

        var fileScopedNamespaces = root.DescendantNodes()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .Select(x => x.Name.ToString())
            .ToArray();

        var body = methodDeclaration.Body?.ToFullString().Replace("{", "").Replace("}", "");
        if (body == null)
        {
            return ([], []);
        }

        var refs = compilation.References.Where(x => x.Display.Contains("Scrutor")).ToArray();

        var referenceWarnings = refs.Select(x => new DiagnosticDescriptor(
                "SCRUTOR004",
                "Reference warning",
                x.Display ?? "unkonw",
                "Usage",
                DiagnosticSeverity.Warning,
                true
        ))
        .Select(x => Diagnostic.Create(x, null))
        .ToArray();

        try
        {
            var options = ScriptOptions.Default
                .WithReferences(refs)
                // .WithReferences(typeof(ScriptContext).Assembly)
                .AddImports(namespaceDeclarations)
                .AddImports(fileScopedNamespaces)
                .AddImports(usingDirectives);

            var services = new ServiceCollection();
            var selector = new TypeSourceSelector();

            var context = new ScriptContext()
            {
                scan = selector
            };
            
            var result = await CSharpScript.EvaluateAsync(body, options, context);

            selector.Populate(services, RegistrationStrategy.Append);

            return (services.ToArray(), []);
        }
        catch (CompilationErrorException ex)
        {
            //TODO: report diagnostics
            var descriptor = new DiagnosticDescriptor(
                "SCRUTOR001",
                "Script compilation error",
                ex.Message,
                "Usage",
                DiagnosticSeverity.Error,
                true,
                ex.ToString()
            );
            return ([], [..referenceWarnings, Diagnostic.Create(descriptor, null)]);
        }
        catch (Exception ex)
        {
            //TODO: report diagnostics
            var descriptor = new DiagnosticDescriptor(
                "SCRUTOR002",
                "Script execution error",
                ex.Message,
                "Usage",
                DiagnosticSeverity.Error,
                true,
                ex.ToString()
            );
            return ([], [..referenceWarnings, Diagnostic.Create(descriptor, null)]);
        }
    }
}
