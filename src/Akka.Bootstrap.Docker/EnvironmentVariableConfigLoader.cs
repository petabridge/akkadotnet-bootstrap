// -----------------------------------------------------------------------
// <copyright file="DockerBootstrap.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Configuration;
using System.Net;
using Hocon;

namespace Akka.Bootstrap.Docker
{
    /// <summary>
    /// Extension and helper methods to support loading a Config instance from 
    /// the environment variables of the current process.
    /// </summary>
    public static class EnvironmentVariableConfigLoader
    {
        private const string DefaultConfigResource = "Akka.Bootstrap.Docker.Docker.Environment.conf";

        private static IEnumerable<EnvironmentVariableConfigEntrySource> GetEnvironmentVariables()
        {
            // Currently, exclude environment variables that do not start with "AKKA__"
            // We can implement variable substitution at a later stage which would allow
            // us to do something like: akka.some-setting=${?HOSTNAME} which can refer 
            // to other non "AKKA__" variables.
            bool UseAllEnvironmentVariables = false;

            foreach (DictionaryEntry set in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
            {
                var key = set.Key.ToString();
                var isList = false;

                if (!UseAllEnvironmentVariables)
                if (!key.StartsWith("AKKA__", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Skip empty environment variables
                var value = set.Value?.ToString()?.Trim();
                if (string.IsNullOrEmpty(value))
                    continue;

                // Ideally, lists should be passed through in an indexed format.
                // However, we can allow for lists to be passed in as array format.
                // Otherwise, we must format the string as an array.
                if (isList)
                {
                    if (value.First() != '[' || value.Last() != ']') 
                    {
                        var values = value.Split(',').Select(x => x.Trim());
                        value = $"[\" {String.Join("\",\"", values)} \"]";
                    } 
                    else if (String.IsNullOrEmpty(value.Substring(1, value.Length - 2).Trim()))
                    {
                        value = "[]";
                    }
                }

                yield return EnvironmentVariableConfigEntrySource.Create(
                    key.ToLower().ToString(), 
                    value
                );
            }
        }
        
        /// <summary>
        /// Load AKKA configuration from the environment variables that are 
        /// accessible from the current process.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Config FromEnvironment(this Config _)
        {
            var environmentConfig = HoconConfigurationFactory.FromResource<AssemblyMarker>(DefaultConfigResource);
            var defaultValues = new StringBuilder();
            defaultValues.AppendLine($@"
                            akka.remote.dot-netty.tcp {{
                                hostname=0.0.0.0
                                public-hostname={Dns.GetHostName()}
                            }}");
            if (environmentConfig.HasPath("environment.seed-nodes"))
                defaultValues.AppendLine($"akka.cluster.seed-nodes=[{environmentConfig.GetString("environment.seed-nodes")}]");

            var entries = GetEnvironmentVariables()
                .OrderByDescending(x => x.Depth)
                .GroupBy(x => x.Key);

            foreach (var set in entries)
            {
                defaultValues.Append($"{set.Key}=");
                if (set.Count() > 1)
                {
                    defaultValues.AppendLine($"[\n\t\"{String.Join("\",\n\t\"", set.OrderBy(y => y.Index).Select(y => y.Value.Trim()))}\"]");
                }
                else
                {
                    defaultValues.AppendLine($"{set.First().Value}");
                }
            }

            return environmentConfig.WithFallback(HoconConfigurationFactory.ParseString(defaultValues.ToString()));
        }
    }

}