// -----------------------------------------------------------------------
// <copyright file="DockerBootstrapSpecs.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using Akka.Configuration;
using FluentAssertions;
using Xunit;

namespace Akka.Bootstrap.Docker.Tests
{
    /// <summary>
    ///     Not using the TestKit here, since most of what we're asserting is just <see cref="Config" /> parsing.
    /// </summary>
    public class DockerBootstrapSpecs
    {
        [Theory]
        [InlineData("akka.tcp://MySys@localhost:9140")]
        [InlineData("akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141")]
        [InlineData("akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141, akka.tcp://MySys@localhost:9142")]
        // The whole line is quoted
        [InlineData("\"akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141, akka.tcp://MySys@localhost:9142\"")]
        // The whole line is quoted with arbitrary whitespaces
        [InlineData("   \"akka.tcp://MySys@localhost:9140,  akka.tcp://MySys@localhost:9141,   akka.tcp://MySys@localhost:9142 \"  ")]
        // Every item is quoted
        [InlineData("\"akka.tcp://MySys@localhost:9140\", \"akka.tcp://MySys@localhost:9141\", \"akka.tcp://MySys@localhost:9142\"")]
        // Only one item is quoted
        [InlineData("\"akka.tcp://MySys@localhost:9140\", akka.tcp://MySys@localhost:9141, akka.tcp://MySys@localhost:9142")]
        // Only one item is quoted
        [InlineData("akka.tcp://MySys@localhost:9140, \"akka.tcp://MySys@localhost:9141\", akka.tcp://MySys@localhost:9142")]
        // Only one item is quoted
        [InlineData("akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141, \"akka.tcp://MySys@localhost:9142\"")]
        public void ShouldStartIfValidSeedNodesIfSupplied(string seedNodes)
        {
            try
            {
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS", seedNodes, EnvironmentVariableTarget.Process);
                var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
                myConfig.HasPath("akka.cluster.seed-nodes").Should().BeTrue();
                var seeds = myConfig.GetStringList("akka.cluster.seed-nodes").Select(x => x.Trim());
                seeds.Should().BeEquivalentTo(seedNodes.Replace("\"", "").Split(",").Select(x => x.Trim()));
            }
            finally
            {
                // clean the environment variable up afterwards
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS", null);
            }
        }

        [Theory]
        [InlineData(
            "akka.tcp://MySys@localhost:9140", 
            new[]{"akka.tcp://MySys@localhost:9140"})]
        [InlineData(
            "akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141", 
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141"})]
        [InlineData(
            "akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141, akka.tcp://MySys@localhost:9142",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // The whole line is quoted
        [InlineData(
            "\"akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141, akka.tcp://MySys@localhost:9142\"",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // The whole line is quoted with arbitrary whitespaces
        [InlineData(
            "   \"akka.tcp://MySys@localhost:9140,  akka.tcp://MySys@localhost:9141,   akka.tcp://MySys@localhost:9142 \"  ",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // Every item is quoted
        [InlineData(
            "\"akka.tcp://MySys@localhost:9140\", \"akka.tcp://MySys@localhost:9141\", \"akka.tcp://MySys@localhost:9142\"",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // Only one item is quoted
        [InlineData(
            "\"akka.tcp://MySys@localhost:9140\", akka.tcp://MySys@localhost:9141, akka.tcp://MySys@localhost:9142",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // Only one item is quoted
        [InlineData(
            "akka.tcp://MySys@localhost:9140, \"akka.tcp://MySys@localhost:9141\", akka.tcp://MySys@localhost:9142",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // Only one item is quoted
        [InlineData(
            "akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141, \"akka.tcp://MySys@localhost:9142\"",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        public void ShouldStartIfValidSeedNodesIfSupplied_Hardcoded(string seedNodes, string[] expected)
        {
            try
            {
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS", seedNodes, EnvironmentVariableTarget.Process);
                var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
                myConfig.HasPath("akka.cluster.seed-nodes").Should().BeTrue();
                var seeds = myConfig.GetStringList("akka.cluster.seed-nodes").Select(x => x.Trim());
                seeds.Should().BeEquivalentTo(expected);
            }
            finally
            {
                // clean the environment variable up afterwards
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS", null);
            }
        }

        [Theory]
        [InlineData("[]", new string[0])]
        [InlineData(
            "[akka.tcp://MySys@localhost:9140]", 
            new[]{"akka.tcp://MySys@localhost:9140"})]
        [InlineData(
            "[akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141]", 
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141"})]
        [InlineData(
            "[akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141, akka.tcp://MySys@localhost:9142]",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // The whole line is quoted
        [InlineData(
            "[\"akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141, akka.tcp://MySys@localhost:9142\"]",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // The whole line is quoted with arbitrary whitespaces
        [InlineData(
            "[   \"akka.tcp://MySys@localhost:9140,  akka.tcp://MySys@localhost:9141,   akka.tcp://MySys@localhost:9142 \"  ]",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // Every item is quoted
        [InlineData(
            "[\"akka.tcp://MySys@localhost:9140\", \"akka.tcp://MySys@localhost:9141\", \"akka.tcp://MySys@localhost:9142\"]",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // Only one item is quoted
        [InlineData(
            "[\"akka.tcp://MySys@localhost:9140\", akka.tcp://MySys@localhost:9141, akka.tcp://MySys@localhost:9142]",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // Only one item is quoted
        [InlineData(
            "[akka.tcp://MySys@localhost:9140, \"akka.tcp://MySys@localhost:9141\", akka.tcp://MySys@localhost:9142]",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        // Only one item is quoted
        [InlineData(
            "[akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141, \"akka.tcp://MySys@localhost:9142\"]",
            new[]{"akka.tcp://MySys@localhost:9140", 
                "akka.tcp://MySys@localhost:9141", 
                "akka.tcp://MySys@localhost:9142"})]
        public void ShouldStartIfValidSeedNodesIfSuppliedInArrayFormat(string seedNodes, string[] expected)
        {
            try
            {
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS", seedNodes, EnvironmentVariableTarget.Process);
                var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
                myConfig.HasPath("akka.cluster.seed-nodes").Should().BeTrue();
                var seeds = myConfig.GetStringList("akka.cluster.seed-nodes").Select(x => x.Trim());
                seeds.Should().BeEquivalentTo(expected);
            }
            finally
            {
                // clean the environment variable up afterwards
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS", null);
            }
        }

        [Fact]
        public void ShouldStartIfValidSeedNodesIsSuppliedInIndexedFormat()
        {
            try
            {
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS__1", "akka.tcp://MySys@localhost:9140", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS__2", "\"akka.tcp://MySys@localhost:9141\"", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS__3", "akka.tcp://MySys@localhost:9142", EnvironmentVariableTarget.Process);
                var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
                myConfig.HasPath("akka.cluster.seed-nodes").Should().BeTrue();
                var seeds = myConfig.GetStringList("akka.cluster.seed-nodes").Select(x => x.Trim());
                var expected = new[]
                {
                    "akka.tcp://MySys@localhost:9140",
                    "akka.tcp://MySys@localhost:9141",
                    "akka.tcp://MySys@localhost:9142",
                };
                seeds.Should().BeEquivalentTo(expected);
            }
            finally
            {
                // clean the environment variable up afterwards
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS", null);
            }
        }

        [Theory]
        [InlineData("localhost")]
        [InlineData("127.0.0.1")]
        public void ShouldStartIfValidHostnameIsSupplied(string hostName)
        {
            try
            {
                Environment.SetEnvironmentVariable("CLUSTER_IP", hostName, EnvironmentVariableTarget.Process);
                var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
                myConfig.HasPath("akka.remote.dot-netty.tcp.public-hostname").Should().BeTrue();
                myConfig.GetString("akka.remote.dot-netty.tcp.public-hostname").Should().Be(hostName);
            }
            finally
            {
                // clean the environment variable up afterwards
                Environment.SetEnvironmentVariable("CLUSTER_IP", null);
            }
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
        public void ShouldStartNormallyIfNotEnvironmentVariablesAreSupplied()
        {
            var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
            myConfig.HasPath("akka.cluster.seed-nodes").Should().BeFalse();
            myConfig.GetString("akka.remote.dot-netty.tcp.public-hostname").Should().Be(Dns.GetHostName());
            myConfig.HasPath("akka.remote.dot-netty.tcp.port").Should().BeFalse();
        }
        
        [Fact]
        public void ShouldStartIfValidAkkaConfigurationSuppliedByEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("AKKA__COORDINATED_SHUTDOWN__EXIT_CLR", "on", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("AKKA__ACTOR__PROVIDER", "cluster", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("AKKA__REMOTE__DOT_NETTY__TCP__HOSTNAME", "127.0.0.1", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("AKKA__REMOTE__DOT_NETTY__TCP__PUBLIC_HOSTNAME", "example.local", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("AKKA__REMOTE__DOT_NETTY__TCP__PORT", "2559", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("AKKA__CLUSTER__ROLES__0", "demo", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("AKKA__CLUSTER__ROLES__1", "test", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("AKKA__CLUSTER__ROLES__2", "backup", EnvironmentVariableTarget.Process);

            var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
            
            myConfig.GetBoolean("akka.coordinated-shutdown.exit-clr").Should().BeTrue();
            myConfig.GetString("akka.actor.provider").Should().Be("cluster");
            myConfig.GetString("akka.remote.dot-netty.tcp.hostname").Should().Be("127.0.0.1");
            myConfig.GetString("akka.remote.dot-netty.tcp.public-hostname").Should().Be("example.local");
            myConfig.GetInt("akka.remote.dot-netty.tcp.port").Should().Be(2559);
            myConfig.GetStringList("akka.cluster.roles").Should().BeEquivalentTo(new [] { "demo", "test", "backup" });
        }
		
        [Fact]
        public void ShouldNotAssignDefaultHostNameIfAssignDefaultHostNameParamIsFalse()
        {
            var hostname = "127.0.0.1";
            var publicHostName = "example.local";
            
            var myConfig = ConfigurationFactory.Empty
                .WithFallback(ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.hostname={hostname}"))
                .WithFallback(ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.public-hostname={publicHostName}"))
                .BootstrapFromDocker(assignDefaultHostName: false);
            
            myConfig.GetString("akka.remote.dot-netty.tcp.hostname").Should().Be(hostname);
            myConfig.GetString("akka.remote.dot-netty.tcp.public-hostname").Should().Be(publicHostName);
        }
		
        [Fact]
        public void ShouldGenerateValidHoconConfigWithComplexEnvironmentValue()
        {
            try
            {
                Environment.SetEnvironmentVariable("AKKA__CLUSTER__DOWNING_PROVIDER_CLASS", "Akka.Cluster.SplitBrainResolver, Akka.Cluster", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS", "akka.tcp://MySys@localhost:9140, akka.tcp://MySys@localhost:9141", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("AKKA__RANDOM__URL", "akka.tcp://MySys@localhost:9140", EnvironmentVariableTarget.Process);

                var myConfig = ConfigurationFactory.Empty.BootstrapFromDocker();
                myConfig.GetString("akka.cluster.downing-provider-class").Should().Be("Akka.Cluster.SplitBrainResolver, Akka.Cluster");
                myConfig.GetString("akka.random.url").Should().Be("akka.tcp://MySys@localhost:9140");
                var expected = new[]
                {
                    "akka.tcp://MySys@localhost:9140",
                    "akka.tcp://MySys@localhost:9141"
                };
                myConfig.HasPath("akka.cluster.seed-nodes").Should().BeTrue();
                myConfig.GetStringList("akka.cluster.seed-nodes").Should().BeEquivalentTo(expected);
            }
            finally
            {
                Environment.SetEnvironmentVariable("AKKA__RANDOM__URL", null);
                Environment.SetEnvironmentVariable("AKKA__CLUSTER__DOWNING_PROVIDER_CLASS", null);
                Environment.SetEnvironmentVariable("CLUSTER_SEEDS", null);
            }
        }
    }
}