// -----------------------------------------------------------------------
// <copyright file="PcfEnvironmentSpecs.cs" company="Petabridge, LLC">
//      Copyright (C) 2018 - 2018 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using FluentAssertions;
using Xunit;

namespace Akka.Bootstrap.PCF.Tests
{
    public class PcfEnvironmentSpecs
    {
        [Fact(DisplayName = "Should still be able to populate the PcfEnvironment withour error outside PCF")]
        public void ShouldInitializePcfEnvironmentWithoutErrorsWhenUnpopulated()
        {
            var pcfEnvironment = PcfEnvironment.Init();
            pcfEnvironment.Should().NotBeNull();
            pcfEnvironment.ToString().Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Should report that we are not in a PCF environment")]
        public void ShouldReportWhetherWeAreInPcfOrNot()
        {
            PcfEnvironment.IsRunningPcf.Should().BeFalse();
        }
    }
}