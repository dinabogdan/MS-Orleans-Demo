using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Configuration.Overrides;
using Orleans.Storage;
using System;

namespace Silo.GrainStorage
{
    public static class FileSiloBuilderExtensions
    {
        public static ISiloHostBuilder AddFileGrainStorage(this ISiloHostBuilder builder,
            string providerName, Action<FileGrainStorageOptions> options)
        {
            return builder.ConfigureServices((context, services) => services.AddFileGrainStorage(providerName, options));
        }

        public static IServiceCollection AddFileGrainStorage(this IServiceCollection services, string providerName, Action<FileGrainStorageOptions> options)
        {
            services.AddOptions<FileGrainStorageOptions>(providerName).Configure(options);
            return services
                .AddSingletonNamedService<IGrainStorage>(providerName, (serviceProvider, name) =>
                {
                    IOptionsSnapshot<FileGrainStorageOptions> optionsSnapshot = 
                    serviceProvider.GetRequiredService<IOptionsSnapshot<FileGrainStorageOptions>>();

                    return ActivatorUtilities.CreateInstance<FileGrainStorage>(serviceProvider, name,
                        optionsSnapshot.Get(name), serviceProvider.GetProviderClusterOptions(name));
                })
                .AddSingletonNamedService(providerName, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
        }
    }
}