# Akka.NET Bootstrapping Tools

Akka.Remote and Akka.Cluster bootstrapping utilities for [Akka.NET](http://getakka.net/) on various runtime environments.

To learn more about a specific runtime environment, please see the following:

* **[Bootstrapping Akka.NET for Docker](src/Akka.Bootstrap.Docker)**
* **[Bootstrapping Akka.NET for Pivotal Cloud Foundry (PCF)](src/Akka.Bootstrap.PCF)**
* **[Bootstrapping Akka.NET for Service Fabric](src/Akka.Bootstrap.ServiceFabric)**

## Building this solution
To run the build script associated with this solution, execute the following:

**Windows**
```
c:\> build.cmd all
```

**Linux / OS X**
```
c:\> build.sh all
```

If you need any information on the supported commands, please execute the `build.[cmd|sh] help` command.

This build script is powered by [FAKE](https://fake.build/); please see their API documentation should you need to make any changes to the [`build.fsx`](build.fsx) file.

This library is maintained by Petabridge®. Copyright 2015-2020.
