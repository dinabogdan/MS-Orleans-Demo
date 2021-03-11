using GrainInterfaces.Integration;
using Orleans.TestingHost;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    [Collection(ClusterCollection.Name)]
    public class HelloGrainTests: IClassFixture<ClusterFixture>
    {
        private readonly TestCluster _cluster;

        public HelloGrainTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
        }

        [Fact]
        public async Task SaysHelloCorrectly()
        {
            var hello = _cluster.GrainFactory.GetGrain<IHello>(1);
            var greetingMessage = "Bonjour";
            var greeting = await hello.SayHello(greetingMessage);

            Assert.Equal($"\n Client said: '{greetingMessage}', so HelloGrain says: Hello!", greeting);
        }
    }
}
