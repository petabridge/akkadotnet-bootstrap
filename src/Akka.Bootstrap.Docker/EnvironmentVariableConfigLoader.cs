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

namespace Akka.Bootstrap.Docker
{
    /// <summary>
    /// Extension and helper methods to support loading a Config instance from 
    /// the environment variables of the current process.
    /// </summary>
    public static class EnvironmentVariableConfigLoader
    {
        private static IEnumerable<EnvironmentVariableConfigEntrySource> GetEnvironmentVariables()
        {
            // Currently, exclude environment variables that do not start with "AKKA__"
            // We can implement variable substitution at a later stage which would allow
            // us to do something like: akka.some-setting=${?HOSTNAME} which can refer 
            // to other non "AKKA__" variables.
            bool UseAllEnvironmentVariables = false;

            // List of environment variable mappings that do not follow the "AKKA__" convention.
            // We are currently supporting these out of convenience, and may choose to officially
            // create a set of aliases in the future.  Doing so would allow envvar configuration
            // to be less verbose but might perpetuate confusion as to source of truth for keys.
            Dictionary<string, string> ExistingMappings = new Dictionary<string, string>() 
            {
                { "CLUSTER_IP", "akka.remote.dot-netty.tcp.public-hostname" },
                { "CLUSTER_PORT", "akka.remote.dot-netty.tcp.port" },
                { "CLUSTER_SEEDS", "akka.cluster.seed-nodes" }
            };

            // Identify environment variable mappings that are expected to be lists
            string[] ExistingMappingLists = new string[] { "CLUSTER_SEEDS" };

            foreach (DictionaryEntry set in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
            {
                var key = set.Key.ToString();
                var isList = false;

                if (ExistingMappings.TryGetValue(key, out var mappedKey))
                {
                    isList = ExistingMappingLists.Contains(key);

                    // Format the key to appear as if it were an environment variable
                    // in the "AKKA__" format
                    key = mappedKey.ToUpper().Replace(".", "__").Replace("-", "_");
                }

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
        public static Config FromEnvironment(this Config input)
        {
            var entries = GetEnvironmentVariables()
                .OrderByDescending(x => x.Depth)
                .GroupBy(x => x.Key);

            StringBuilder sb = new StringBuilder();
            foreach (var set in entries)
            {
                sb.Append($"{set.Key}=");
                if (set.Count() > 1)
                {   
                    sb.Append($"[{String.Join(",", set.OrderBy(y => y.Index).Select(y => y.Value))}]");
                }
                else
                {
                    sb.Append($"{set.First().Value}");
                }
            }

            var config = ConfigurationFactory.ParseString(sb.ToString());

            return config;
        }
    }

}