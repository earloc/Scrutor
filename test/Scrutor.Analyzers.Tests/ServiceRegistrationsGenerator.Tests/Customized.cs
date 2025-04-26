using Scrutor.Analyzers;

namespace Foo;


public class Config
{


    [CLSCompliant(false)]
    public static void Configure(Scrutor.ITypeSourceSelector Scan)
    {
        Scan.FromAssemblyOf<IAbstraction>()
            .AddClasses(classes => classes.AssignableTo<IAbstraction>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        ;
    }
  
}

