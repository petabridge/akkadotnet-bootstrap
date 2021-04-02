// -----------------------------------------------------------------------
// <copyright file="DockerBootstrap.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text;
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
            var finalConfig =  ConfigurationFactory.Empty.FromEnvironment();
            
            if (assignDefaultHostName)
            {
                finalConfig = finalConfig
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            $@"
                            akka.remote.dot-netty.tcp {{
                                hostname=0.0.0.0
                                public-hostname={Dns.GetHostName()}
                            }}
                        "
                        )
                    );
            }

            finalConfig = finalConfig.WithFallback(input);

            // Diagnostic logging
            Console.WriteLine($"[Docker-Bootstrap] IP={finalConfig.GetString("akka.remote.dot-netty.tcp.public-hostname")}");
            Console.WriteLine($"[Docker-Bootstrap] PORT={finalConfig.GetString("akka.remote.dot-netty.tcp.port")}");
            var seeds = string.Join(",", finalConfig.GetStringList("akka.cluster.seed-nodes").Select(s => $"\"{s}\""));
            Console.WriteLine($"[Docker-Bootstrap] SEEDS=[{seeds}]");

            return finalConfig;
        }
    }
}