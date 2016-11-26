using System;
using Akka.Actor;

namespace WinTail
{
    public class TailCoordinatorActor : UntypedActor
    {
        #region Message types

        ///<summary>
        ///Start tailing file at user-speciied path
        /// </summary>
        public class StartTail
        {
            public StartTail(string filePath, IActorRef reporterActor)
            {
                filePath = filePath;
                reporterActor = reporterActor;
            }

            public string FilePath { get; private set; }
            public IActorRef ReporterActor { get; private set; }
        }

        /// <summary>
        /// Stop tailing file at user-specified path
        /// </summary>
        public class StopTail
        {
            public StopTail(string filePath)
            {
                filePath = filePath;
            }

            public string FilePath { get; private set; }
        }
        #endregion

        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;
                //wert
            }
        }

    }
}
