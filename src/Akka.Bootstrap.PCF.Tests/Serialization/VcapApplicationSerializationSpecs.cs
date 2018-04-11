// -----------------------------------------------------------------------
// <copyright file="VcapApplicationSerializationSpecs.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using Akka.Bootstrap.PCF.Serialization;
using FluentAssertions;
using Xunit;

namespace Akka.Bootstrap.PCF.Tests
{
    public class VcapApplicationSerializationSpecs
    {
        public static readonly string SampleVcapApplication = @"
              {
                  ""application_id"": ""aaaae2ba-e841-1211-94e6-00a319bc2560"",
                  ""application_name"": ""helloworld"",
                  ""application_uris"": [
                   ""helloworld.cfapps.io""
                  ],
                  ""application_version"": ""eec414fe-4016-40ae-bd70-da2e564c0a90"",
                  ""cf_api"": ""https://api.run.pivotal.io"",
                  ""limits"": {
                   ""disk"": 64,
                   ""fds"": 16384,
                   ""mem"": 64
                  },
                  ""name"": ""helloworld"",
                  ""space_id"": ""aaa0b011-ed21-4934-c1721-b07207b9f9a1"",
                  ""space_name"": ""development"",
                  ""uris"": [
                   ""helloworld.cfapps.io""
                  ],
                  ""users"": null,
                  ""version"": ""eec414fe-4016-40ae-bd70-da2e564c0a90""
                 }";

        [Fact(DisplayName = "Should parse VCAP_APPLICATION environment variable correctly")]
        public void ShouldParseVcapApplication()
        {
            var vcapApp = PcfSerializer.ParseVcapApplication(SampleVcapApplication);

            vcapApp.application_id.Should().Be("aaaae2ba-e841-1211-94e6-00a319bc2560");
            vcapApp.application_name.Should().Be("helloworld");
            vcapApp.application_uris.Count.Should().Be(1);
            vcapApp.application_uris.Single().Should().Be("helloworld.cfapps.io");
            vcapApp.application_version.Should().Be("eec414fe-4016-40ae-bd70-da2e564c0a90");
            vcapApp.cf_api.Should().Be("https://api.run.pivotal.io");

            vcapApp.limits.disk.Should().Be(64);
            vcapApp.limits.fds.Should().Be(16384);
            vcapApp.limits.mem.Should().Be(64);

            vcapApp.name.Should().Be("helloworld");
            vcapApp.space_id.Should().Be("aaa0b011-ed21-4934-c1721-b07207b9f9a1");
            vcapApp.space_name.Should().Be("development");
            vcapApp.uris.Count.Should().Be(1);
            vcapApp.uris.Single().Should().Be("helloworld.cfapps.io");
            vcapApp.version.Should().Be("eec414fe-4016-40ae-bd70-da2e564c0a90");
        }
    }
}