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

                foreach (var mapKey in ExistingMappings.Keys)
                {
                    if (key.Contains(mapKey))
                    {
                        foreach (var listMap in ExistingMappingLists)
                        {
                            if (key == listMap)
                            {
                                isList = true;
                                break;
                            }
                        }

                        // Format the key to appear as if it were an environment variable
                        // in the "AKKA__" format
                        key = key.Replace(mapKey, ExistingMappings[mapKey])
                            .ToUpperInvariant().Replace(".", "__").Replace("-", "_");
                        break;
                    }
                }

                if (!UseAllEnvironmentVariables)
                    if (!key.StartsWith("AKKA__", StringComparison.OrdinalIgnoreCase))
                        continue;

                // Skip empty environment variables
                var value = set.Value?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                // Ideally, lists should be passed through in an indexed format.
                // However, we can allow for lists to be passed in as array format.
                // Otherwise, we must format the string as an array.
                if (isList)
                {
                    value = value.Trim();

                    string[] values;
                    try
                    {
                        values = new ListParser().Parse(value).ToArray();
                    }
                    catch (Exception e)
                    {
                        throw new ConfigurationException($"Failed to parse list value [{value}]", e);
                    }

                    if (values.Length == 1)
                    {
                        // if the parser only returns a single value,
                        // we might have a quoted environment variable
                        try
                        {
                            values = new ListParser().Parse(values[0]).ToArray();
                        }
                        catch (Exception e)
                        {
                            throw new ConfigurationException($"Failed to parse list value [{values[0]}]", e);
                        }
                    }

                    // always assume that string needs to be quoted
                    value = $"[{string.Join(",", values.Select(s => s.AddQuotes()))}]";
                }
                else
                {
                    value = value.AddQuotesIfNeeded();
                }

                yield return EnvironmentVariableConfigEntrySource.Create(
                    key.ToLowerInvariant(),
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
                    sb.AppendLine($"[{string.Join(",", set.OrderBy(y => y.Index).Select(y => y.Value.AddQuotesIfNeeded()))}]");
                }
                else
                {
                    sb.AppendLine($"{set.First().Value}");
                }
            }

            if(sb.Length == 0)
                return Config.Empty;
            var config = ConfigurationFactory.ParseString(sb.ToString());

            return config;
        }
    }

}