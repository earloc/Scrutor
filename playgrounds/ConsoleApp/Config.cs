// See https://aka.ms/new-console-template for more information
using Scrutor;

// namespace Playground;

public class Config
{
    public static void Configure(ITypeSourceSelector scan)
    {
        scan
            .FromAssemblyOf<ITypeSelector>()
            .AddClasses(classes => classes.AssignableTo<ITypeSelector>())
            .AsSelf()
            .WithScopedLifetime();
    }
}
