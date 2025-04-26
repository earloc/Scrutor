using Scrutor.Analyzers;

namespace Foo;


public class Config
{
    public static void Configure(Scrutor.ITypeSourceSelector Scan)
    {
        Scan.FromAssemblyOf<IAbstraction>()
            .AddClasses(classes => classes
                .AssignableTo<IAbstraction>()
                .Where(x => x.Name.StartsWith("Transient"))
            )
            .AsImplementedInterfaces()
            .WithTransientLifetime()
        ;

        Scan.FromAssemblyOf<IAbstraction>()
            .AddClasses(classes => classes
                .AssignableTo<IAbstraction>()
                .Where(x => x.Name.StartsWith("Scoped"))
            )
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        ;

        Scan.FromAssemblyOf<IAbstraction>()
            .AddClasses(classes => classes
                .AssignableTo<IAbstraction>()
                .Where(x => x.Name.StartsWith("Singleton"))
            )
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
        ;
    }
  
}

