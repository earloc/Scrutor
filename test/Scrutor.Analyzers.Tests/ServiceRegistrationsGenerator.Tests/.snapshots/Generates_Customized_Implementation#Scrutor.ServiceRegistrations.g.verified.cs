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
                services.AddTransient<Scrutor.Analyzers.IAbstraction, Scrutor.Analyzers.TransientImplementation>();
                services.AddScoped<Scrutor.Analyzers.IAbstraction, Scrutor.Analyzers.ScopedImplementation>();
                services.AddSingleton<Scrutor.Analyzers.IAbstraction, Scrutor.Analyzers.SingletonImplementation>();
                return services;
            }
        }
    }