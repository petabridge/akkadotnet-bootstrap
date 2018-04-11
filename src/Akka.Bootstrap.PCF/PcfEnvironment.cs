using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Akka.Bootstrap.PCF.Serialization;

namespace Akka.Bootstrap.PCF
{
    /// <summary>
    /// Populates all of the built-in Pivotal Cloud Foundry environment variables.
    /// 
    /// Some of this data is needed for bootstrapping applications, integrating with built-in
    /// PCF services such as the Metrics Forwarder, and more.
    /// </summary>
    /// <remarks>
    /// Depending on your environment, not all of these values may be populated. 
    /// 
    /// Please see https://docs.run.pivotal.io/devguide/deploy-apps/environment-variable.html for more details.
    /// </remarks>
    public sealed class PcfEnvironment
    {
        private static readonly Lazy<PcfEnvironment> Instance = new Lazy<PcfEnvironment>(Init);

        private static PcfEnvironment Init()
        {
            IPEndPoint cfInstanceAddr = null;
            IPAddress cfInstanceIp = null;
            int? cfInstancePort = null;

            var instanceIp = Environment.GetEnvironmentVariable("CF_INSTANCE_IP");
            var instancePort = Environment.GetEnvironmentVariable("CF_INSTANCE_PORT");

            if (!string.IsNullOrEmpty(instancePort) && !string.IsNullOrEmpty(instanceIp))
            {
                cfInstanceIp = IPAddress.Parse(instanceIp);
                cfInstancePort = int.Parse(instancePort);
                cfInstanceAddr = new IPEndPoint(cfInstanceIp, cfInstancePort.Value);
            }

            var cfInstanceGuid = Environment.GetEnvironmentVariable("CF_INSTANCE_GUID");
            var instanceIndex = Environment.GetEnvironmentVariable("CF_INSTANCE_INDEX");
            int? cfInstanceIndex = null;

            if (!string.IsNullOrEmpty(instanceIndex) && int.TryParse(instanceIndex, out var index))
            {
                cfInstanceIndex = index;
            }

            var cfHome = Environment.GetEnvironmentVariable("HOME");
            var lang = Environment.GetEnvironmentVariable("LANG");
            var pwd = Environment.GetEnvironmentVariable("PWD");
            var tmpdir = Environment.GetEnvironmentVariable("TMPDIR");
            var user = Environment.GetEnvironmentVariable("USER");

            var strPortMappings = Environment.GetEnvironmentVariable("CF_INSTANCE_PORTS");
            IReadOnlyList<PortMapping> mappings = new List<PortMapping>();
            if (!string.IsNullOrEmpty(strPortMappings))
            {
                mappings = PcfSerializer.ParsePcfPorts(strPortMappings);
            }

            var jsonVcapApplication = PcfSerializer
                .ParseVcapApplication(Environment.GetEnvironmentVariable("VCAP_APPLICATION")).ToVcapApplication();

            var strPort = Environment.GetEnvironmentVariable("PORT");
            int? port = null;

            if (!string.IsNullOrEmpty(strPort) && int.TryParse(strPort, out var intPort))
            {
                port = intPort;
            }

            return new PcfEnvironment(cfInstanceAddr, cfInstanceGuid, cfInstanceIndex, cfInstanceIp, cfInstancePort,
                mappings, cfHome, lang, port, pwd, tmpdir, user, jsonVcapApplication);
        }

        private PcfEnvironment(IPEndPoint cfInstanceAddr, string cfInstanceGuid, int? cfInstanceIndex,
            IPAddress cfInstanceIp, int? cfInstancePort, IReadOnlyList<PortMapping> cfInstancePorts,
            string home, string lang, int? port, string pwd, string tmpdir, string user,
            VcapApplication vcapApplication)
        {
            CF_INSTANCE_ADDR = cfInstanceAddr;
            CF_INSTANCE_GUID = cfInstanceGuid;
            CF_INSTANCE_INDEX = cfInstanceIndex;
            CF_INSTANCE_IP = cfInstanceIp;
            CF_INSTANCE_PORT = cfInstancePort;
            CF_INSTANCE_PORTS = cfInstancePorts;
            HOME = home;
            LANG = lang;
            PORT = port;
            PWD = pwd;
            TMPDIR = tmpdir;
            USER = user;
            VCAP_APPLICATION = vcapApplication;
        }

        /// <summary>
        /// The IP and Port of the app instance.
        /// </summary>
        public IPEndPoint CF_INSTANCE_ADDR { get; }

        /// <summary>
        /// The UUID of this app instance.
        /// </summary>
        public string CF_INSTANCE_GUID { get; }

        /// <summary>
        /// The index number of the app instance.
        /// </summary>
        public int? CF_INSTANCE_INDEX { get; }

        /// <summary>
        /// The IP of this instance.
        /// </summary>
        /// <remarks>
        /// Already included along with the <see cref="CF_INSTANCE_PORT"/> inside the <see cref="CF_INSTANCE_ADDR"/> value.
        /// </remarks>
        public IPAddress CF_INSTANCE_IP { get; }

        /// <summary>
        /// The external, or host-side, port corresponding to the internal, or container-side, port with value <see cref="PORT"/>. 
        /// 
        /// This value is generally different from the <see cref="PORT"/> of the app instance.
        /// </summary>
        /// <remarks>
        /// Already included along with the <see cref="CF_INSTANCE_IP"/> inside the <see cref="CF_INSTANCE_ADDR"/> value.
        /// </remarks>
        public int? CF_INSTANCE_PORT { get; }

        /// <summary>
        /// The list of mappings between internal, or container-side, and external, or host-side, ports allocated to the instance’s container. 
        /// Not all of the internal ports are necessarily available for the application to bind to, as some of them may be used by 
        /// system-provided services that also run inside the container. These internal and external values may differ.
        /// </summary>
        public IReadOnlyList<PortMapping> CF_INSTANCE_PORTS { get; }

        /// <summary>
        /// The root folder on disk for the currently deployed application.
        /// </summary>
        /// <example>
        /// /home/vcap/app
        /// </example>
        public string HOME { get; }

        /// <summary>
        /// LANG is required by buildpacks to ensure consistent script load order.
        /// </summary>
        public string LANG { get; }

        /// <summary>
        /// The port on which the application should listen for requests.
        /// </summary>
        /// <remarks>
        /// The Cloud Foundry runtime allocates a port dynamically for each instance of the 
        /// application, so code that obtains or uses the app port should refer to it using 
        /// the PORT environment variable.
        /// </remarks>
        public int? PORT { get; }

        /// <summary>
        /// The present working directory of the running application.
        /// </summary>
        /// <example>
        /// /home/vcap/app
        /// </example>
        public string PWD { get; }

        /// <summary>
        /// The temporary file directory where temp and staging files are stored.
        /// </summary>
        /// <example>
        /// /home/vcap/tmp
        /// </example>
        public string TMPDIR { get; }

        /// <summary>
        /// The user account under which the application runs.
        /// </summary>
        /// <example>
        /// vcap
        /// </example>
        public string USER { get; }

        /// <summary>
        /// The parsed, immutable value of the VCAP_APPLICATION environment variable.
        /// </summary>
        public VcapApplication VCAP_APPLICATION { get; }
    }
}
