// -----------------------------------------------------------------------
// <copyright file="DockerBootstrap.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using Akka.Configuration;

namespace Akka.Bootstrap.Docker
{
    /// <summary>
    ///     Modifies our HOCON configuration based on environment variables
    ///     supplied by Docker.
    /// </summary>
    public static class DockerBootstrap
    {
        /// <summary>
        ///     Extension method intended to chain configuration derived from
        ///     Docker-supplied environment variables to the front of the fallback chain,
        ///     overriding any values that were provided in a built-in HOCON file.
        /// </summary>
        /// <param name="input">The current configuration object.</param>
        /// <param name="assignDefaultHostName">If set to <c>true</c>, bootstrapper will use <see cref="Dns.GetHostName"/> to assign a default hostname
        /// in the event that the CLUSTER_IP environment variable is not specified. If <c>false</c>, then we'll leave it blank.</param>
        /// <returns>An updated Config object with <see cref="input" /> chained behind it as a fallback. Immutable.</returns>
        /// <example>
        ///     var config = HoconLoader.FromFile("myHocon.hocon");
        ///     var myActorSystem = ActorSystem.Create("mySys", config.BootstrapFromDocker());
        /// </example>
        public static Config BootstrapFromDocker(this Config input, bool assignDefaultHostName = true)
        {
            /*
             * Trim any leading or trailing whitespace since that can cause problems
             * with the URI / IP parsing that happens in the next stage
             */
            var clusterIp = Environment.GetEnvironmentVariable("CLUSTER_IP")?.Trim();
            var clusterPort = Environment.GetEnvironmentVariable("CLUSTER_PORT")?.Trim();
            var clusterSeeds = Environment.GetEnvironmentVariable("CLUSTER_SEEDS")?.Trim();

            if (string.IsNullOrEmpty(clusterIp) && assignDefaultHostName)
            {
                clusterIp = Dns.GetHostName();
                Console.WriteLine($"[Docker-Bootstrap] Environment variable CLUSTER_IP was not set." +
                                  $"Defaulting to local hostname [{clusterIp}] for remote addressing.");
            }

            // Don't have access to Akka.NET ILoggingAdapter yet, since ActorSystem isn't started.
            Console.WriteLine($"[Docker-Bootstrap] IP={clusterIp}");
            Console.WriteLine($"[Docker-Bootstrap] PORT={clusterPort}");
            Console.WriteLine($"[Docker-Bootstrap] SEEDS={clusterSeeds}");


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
                var seeds = clusterSeeds.Split(',');
                var injectedClusterConfigString = seeds.Aggregate("akka.cluster.seed-nodes = [",
                    (current, seed) => current + @"""" + seed + @""", ");
                injectedClusterConfigString += "]";
                input = ConfigurationFactory.ParseString(injectedClusterConfigString)
                    .WithFallback(input);
            }

            return input;
        }
    }
}