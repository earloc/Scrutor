using Scrutor.Analyzers;

namespace Foo;


public class Config
{
    public static void Configure(Scrutor.ITypeSourceSelector Scan)
    {
        Scan.FromAssemblyOf<IAbstraction>()
            .AddClasses(classes => classes.AssignableTo<IAbstraction>())
            .AsImplementedInterfaces()
            .WithTransientLifetime()
        ;

        Scan.FromAssemblyOf<IAbstraction>()
            .AddClasses(classes => classes.AssignableTo<IAbstraction>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        ;

        Scan.FromAssemblyOf<IAbstraction>()
            .AddClasses(classes => classes.AssignableTo<IAbstraction>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
        ;
    }
  
}

