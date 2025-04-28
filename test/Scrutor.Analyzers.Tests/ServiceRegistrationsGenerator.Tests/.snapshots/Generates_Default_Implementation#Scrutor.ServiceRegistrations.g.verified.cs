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
                return services;
            }
        }
    }