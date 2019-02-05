#### 0.2.0 February 05 2019 ####
Add Service Fabric bootstrapper that sets up Akka.Cluster by reading from Service Fabric created environment variables
Update to netstandard2.0
Update to Akka 1.3.11
Update to FluentAssertions 5.4.1
Update Microsoft.NET.Test.Sdk to 15.9.0
Update XUnit to 2.4.1
Fix some spelling errors.

**Akka.Bootstrap.Docker Changes**
- No more dependency on `get-dockerip.sh`. This can now be handled programmatically via the C# inside `Akka.Docker.Bootstrap`.
- If the `CLUSTER_IP` environment variable is not set, Docker will now automatically resolve the hostname via `Dns.GetHostName` inside the application. This change was made in order to remove the `get-dockerip.sh` dependency, which was causing numerous usability problems for end-users.

#### 0.1.3 April 26 2018 ####
* Upgraded to [Akka.NET v1.3.6](https://github.com/akkadotnet/akka.net/releases/tag/v1.3.6)
* Added XML-DOCs to NuGet package output.

#### 0.1.2 April 13 2018 ####
* Completed: [Add flag to see if we're currently running in PCF environment](https://github.com/petabridge/akkadotnet-bootstrap/issues/9)

#### 0.1.1 April 11 2018 ####
Added support for `Akka.Bootstrap.PCF`.

#### 0.1.0 March 20 2018 ####
Initial release of `Akka.Bootstrap.Docker`, which you can read more about here: https://github.com/petabridge/akkadotnet-bootstrap/tree/docker-bootstrap/src/Akka.Bootstrap.Docker