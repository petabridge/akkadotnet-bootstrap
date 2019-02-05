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
