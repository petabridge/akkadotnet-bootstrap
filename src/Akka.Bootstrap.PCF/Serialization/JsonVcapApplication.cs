// -----------------------------------------------------------------------
// <copyright file="JsonVcapApplication.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Akka.Bootstrap.PCF.Serialization
{
    /// <summary>
    ///     INTERNAL API.
    ///     JSON representation of the application resource limits in PCF.
    /// </summary>
    public class JsonLimits
    {
        public int disk { get; set; }
        public int fds { get; set; }
        public int mem { get; set; }
    }

    /// <summary>
    ///     INTERNAL API.
    ///     JSON representation of the VCAP_APPLICATION environment variable.
    /// </summary>
    /// <remarks>
    ///     See https://docs.run.pivotal.io/devguide/deploy-apps/environment-variable.html for more details.
    /// </remarks>
    public class JsonVcapApplication
    {
        public string application_id { get; set; }
        public string application_name { get; set; }
        public List<string> application_uris { get; set; }
        public string application_version { get; set; }
        public string cf_api { get; set; }
        public JsonLimits limits { get; set; }
        public string name { get; set; }
        public string space_id { get; set; }
        public string space_name { get; set; }
        public List<string> uris { get; set; }
        public object users { get; set; }
        public string version { get; set; }
    }
}