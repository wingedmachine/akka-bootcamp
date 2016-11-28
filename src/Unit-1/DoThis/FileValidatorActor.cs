using Akka.Actor;
using System.IO;

namespace WinTail
{
    /// <summary>
    /// Acor that validates user input and signals result to others.
    /// </summary>
    public class FileValidatorActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;

        public FileValidatorActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                // signal that the user needs to supply non-blank input
                _consoleWriterActor.Tell(new Messages.NullInputError("No input received."));

                Sender.Tell(new Messages.ContinueProcessing());
            }
            else
            {
                if (IsFileUri(msg))
                {
                    _consoleWriterActor.Tell(new Messages.InputSuccess(string.Format("Hang on while we look at {0}...", msg)));

                    Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorActor").Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                }
                else
                {
                    _consoleWriterActor.Tell(new Messages.ValidationError(string.Format("Fuck you. {0} ain't no file.", msg)));

                    Sender.Tell(new Messages.ContinueProcessing());
                }
            }
        }

        /// <summary>
        /// Checks if file exists at path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}
