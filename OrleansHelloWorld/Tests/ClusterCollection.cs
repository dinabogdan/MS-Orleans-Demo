using Xunit;

namespace Tests
{
    [CollectionDefinition(ClusterCollection.Name)]
    class ClusterCollection : ICollectionFixture<ClusterFixture>
    {
        public const string Name = "ClusterCollection";
    }
}
