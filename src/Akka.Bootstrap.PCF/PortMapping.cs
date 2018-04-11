// -----------------------------------------------------------------------
// <copyright file="PortMapping.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

namespace Akka.Bootstrap.PCF
{
    /// <summary>
    ///     Expresses a mapping between an internal and external port
    /// </summary>
    public sealed class PortMapping
    {
        public PortMapping(int external, int @internal)
        {
            External = external;
            Internal = @internal;
        }

        /// <summary>
        ///     The external port visible to remote parties.
        /// </summary>
        public int External { get; }

        /// <summary>
        ///     The internal port used internally.
        /// </summary>
        public int Internal { get; }

        public override string ToString()
        {
            return $"[External: {External}, Internal: {Internal}]";
        }
    }
}