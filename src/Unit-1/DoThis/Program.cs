using Akka.Actor;

namespace WinTail
{
    #region Program
    class Program
    {
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            MyActorSystem = ActorSystem.Create("MyActorStystem");

            Props consoleWriterProps = Props.Create(() => new ConsoleWriterActor());
            IActorRef consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");

            Props tailCoordinatorProps = Props.Create(() => new TailCoordinatorActor());
            IActorRef tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");

            Props validationActorProps = Props.Create(() => new FileValidatorActor(consoleWriterActor, tailCoordinatorActor));
            IActorRef validationActor = MyActorSystem.ActorOf(validationActorProps, "validationActor");

            Props consoleReaderProps = Props.Create<ConsoleReaderActor>(validationActor);
            IActorRef consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");

            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }
    }
    #endregion
}
