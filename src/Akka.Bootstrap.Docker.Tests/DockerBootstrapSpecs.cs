using System;
using System.Linq;
using Akka.Configuration;
using FluentAssertions;
using Xunit;

namespace Akka.Bootstrap.Docker.Tests
{
    /// <summary>
    /// Not using the TestKit here, since most of what we're asserting is just <see cref="Config"/> parsing.
    /// </summary>
    public class DockerBootstrapSpecs
    {
        [Fact]
        public void ShouldStartNormallyIfNotEnvironmentVariablesAreSupplied()
        {
            var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
            myConfig.HasPath("akka.cluster.seed-nodes").Should().BeFalse();
            myConfig.HasPath("akka.remote.dot-netty.tcp.hostname").Should().BeFalse();
            myConfig.HasPath("akka.remote.dot-netty.tcp.port").Should().BeFalse();
        }

        [Fact]
        public void ShouldStartIfValidPortIsSupplied()
        {
            try
            {
                Environment.SetEnvironmentVariable("CLUSTER_PORT", "8000", EnvironmentVariableTarget.Process);
                var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
                myConfig.HasPath("akka.remote.dot-netty.tcp.port").Should().BeTrue();
                myConfig.GetInt("akka.remote.dot-netty.tcp.port").Should().Be(8000);
            }
            finally
            {
                // clean the environment variable up afterwards
                Environment.SetEnvironmentVariable("CLUSTER_PORT", null);
            }
            
        }

        [Fact]
        public void ShouldStartIfValidHostnameIsSupplied()
        {
            try
            {
                Environment.SetEnvironmentVariable("CLUSTER_IP", "localhost", EnvironmentVariableTarget.Process);
                var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
                myConfig.HasPath("akka.remote.dot-netty.tcp.public-hostname").Should().BeTrue();
                myConfig.GetString("akka.remote.dot-netty.tcp.public-hostname").Should().Be("localhost");
            }
            finally
            {
                // clean the environment variable up afterwards
                Environment.SetEnvironmentVariable("CLUSTER_IP", null);
            }

        }

        [Theory]
        [InlineData("[]")]
        [InlineData("akka.tcp://MySys@localhost:9140")]
        [InlineData("akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141")]
        [InlineData("akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141, akka.tcp://MySys@localhost:9142")]
        public void ShouldStartIfValidSeedNodesIfSupplied(string seedNodes)
        {
            try
            {
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS", seedNodes, EnvironmentVariableTarget.Process);
                var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
                myConfig.HasPath("akka.cluster.seed-nodes").Should().BeTrue();
                var seeds = myConfig.GetStringList("akka.cluster.seed-nodes");
                seeds.Should().BeEquivalentTo(seedNodes.Split(",").Select(x => x.Trim()));
            }
            finally
            {
                // clean the environment variable up afterwards
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS", null);
            }

        }
    }
}
