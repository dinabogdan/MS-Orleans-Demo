using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Silo.GrainStorage
{
    public class FileGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly string _storageName;
        private readonly FileGrainStorageOptions _options;
        private readonly ClusterOptions _clusterOptions;
        private readonly IGrainFactory _grainFactory;
        private readonly ITypeResolver _typeResolver;
        private readonly string _rootDirectory;
        private JsonSerializerSettings _jsonSettings;

        public FileGrainStorage(string storageName, FileGrainStorageOptions storageOptions,
            IOptions<ClusterOptions> clusterOptions, IGrainFactory grainFactory, ITypeResolver typeResolver)
        {
            _storageName = storageName;
            _options = storageOptions;
            _clusterOptions = clusterOptions.Value;
            _grainFactory = grainFactory;
            _typeResolver = typeResolver;
            _rootDirectory = storageOptions.RootDirectory;
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var fName = GetKeyString(grainType, grainReference);
            var path = Path.Combine(_options.RootDirectory, fName);

            var fileInfo = new FileInfo(path);
            if(fileInfo.Exists)
            {
                if(fileInfo.LastWriteTimeUtc.ToString() != grainState.ETag)
                {
                    throw new InconsistentStateException($"Version conflict (ClearState): " +
                        $"ServiceId={_clusterOptions.ServiceId} " +
                        $"ProviderName={_storageName} " +
                        $"GrainType={grainType} " +
                        $"GrainReference={grainReference.ToKeyString()}.");
                }

                grainState.ETag = null;
                grainState.State = Activator.CreateInstance(grainState.State.GetType());
                fileInfo.Delete();
            }
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<FileGrainStorage>(_storageName),
                ServiceLifecycleStage.ApplicationServices, Init);
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var fName = GetKeyString(grainType, grainReference);
            var path = Path.Combine(_options.RootDirectory, fName);

            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                grainState.State = Activator.CreateInstance(grainState.State.GetType());
                return;
            }

            using (var stream = fileInfo.OpenText())
            {
                var storedData = await stream.ReadToEndAsync();
                grainState.State = JsonConvert.DeserializeObject(storedData, _jsonSettings);
            }

            grainState.ETag = fileInfo.LastWriteTimeUtc.ToString();
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var storedData = JsonConvert.SerializeObject(grainState.State, _jsonSettings);
            var fName = GetKeyString(grainType, grainReference);
            var path = Path.Combine(_options.RootDirectory, fName);

            var fileInfo = new FileInfo(path);

            if (fileInfo.Exists && fileInfo.LastWriteTimeUtc.ToString() != grainState.ETag)
            {
                throw new InconsistentStateException($"Version conflict (WriteState): " +
                    $"ServiceId={_clusterOptions.ServiceId} " +
                    $"ProviderName={_storageName} " +
                    $"GrainType={grainType} " +
                    $"GrainReference={grainReference.ToKeyString()}.");
            }

            using (var stream = new StreamWriter(fileInfo.Open(FileMode.Create, FileAccess.Write)))
            {
                await stream.WriteAsync(storedData);
            }

            fileInfo.Refresh();
            grainState.ETag = fileInfo.LastWriteTimeUtc.ToString();
        }

        private Task Init(CancellationToken ct)
        {
            _jsonSettings = OrleansJsonSerializer.UpdateSerializerSettings(OrleansJsonSerializer.GetDefaultSerializerSettings(
                _typeResolver, _grainFactory), false, false, null);

            var directory = new System.IO.DirectoryInfo(_rootDirectory);
            if (!directory.Exists) directory.Create();

            return Task.CompletedTask;
        }

        private string GetKeyString(string grainType, GrainReference grainReference)
        {
            return $"{_clusterOptions.ServiceId}.{grainReference.ToKeyString()}.{grainType}";
        }
    }
}
