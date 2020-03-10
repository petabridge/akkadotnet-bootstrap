using System;
using System.Diagnostics;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Akka.Routing;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote;

namespace Akka.Bootstrap.Docker.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached) // If we're launching running from visual studio, set CLUSTER_IP
            {
                Environment.SetEnvironmentVariable("CLUSTER_IP", "localhost");
            }

            var config = ConfigurationFactory.ParseString(File.ReadAllText("app.conf"))
                .BootstrapFromDocker(); // forces all Docker environment variable substitution
            var actorSystem = ActorSystem.Create("DockerBootstrap", config);

            var echo = actorSystem.ActorOf(Props.Create(() => new EchoActor()), "echo");
            var router = actorSystem.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "router");

            // use PBM so our containers can be interacted with from the host system
            var pbm = PetabridgeCmd.Get(actorSystem);
            pbm.RegisterCommandPalette(ClusterCommands.Instance);
            pbm.RegisterCommandPalette(RemoteCommands.Instance);
            pbm.Start();

            /*
             * Wait until we've joined the cluster before we begin messaging any other nodes.
             */
            var count = 0;
            Cluster.Cluster.Get(actorSystem).RegisterOnMemberUp(() =>
            {
                actorSystem.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), () =>
                {
                    router.Tell(count++, echo);
                });
            });

            // block until the ActorSystem is terminated (try "cluster leave" using Petabridge.Cmd https://cmd.petabridge.com/articles/commands/cluster-commands.html)
            actorSystem.WhenTerminated.Wait();
        }
    }

    public sealed class EchoActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        public EchoActor()
        {
            ReceiveAny(_ =>
            {
                _log.Info("Received {0} from {1}", _,Sender);
            });
        }
    }
}
