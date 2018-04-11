// -----------------------------------------------------------------------
// <copyright file="PcfSerializer.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Akka.Bootstrap.PCF.Serialization
{
    /// <summary>
    ///     INTERNAL API.
    /// </summary>
    internal static class PcfSerializer
    {
        public static JsonVcapApplication ParseVcapApplication()
        {
            return ParseVcapApplication(Environment.GetEnvironmentVariable("VCAP_APPLICATION"));
        }

        public static JsonVcapApplication ParseVcapApplication(string vcapApplication)
        {
            return JsonConvert.DeserializeObject<JsonVcapApplication>(vcapApplication);
        }

        public static IReadOnlyList<PortMapping> ParsePcfPorts(string pcfPorts)
        {
            return JsonConvert.DeserializeObject<IEnumerable<PortMapping>>(pcfPorts).ToList();
        }

        public static VcapApplication ToVcapApplication(this JsonVcapApplication jsonVcap)
        {
            var appLimits = new AppResourceLimits(jsonVcap.limits.disk, jsonVcap.limits.fds, jsonVcap.limits.mem);
            return new VcapApplication(jsonVcap.application_id, jsonVcap.application_name, jsonVcap.application_uris ?? new List<string>(), jsonVcap.application_version,
                jsonVcap.cf_api, appLimits, jsonVcap.name, jsonVcap.space_id, jsonVcap.space_name, jsonVcap.uris ?? new List<string>(), jsonVcap.version);
        }
    }
}