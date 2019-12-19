// -----------------------------------------------------------------------
// <copyright file="DockerBootstrap.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Akka.Bootstrap.Docker
{
    /// <summary>
    /// Defines a source of configuration to be evaluated and applied
    /// against a HOCON key/value pair.
    /// </summary>
    public abstract class ConfigEntrySource
    {
        /// <summary>
        /// Override to describe the implementation type
        /// </summary>
        /// <value></value>
        public abstract string SourceName { get; }

        /// <summary>
        /// The series of key nodes which make up the path
        /// </summary>
        /// <value></value>
        public string[] Nodes { get; }
        /// <summary>
        /// The full HOCON path for the given value (Derived from `Nodes`)
        /// </summary>
        /// <value></value>
        public string Key { get; }
        /// <summary>
        /// The value for this given HOCON node
        /// </summary>
        /// <value></value>
        public string Value { get; }
        /// <summary>
        /// Identifies if the source is a series of values
        /// </summary>
        public int Index { get; }
        /// <summary>
        /// Returns the depth of the hocon key (ie. number of nodes on the key)
        /// </summary>
        public int Depth => Nodes.Length;

        /// <summary>
        /// Creates a config entry source from a set of nodes, value and optional index
        /// </summary>
        /// <param name="nodes">Set of nodes which comprise the path</param>
        /// <param name="value">Value stored in this entry</param>
        /// <param name="index">Provided index to identify a set of hocon values stored 
        /// against a single key</param>
        protected ConfigEntrySource(string[] nodes, string value, int index = 0)
        {
            Nodes = nodes;
            Key = String.Join(".", Nodes);
            Index = index;
            Value = value;
        }

    }

}