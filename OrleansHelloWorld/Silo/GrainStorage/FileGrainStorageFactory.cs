using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Storage;

namespace Silo.GrainStorage
{
    public static class FileGrainStorageFactory
    {
        internal static IGrainStorage Create(ServiceProvider services, string name)
        {
            IOptionsSnapshot<FileGrainStorageOptions> optionsSnapshot = 
                services.GetRequiredService<IOptionsSnapshot<FileGrainStorageOptions>>();

            return ActivatorUtilities.CreateInstance<FileGrainStorage>(services, name,
                optionsSnapshot.Get(name), services.GetProviderClusterOptions(name));
        }
    }
}
