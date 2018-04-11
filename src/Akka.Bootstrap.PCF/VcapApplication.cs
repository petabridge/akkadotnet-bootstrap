using System;
using System.Collections.Generic;
using System.Text;
using Akka.Bootstrap.PCF.Serialization;

namespace Akka.Bootstrap.PCF
{
    /// <summary>
    /// Exposes the values from the VCAP_SERVICES PCF environment variable.
    /// </summary>
    /// <remarks>
    ///     See https://docs.run.pivotal.io/devguide/deploy-apps/environment-variable.html for more details.
    /// </remarks>
    public sealed class VcapApplication
    {
        public VcapApplication(string applicationId, string applicationName, IReadOnlyList<string> applicationUris, 
            string applicationVersion, string cfApi, AppResourceLimits limits, string name, 
            string spaceId, string spaceName, IReadOnlyList<string> uris, string version)
        {
            ApplicationId = applicationId;
            ApplicationName = applicationName;
            ApplicationUris = applicationUris;
            ApplicationVersion = applicationVersion;
            CfApi = cfApi;
            Limits = limits;
            Name = name;
            SpaceId = spaceId;
            SpaceName = spaceName;
            Uris = uris;
            Version = version;
        }

        public string ApplicationId { get; }
        public string ApplicationName { get; }
        public IReadOnlyList<string> ApplicationUris { get; }
        public string ApplicationVersion { get; }

        /// <summary>
        /// The URI of the Cloud Foundry host endpoint.
        /// </summary>
        public string CfApi { get; }
        public AppResourceLimits Limits { get; }
        public string Name { get; }
        public string SpaceId { get; }
        public string SpaceName { get; }
        public IReadOnlyList<string> Uris { get; }
        public string Version { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            return ToString(sb);
        }

        public string ToString(StringBuilder sb)
        {
            sb.AppendLine("VCAP_APPLICATION:")
                .AppendLine($"  ApplicationId: {ApplicationId}")
                .AppendLine($"  ApplicationName: {ApplicationName}")
                .AppendLine($"  ApplicationVersion: {ApplicationVersion}")
                .AppendLine("  ApplicationUris:");

            foreach (var uri in ApplicationUris)
            {
                sb.AppendLine("    " + uri);
            }



            sb.AppendLine($"  CfApi: {CfApi}")
                .AppendLine($"  Name: {Name}")
                .AppendLine($"  SpaceId: {SpaceId}")
                .AppendLine($"  SpaceName: {SpaceName}")
                .AppendLine($"  Version: {Version}")
                .AppendLine("  Uris:");

            foreach (var uri in Uris)
            {
                sb.AppendLine("    " + uri);
            }

            return Limits.ToString(sb);
        }
    }

    /// <summary>
    /// Limits on application resources in PCF at the moment when this application was booted.
    /// </summary>
    public sealed class AppResourceLimits
    {
        public AppResourceLimits(int disk, int fds, int mem)
        {
            Disk = disk;
            Fds = fds;
            Mem = mem;
        }

        /// <summary>
        /// Disk consumption limits in megabytes.
        /// </summary>
        public int Disk { get; }

        /// <summary>
        /// The number of allowed file descriptors.
        /// </summary>
        public int Fds { get; }

        /// <summary>
        /// The amount of available memory in megabytes.
        /// </summary>
        public int Mem { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            return ToString(sb);
        }

        public string ToString(StringBuilder sb)
        {
            return sb.AppendLine("Limits:")
                .AppendLine($"  Disk: {Disk}")
                .AppendLine($"  Memory: {Mem}")
                .AppendLine($"  FileDescriptors: {Fds}").ToString();
        }
    }
}
