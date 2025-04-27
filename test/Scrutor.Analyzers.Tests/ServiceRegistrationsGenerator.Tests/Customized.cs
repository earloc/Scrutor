
using Scrutor;

namespace Foo;

public interface IAbstraction
{
}

public class Implementation : IAbstraction
{
}

public class TransientImplementation : IAbstraction
{
}

public class ScopedImplementation : IAbstraction
{
}

public class SingletonImplementation : IAbstraction
{
}

public static class Config
{
    public static void Configure(ITypeSourceSelector Scan)
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

