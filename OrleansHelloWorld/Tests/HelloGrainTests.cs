using GrainInterfaces.Integration;
using Orleans.TestingHost;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class HelloGrainTests
    {
        [Fact]
        public async Task SaysHelloCorrectly()
        {
            var builder = new TestClusterBuilder();
            builder.Options.ServiceId = Guid.NewGuid().ToString();
            var cluster = builder.Build();
            cluster.Deploy();

            var hello = cluster.GrainFactory.GetGrain<IHello>(1);
            var greetingMessage = "Gutten tag";
            var greeting = await hello.SayHello("Gutten tag");

            cluster.StopAllSilos();

            Assert.Equal($"\n Client said: '{greetingMessage}', so HelloGrain says: Hello!", greeting);
        }
    }
}
