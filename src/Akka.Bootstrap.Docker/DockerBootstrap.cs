// -----------------------------------------------------------------------
// <copyright file="DockerBootstrap.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Net;
using System.Text;
using Akka.Configuration;
using Hocon;

namespace Akka.Bootstrap.Docker
{
    /// <summary>
    ///     Modifies our HOCON configuration based on environment variables
    ///     supplied by Docker.
    /// </summary>
    public static class DockerBootstrap
    {
        private const string DefaultConfigResource = "Akka.Bootstrap.Docker.Docker.Environment.conf";

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
            var environmentConfig = HoconConfigurationFactory.FromResource<AssemblyMarker>(DefaultConfigResource);

            if(!environmentConfig.HasPath("akka.remote.dot-netty.tcp.public-hostname") && assignDefaultHostName)
                Console.WriteLine($"[Docker-Bootstrap] Environment variable CLUSTER_IP was not set." +
                        $"Defaulting to local hostname [{Dns.GetHostName()}] for remote addressing.");

            var defaultValues = new StringBuilder();
            defaultValues.AppendLine("akka.remote.dot-netty.tcp.hostname=0.0.0.0");

            if (assignDefaultHostName)
                defaultValues.AppendLine($"akka.remote.dot-netty.tcp.public-hostname={Dns.GetHostName()}");

            if (environmentConfig.HasPath("environment.seed-nodes"))
                defaultValues.AppendLine(
                    $"akka.cluster.seed-nodes={environmentConfig.GetString("environment.seed-nodes").ToProperHoconArray(true)}");

            var finalConfig = environmentConfig
                .WithEnvironmentFallback()
                .WithFallback(ConfigurationFactory.ParseString(defaultValues.ToString()))
                .WithFallback(input);

            Console.WriteLine($"[Docker-Bootstrap] IP={finalConfig.GetString("akka.remote.dot-netty.tcp.public-hostname")}");

            if(finalConfig.HasPath("akka.remote.dot-netty.tcp.port"))
                Console.WriteLine($"[Docker-Bootstrap] PORT={finalConfig.GetString("akka.remote.dot-netty.tcp.port")}");
            else
                Console.WriteLine($"[Docker-Bootstrap] PORT=0");

            if(finalConfig.HasPath("akka.cluster.seed-nodes"))
                Console.WriteLine($"[Docker-Bootstrap] SEEDS={finalConfig.GetStringList("akka.cluster.seed-nodes")}");
            else
                Console.WriteLine($"[Docker-Bootstrap] SEEDS=[]");

            return finalConfig;
        }
    }
}