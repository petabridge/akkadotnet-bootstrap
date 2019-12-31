// -----------------------------------------------------------------------
// <copyright file="DockerBootstrap.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Akka.Bootstrap.Docker
{
    /// <summary>
    /// Configuration entry source from Environment Variables
    /// </summary>
    public class EnvironmentVariableConfigEntrySource : ConfigEntrySource
    {
        public override string SourceName { get; }= "environment-variable";

        /// <summary>
        /// Creates an instance of EnvironmentVariableConfigEntrySource
        /// from a raw environment variable key/value pair
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// Will perform analysis on the key in the following way:
        ///     - Instances of '__' are treated as path delimiterrs
        ///     - Instances of '_' are treated as substitutions of '-'
        ///     - Terminal nodes which appear to be integers will be taken as indexes
        ///       to support multi-value keys
        /// </remarks>
        public static EnvironmentVariableConfigEntrySource Create(string key, string value)
        {
            var nodes = key.Split(new [] { "__" }, StringSplitOptions.None)
                .Select(x => x.Replace("_", "-"))
                .ToArray();
            var index = 0;
            var maybeIndex = nodes.Last();
            if (Regex.IsMatch(maybeIndex, @"^\d+$")) {
                nodes = nodes.Take(nodes.Length - 1).ToArray();
                index = int.Parse(maybeIndex);
            }
            return new EnvironmentVariableConfigEntrySource(nodes, value, index);
        } 

        EnvironmentVariableConfigEntrySource(string[] nodes, string value, int index = 0)
            :base(nodes, value, index)
        {
            
        }
    }

}