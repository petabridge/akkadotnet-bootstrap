// -----------------------------------------------------------------------
// <copyright file="PcfSerializer.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace Akka.Bootstrap.PCF.Serialization
{
    /// <summary>
    ///     INTERNAL API.
    /// </summary>
    internal static class PcfSerializer
    {
        public static VcapApplication ParseVcapApplication()
        {
            return ParseVcapApplication(Environment.GetEnvironmentVariable("VCAP_APPLICATION"));
        }

        public static VcapApplication ParseVcapApplication(string vcapApplication)
        {
            return JsonConvert.DeserializeObject<VcapApplication>(vcapApplication);
        }
    }
}