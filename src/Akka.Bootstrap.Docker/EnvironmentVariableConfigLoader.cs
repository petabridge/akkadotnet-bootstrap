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

        private static IEnumerable<KeyValuePair<string, string>> GetEnvironmentVariables()
        {
            // Currently, exclude environment variables that do not start with "AKKA__"
            // We can implement variable substitution at a later stage which would allow
            // us to do something like: akka.some-setting=${?HOSTNAME} which can refer 
            // to other non "AKKA__" variables.
            bool UseAllEnvironmentVariables = false;

            foreach (DictionaryEntry set in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
            {
                var key = set.Key.ToString();

                if (!UseAllEnvironmentVariables)
                if (!key.StartsWith("AKKA__", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Skip empty environment variables
                var value = set.Value?.ToString()?.Trim();
                if (string.IsNullOrEmpty(value))
                    continue;

                yield return new KeyValuePair<string, string>(
                    key.Replace("__", ".").Replace("_", "-").ToLower(), 
                    value.ToProperHoconArray());
            }
        }

        // Ideally, lists should be passed through in an indexed format.
        // However, we can allow for lists to be passed in as array format.
        // Otherwise, we must format the string as an array.
        public static string ToProperHoconArray(this string source, bool isArray = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            source = source.Trim();
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            if (source.First() == '[' && source.Last() == ']')
                source = source.Substring(1, source.Length - 2);
            else if (!isArray && (source.First() != '[' || source.Last() != ']'))
                return source.ToSafeHoconString();

            var stringArray = source.Split(',');
            var sb = new StringBuilder("[\n");
            foreach(var value in stringArray)
            {
                sb.AppendLine(value.ToSafeHoconString());
            }
            sb.AppendLine("]");
            return sb.ToString();
        }

        public static string ToSafeHoconString(this string value)
        {
            if (value.NeedQuotes())
                return $"\"{value}\"";
            if (value.NeedTripleQuotes())
                return $"\"\"\"{value}\"\"\"";
            else
                return value;
        }

        public static Config GetConfig()
        {
            var sb = new StringBuilder();
            foreach (var kvp in GetEnvironmentVariables())
                sb.AppendLine($"{kvp.Key}={kvp.Value}");

            return sb.Length == 0 ? Config.Empty : HoconConfigurationFactory.ParseString(sb.ToString());
        }

        public static Config WithEnvironmentFallback(this Config config)
        {
            return config.WithFallback(GetConfig());
        }

        /// <summary>
        /// Load AKKA configuration from the environment variables that are 
        /// accessible from the current process.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Config FromEnvironment(this Config _)
        {
            return GetConfig();
        }
    }

}