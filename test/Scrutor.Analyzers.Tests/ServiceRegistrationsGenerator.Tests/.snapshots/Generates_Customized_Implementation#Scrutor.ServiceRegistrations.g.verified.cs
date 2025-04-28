//HintName: Scrutor.ServiceRegistrations.g.cs
    using System;
    using System.Collections.Generic;
    using System.Linq;
    namespace Microsoft.Extensions.DependencyInjection
    {
        public static class ScrutorServiceCollectionExtensions
        {
            public static IServiceCollection AddScannedServices(this IServiceCollection services)
            {
                services.AddTransient<Foo.IAbstraction, Foo.TransientImplementation>();
                services.AddScoped<Foo.IAbstraction, Foo.ScopedImplementation>();
                services.AddSingleton<Foo.IAbstraction, Foo.SingletonImplementation>();
                return services;
            }
        }
    }