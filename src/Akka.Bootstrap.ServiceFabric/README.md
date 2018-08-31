# Akka.Bootstrap.ServiceFabric

This project is intended to give Akka.NET users the tools they need to begin building Service Fabric services that use Akka.NET.

At the moment, the build scripts here support the following runtimes:

1. .NET Core 2.0

## Bootstrapping your Akka.NET Applications with Service Fabric
`Akka.Bootstrap.ServiceFabric` depends on the [environment variables](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-environment-variables-reference) Service Fabric makes available to each of your Service Fabric services. You will need to define appropriate endpoint resources in the service manifest. 

### Using `Akka.Bootstrap.ServiceFabric`
The `Akka.Bootstrap.ServiceFabric` NuGet package itself is pretty simple - all it does is expose the `ServiceFabricBootstrap` class which loads the of the Service Fabric environment variables into your Akka.Remote and Akka.Cluster configuration:

```csharp
var config = HoconLoader.FromFile("myHocon.hocon");
var myActorSystem = ActorSystem.Create(
                        "myactorsystem", config.BootstrapFromServiceFabric("AkkaClusterEndpoint"));
```

The `config.BootstrapFromServiceFabric("<endpoint name>")` line will do the heavy lifting. `<endpoint name>` is the name of the endpoint resource defined in the Service Fabric service manifest that nis to be used as the endpoint for Akka cluster communication.

Once this call is finished, all you have to do is launch your `ActorSystem` via `ActorSystem.Create` and everything will run automatically from that point onward.

### Environment Variables
`ServiceFabricBootstrap` uses the following environment:

| Environment Variable | Description |
|------------------------------------------|----------------------------------------|
| Fabric_Endpoint_IPOrFQDN_*ServiceEndpointName* | The IP address or FQDN of the endpoint |
| Fabric_Endpoint_*ServiceEndpointName* | Port number for the endpoint |
| CLUSTER_SEEDS | A comma-delimited list of seed node addressed used by Akka.Cluster |

where *ServiceEndpointName* is the name of an endpoint resource defined in the service manifest, typically `ServiceManifest.xml`, e.g.:

```xml
  <Resources>
    <Endpoints>
      <Endpoint Name="AkkaClusterEndpoint" Protocol="tcp" Port="4053" />
    </Endpoints>
  </Resources>

```
The `Fabric_Endpoint_*` environment variables will be available if you're running inside Service Fabric. In the above case the environment variable names will be `Fabric_Endpoint_IPOrFQDN_AkkaClusterEndpoint` and `Fabric_Endpoint_AkkaClusterEndpoint`.

You can define the `CLUSTER_SEEDS` environment variable in your `ServiceManifest.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<ServiceManifest Name="MyServicePkg"
                 Version="1.0.0"
                 xmlns="http://schemas.microsoft.com/2011/01/fabric"
                 xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                 xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
    <ServiceTypes>
        <StatelessServiceType ServiceTypeName="MyServiceType" HasPersistedState="true" />
    </ServiceTypes>
    
    <CodePackage Name="Code" Version="1.0.0">
        <EntryPoint>
          <ExeHost>
            <Program>MyService.exe</Program>
          </ExeHost>
        </EntryPoint>
        <EnvironmentVariables>
          <EnvironmentVariable Name="CLUSTER_SEEDS" Value="akka.tcp://myactorsystem@192.168.10.11:4053"/>
        </EnvironmentVariables>
    </CodePackage>
    <ConfigPackage Name="Config" Version="1.0.0" />
    
    <Resources>
        <Endpoints>
            <Endpoint Name="AkkaClusterEndpoint" Protocol="tcp" Port="4053" />
        </Endpoints>
    </Resources>
</ServiceManifest>
```

You can override these values in your `ServiceManifest.xml` (abbreviated):

```xml
<?xml version="1.0" encoding="utf-8"?>
<ApplicationManifest xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" ApplicationTypeName="MyApp" ApplicationTypeVersion="1.0.0" xmlns="http://schemas.microsoft.com/2011/01/fabric">
  <Parameters>
    <Parameter Name="CLUSTER_SEEDS" DefaultValue="akka.tcp://myactorsystem@192.168.10.11:4053" />
  </Parameters>
  <ServiceManifestImport>
    <ServiceManifestRef ServiceManifestName="MyServicePkg" ServiceManifestVersion="1.0.0" />
    <EnvironmentOverrides CodePackageRef="Code">
      <EnvironmentVariable Name="CLUSTER_SEEDS" Value="[CLUSTER_SEEDS]" />
    </EnvironmentOverrides>
  </ServiceManifestImport>
</ApplicationManifest>
```

Declaring override parameters in `ServiceManifest.xml` allows you to override these values per environment with parameters override files like this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Application xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Name="fabric:/AnalyticsSystem.Application" xmlns="http://schemas.microsoft.com/2011/01/fabric">
  <Parameters>
    <Parameter Name="CLUSTER_SEEDS" Value="akka.tcp://myactorsystem@10.0.0.9:4053" />
  </Parameters>
</Application>
```

You can use this parameter technique to override the port numbers on your endpoints as well:

```xml
<?xml version="1.0" encoding="utf-8"?>
<ApplicationManifest xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" ApplicationTypeName="MyApp" ApplicationTypeVersion="1.0.0" xmlns="http://schemas.microsoft.com/2011/01/fabric">
  <Parameters>
    <Parameter Name="CLUSTER_SEEDS" DefaultValue="akka.tcp://myactorsystem@192.168.10.11:4053" />
    <Parameter Name="CLUSTER_PORT" DefaultValue="4053" />
  </Parameters>
  <ServiceManifestImport>
    <ServiceManifestRef ServiceManifestName="MyServicePkg" ServiceManifestVersion="1.0.0" />
    <ResourceOverrides>
      <Endpoints>
        <Endpoint Name="AkkaClusterEndpoint" Protocol="tcp" Port="[CLUSTER_PORT]" />
      </Endpoints>
    </ResourceOverrides>
    <EnvironmentOverrides CodePackageRef="Code">
      <EnvironmentVariable Name="CLUSTER_SEEDS" Value="[CLUSTER_SEEDS]" />
    </EnvironmentOverrides>
  </ServiceManifestImport>
</ApplicationManifest>
```


If `CLUSTER_SEEDS` is not available Akka.NET will fall back to whatever was configured inside your application itself.

