using Orleans.TestingHost;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class ClusterFixture : IDisposable
    {
        public TestCluster Cluster { get; private set; }

        public ClusterFixture()
        {
            var builder = new TestClusterBuilder();
            builder.Options.ServiceId = Guid.NewGuid().ToString();
            this.Cluster = builder.Build();
            this.Cluster.Deploy();
        }

        public void Dispose()
        {
            this.Cluster.StopAllSilos();
        }
    }
}
