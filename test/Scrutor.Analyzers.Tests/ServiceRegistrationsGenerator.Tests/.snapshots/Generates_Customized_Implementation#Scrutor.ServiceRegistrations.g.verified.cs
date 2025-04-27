//HintName: Scrutor.ServiceRegistrations.g.cs
    using System;
    using System.Collections.Generic;
    using System.Linq;
    namespace Scrutor.Analyzers
    {
        public class ServiceCollectionExtensions
        {
            public static IServiceCollection AddServicesScrutor(this IServiceCollection services)
            {
                services.AddTransient<Foo.IAbstraction, Foo.TransientImplementation>();
                services.AddScoped<Foo.IAbstraction, Foo.ScopedImplementation>();
                services.AddSingleton<Foo.IAbstraction, Foo.SingletonImplementation>();
                return services;
            }
        }
    }