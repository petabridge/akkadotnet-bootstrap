// -----------------------------------------------------------------------
// <copyright file="DockerBootstrapSpecs.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Akka.Configuration;
using Hocon;
using FluentAssertions;
using Xunit;

namespace Akka.Bootstrap.ServiceFabric.Tests
{
    /// <summary>
    ///     Not using the TestKit here, since most of what we're asserting is just <see cref="Config" /> parsing.
    /// </summary>
    public class ServiceFabricBootstrapSpecs
    {
        private const string ServiceEndpointName = "MyBootstrapTestEndpoint";

        [Theory]
        [InlineData("[]")]
        [InlineData("akka.tcp://MySys@localhost:9140")]
        [InlineData("akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141")]
        [InlineData("akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141, akka.tcp://MySys@localhost:9142")]
        public void ShouldStartIfValidSeedNodesIfSupplied(string seedNodes)
        {
            var name = "CLUSTER_SEEDS";
            var old = Environment.GetEnvironmentVariable(name);
            try
            {
                Environment.SetEnvironmentVariable(name, seedNodes, EnvironmentVariableTarget.Process);
                var myConfig = ConfigurationFactory.Empty.BootstrapFromServiceFabric(ServiceEndpointName);
                myConfig.HasPath("akka.cluster.seed-nodes").Should().BeTrue();
                var seeds = myConfig.GetStringList("akka.cluster.seed-nodes");
                seeds.Should().BeEquivalentTo(seedNodes.Split(",").Select(x => x.Trim()));
            }
            finally
            {
                // clean the environment variable up afterwards
                Environment.SetEnvironmentVariable(name, old);
            }
        }

        [Theory]
        [InlineData("localhost")]
        [InlineData("127.0.0.1")]
        public void ShouldStartIfValidHostnameIsSupplied(string hostName)
        {
            var name = $"Fabric_Endpoint_IPOrFQDN_{ServiceEndpointName}";
            var old = Environment.GetEnvironmentVariable(name);
            try
            {
                Environment.SetEnvironmentVariable(name, hostName, EnvironmentVariableTarget.Process);
                var myConfig = ConfigurationFactory.Empty.BootstrapFromServiceFabric(ServiceEndpointName);
                myConfig.HasPath("akka.remote.dot-netty.tcp.public-hostname").Should().BeTrue();
                myConfig.GetString("akka.remote.dot-netty.tcp.public-hostname").Should().Be(hostName);
            }
            finally
            {
                // clean the environment variable up afterwards
                Environment.SetEnvironmentVariable(name, old);
            }
        }

        [Fact]
        public void ShouldStartIfValidPortIsSupplied()
        {
            var name = $"Fabric_Endpoint_{ServiceEndpointName}";
            var old = Environment.GetEnvironmentVariable(name);
            try
            {
                Environment.SetEnvironmentVariable(name, "8000", EnvironmentVariableTarget.Process);
                var myConfig = ConfigurationFactory.Empty.BootstrapFromServiceFabric(ServiceEndpointName);
                myConfig.HasPath("akka.remote.dot-netty.tcp.port").Should().BeTrue();
                myConfig.GetInt("akka.remote.dot-netty.tcp.port").Should().Be(8000);
            }
            finally
            {
                // clean the environment variable up afterwards
                Environment.SetEnvironmentVariable(name, old);
            }
        }

        [Fact]
        public void ShouldStartNormallyIfNotEnvironmentVariablesAreSupplied()
        {
            var myConfig = ConfigurationFactory.Empty.BootstrapFromServiceFabric(ServiceEndpointName);
            myConfig.HasPath("akka.cluster.seed-nodes").Should().BeFalse();
            myConfig.HasPath("akka.remote.dot-netty.tcp.hostname").Should().BeFalse();
            myConfig.HasPath("akka.remote.dot-netty.tcp.port").Should().BeFalse();
        }
    }
}