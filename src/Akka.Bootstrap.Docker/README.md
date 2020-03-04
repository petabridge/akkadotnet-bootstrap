# Akka.Bootstrap.Docker

This project is intended to give Akka.NET users the tools they need to begin building Docker base images on top of Akka.NET.

This library works with any runtime supported by Akka.NET.

## Bootstrapping your Akka.NET Applications with Docker
`Akka.Bootstrap.Docker` depends on having some standardized environment variables made available inside each of your Docker containers:

* `CLUSTER_IP` - this value will replace the `akka.remote.dot-netty.tcp.public-hostname` at runtime. If this value is not provided, we will use `Dns.GetHostname()` instead.
* `CLUSTER_PORT` - the port number that will be used by Akka.Remote for inbound connections.
* `CLUSTER_SEEDS` - a comma-delimited list of seed node addresses used by Akka.Cluster. Here's [an example](https://github.com/petabridge/Cluster.WebCrawler/blob/9f854ff2bfb34464769f562936183ea7719da4ea/yaml/k8s-tracker-service.yaml#L46-L47).

### Using Environment Variables to Configure Akka.NET
In addition to the standardized environment variables listed above, the `Akka.Bootstrap.Docker` NuGet package can also parse Akka.NET configuration from environment variables.  In order for the package to map environment variables to a HOCON entry, the name of the environment variable must adhere to the following conventions:

* Full stops (`.`) in the HOCON path are replaced with two underscores (`__`)
* Hyphens (`-`) in the HOCON path are replaced with one underscore (`_`)
* For an array based HOCON entry, the environment variable should be suffixed by two underscores and the index of the entry within the array (`__0`, `__1`, `__2`, etc. ).

As an example, the following list of environment variables:

```
AKKA__COORDINATED_SHUTDOWN__EXIT_CLR="on"
AKKA__ACTOR__PROVIDER="cluster"
AKKA__REMOTE__DOT_NETTY__TCP__HOSTNAME="127.0.0.1"
AKKA__REMOTE__DOT_NETTY__TCP__PUBLIC_HOSTNAME="example.local"
AKKA__REMOTE__DOT_NETTY__TCP__PORT="2559"
AKKA__CLUSTER__ROLES__0="demo"
AKKA__CLUSTER__ROLES__1="test"
AKKA__CLUSTER__ROLES__2="backup"
```

will produce a HOCON structure of:

```
akka {
    coordinated_shutdown.exit_clr = "on"
    actor.provider = "cluster"
    remote.dot_netty.tcp {
        hostname = "127.0.0.1"
        public_hostname = "example.local"
        port = "2559"
    }
    cluster.roles = [ "demo", "test", "backup" ]
}
```

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

In our opinion, the easiest way to work with Docker in .NET is to call `dotnet publish` on the application you want to Dockerize and then to simply copy all of the binaries from the `bin/release/[framework]/publish` directory into the working directory of your Docker container. 

This keeps the Dockerfile very tidy and enables developers to take advantage of things like copying local binaries and packages into the container (which the Petabridge team has found to be helpful during dev and test.) 

This is exactly what the [`buildWindowsDockerImages.ps1`](../Akka.Bootstrap.Docker.Sample/buildWindowsDockerImages.ps1) and [`buildLinuxDockerImages.sh`](../Akka.Bootstrap.Docker.Sample/buildLinuxDockerImages.sh) scripts in our sample do.

From there, we recommend using tools like `docker-compose` or Kubernetes to pass in the environment variables expected by `Akka.Bootstrap.Docker` - that way you can form your initial network of Docker containers automatically. 

Here are some relevant examples:

* `docker-compose` - [Akka.Bootstrap.Docker.Sample/docker-compose-windows.yaml](../Akka.Bootstrap.Docker.Sample/docker-compose-windows.yaml)
* `docker-compose` - [Akka.Bootstrap.Docker.Sample/docker-compose-linux.yaml](../Akka.Bootstrap.Docker.Sample/docker-compose-linux.yaml)
* `kubernetes` - [Cluster.WebCrawler K8s Service Definitions](https://github.com/petabridge/Cluster.WebCrawler/tree/dev/yaml)
