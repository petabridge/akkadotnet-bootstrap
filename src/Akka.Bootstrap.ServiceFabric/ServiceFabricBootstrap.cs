// -----------------------------------------------------------------------
// <copyright file="ServiceFabricBootstrap.cs" company="Petabridge, LLC">
// Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Akka.Configuration;

namespace Akka.Bootstrap.ServiceFabric
{
    /// <summary>
    /// Modifies our HOCON configuration based on environment variables supplied by Service Fabric.
    /// </summary>
    public static class ServiceFabricBootstrap
    {
        /// <summary>
        /// Extension method intended to chain configuration derived from Service Fabric-supplied environment variables to the front
        /// of the fallback chain, overriding any values that were provided in a built-in HOCON file.
        /// </summary>
        /// <param name="input">The current configuration object.</param>
        /// <param name="serviceEndpointName">Name of the service endpoint as defined in the service manifest.</param>
        /// <returns>An updated Config object with <see cref="input"/> chained behind it as a fallback. Immutable.</returns>
        /// <example>var config = HoconLoader.FromFile("myHocon.hocon"); var myActorSystem = ActorSystem.Create("mySys", config.BootstrapFromServiceFabric());</example>
        public static Config BootstrapFromServiceFabric(this Config input, string serviceEndpointName)
        {
            /*
             * Trim any leading or trailing whitespace since that can cause problems
             * with the URI / IP parsing that happens in the next stage
             */
            var clusterIp = Environment.GetEnvironmentVariable($"Fabric_Endpoint_IPOrFQDN_{serviceEndpointName}")?.Trim();
            var clusterPort = Environment.GetEnvironmentVariable($"Fabric_Endpoint_{serviceEndpointName}")?.Trim();
            var clusterSeeds = Environment.GetEnvironmentVariable("CLUSTER_SEEDS")?.Trim();

            // Don't have access to Akka.NET ILoggingAdapter yet, since ActorSystem isn't started.
            Console.WriteLine($"[ServiceFabric-Bootstrap] IP={clusterIp}");
            Console.WriteLine($"[ServiceFabric-Bootstrap] PORT={clusterPort}");
            Console.WriteLine($"[ServiceFabric-Bootstrap] SEEDS={clusterSeeds ?? string.Empty}");

            if (!string.IsNullOrEmpty(clusterIp))
                input = ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.hostname=0.0.0.0" +
                                                         Environment.NewLine +
                                                         "akka.remote.dot-netty.tcp.public-hostname=" + clusterIp)
                    .WithFallback(input);

            if (!string.IsNullOrEmpty(clusterPort) && int.TryParse(clusterPort, out var portNum))
                input = ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + portNum)
                    .WithFallback(input);

            if (!string.IsNullOrEmpty(clusterSeeds))
            {
                var seeds = clusterSeeds.Split(',')
                    .Where(seed => !string.IsNullOrWhiteSpace(seed))
                    .Select(seed => $"\"{seed.Trim()}\"");
                var injectedClusterConfigString = $"akka.cluster.seed-nodes = [{string.Join(",", seeds)}]";
                input = ConfigurationFactory.ParseString(injectedClusterConfigString)
                    .WithFallback(input);
            }

            return input;
        }
    }
}
