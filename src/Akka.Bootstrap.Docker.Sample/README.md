# Akka.Bootstrap.Docker.Sample
To run this sample:

## Windows
Open a PowerShell to _this directory_ and execute [`buildWindowsDockerImages.ps1`](buildWindowsDockerImages.ps1), like this:

```
PS> buildWindowsDockerImages.ps1 -tagVersion [version] [-imageName [name]]
```

This will build docker and tag two Docker images in your local Windows Docker registry:

* `[imageName]:latest-windows` and
* `[imageName]:[version]-windows`

> If the `imageName` argument is not provided, the image name will default to `akka.bootstrap.docker`.

To launch a small cluster using `docker-compose`, run the following command after building your docker images.

```
PS> docker-compose up -f docker-compose-windows.yaml
```

This will launch a three-node Akka.NET cluster by default, and you can interact with that cluster from the host machine via [Petabridge.Cmd](https://cmd.petabridge.com/) via the following:

```
PS> pbm 127.0.0.1:9110
```

That command will allow the `pbm` client to connect to the Petabridge.Cmd.Host running inside the `seed` node Docker container, and you can use that do things like monitor or manage the underlying Akka.NET cluster.

## Linux
Open a PowerShell to _this directory_ and execute [`buildLinuxDockerImages.sh`](buildLinuxDockerImages.sh), like this:

```
PS> buildLinuxDockerImages.sh [version] [imageName OPTIONAL]
```

This will build docker and tag two Docker images in your local Windows Docker registry:

* `[imageName]:latest-linux` and
* `[imageName]:[version]-linux`

> If the `imageName` argument is not provided, the image name will default to `akka.bootstrap.docker`.

To launch a small cluster using `docker-compose`, run the following command after building your docker images.

```
PS> docker-compose up -f docker-compose-linux.yaml
```

This will launch a three-node Akka.NET cluster by default, and you can interact with that cluster from the host machine via [Petabridge.Cmd](https://cmd.petabridge.com/) via the following:

```
PS> pbm 127.0.0.1:9110
```

That command will allow the `pbm` client to connect to the Petabridge.Cmd.Host running inside the `seed` node Docker container, and you can use that do things like monitor or manage the underlying Akka.NET cluster.

