# Akka.Bootstrap.Docker

This project is intended to give Akka.NET users the tools they need to begin building Docker base images on top of Akka.NET.

This library works with any runtime supported by Akka.NET.

## Bootstrapping your Akka.NET Applications with Docker
`Akka.Bootstrap.Docker` depends on having some standardized environment variables made available inside each of your Docker containers:

* `CLUSTER_IP` - this value will replace the `akka.remote.dot-netty.tcp.public-hostname` at runtime. If this value is not provided, we will use `Dns.GetHostname()` instead.
* `CLUSTER_PORT` - the port number that will be used by Akka.Remote for inbound connections.
* `CLUSTER_SEEDS` - a comma-delimited list of seed node addressed used by Akka.Cluster.

### Using `Akka.Bootstrap.Docker`
The `Akka.Bootstrap.Docker` NuGet package itself is pretty simple - all it does is expose the `DockerBootstrap` class which gives you the ability to automatically load all of the environment variables we pass in via Docker into your Akka.Remote and Akka.Cluster configuration:

```csharp
var config = HoconLoader.FromFile("myHocon.hocon");
var myActorSystem = ActorSystem.Create("mySys", config.BootstrapFromDocker());
```

The `config.BootstrapFromDocker()` line will do the heavy lifting.

Once this call is finished, all you have to do is launch your `ActorSystem` via `ActorSystem.Create` and everything will run automatically from that point onward.

### Building Docker Images with Akka.NET

> N.B. If you want a full, end-to-end example of how to use `Akka.Bootstrap.Docker` on both Windows and Linux containers, please [see the Akka.Bootstrap.Docker.Sample folder](../Akka.Bootstrap.Docker.Sample).

The first step in building an effective Akka.NET image is to define a Dockerfile, which you will use to create your Docker images. Here's an example of a minimal (but effective) Dockerfile that uses .NET Core 2.1, Akka.Cluster, and [Petabridge.Cmd](https://cmd.petabridge.com/) 

```
FROM mcr.microsoft.com/dotnet/core/runtime:2.1 AS base
WORKDIR /app

# should be a comma-delimited list
ENV CLUSTER_SEEDS "[]"
ENV CLUSTER_IP ""
ENV CLUSTER_PORT "4053"

COPY ./bin/Release/netcoreapp2.1/publish/ /app

# 9110 - Petabridge.Cmd
# 4053 - Akka.Cluster
EXPOSE 9110 4053

CMD ["dotnet", "Akka.Bootstrap.Docker.Sample.dll"]
```