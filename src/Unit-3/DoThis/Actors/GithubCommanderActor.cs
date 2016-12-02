﻿using System;
using System.Linq;
using Akka.Actor;
using Akka.Routing;

namespace GithubActors.Actors
{
    /// <summary>
    /// Top-level actor responsible for coordinating and launching repo-processing jobs
    /// </summary>
    public class GithubCommanderActor : ReceiveActor, IWithUnboundedStash
    {
        #region Message classes

        public class CanAcceptJob
        {
            public CanAcceptJob(RepoKey repo)
            {
                Repo = repo;
            }

            public RepoKey Repo { get; private set; }
        }

        public class AbleToAcceptJob
        {
            public AbleToAcceptJob(RepoKey repo)
            {
                Repo = repo;
            }

            public RepoKey Repo { get; private set; }
        }

        public class UnableToAcceptJob
        {
            public UnableToAcceptJob(RepoKey repo)
            {
                Repo = repo;
            }

            public RepoKey Repo { get; private set; }
        }

        #endregion

        private IActorRef _coordinator;
        private IActorRef _canAcceptJobSender;
        private int _pendingJobReplies;

        public IStash Stash { get; set; }

        public GithubCommanderActor()
        {
            Ready();
        }

        private void Ready()
        {
            Receive<CanAcceptJob>(job =>
            {
                _coordinator.Tell(job);

                BecomeAsking();
            });
        }

        private void BecomeAsking()
        {
            _canAcceptJobSender = Sender;
            _pendingJobReplies = _coordinator.Ask<Routees>(new GetRoutees()).Result.Members.Count(); //wert
            Become(Asking);
        }

        private void Asking()
        {
            Receive<CanAcceptJob>(job => Stash.Stash());

            Receive<UnableToAcceptJob>(job =>
            {
                _pendingJobReplies--;
                if (_pendingJobReplies == 0)
                {
                    _canAcceptJobSender.Tell(job);
                    BecomeReady();
                }
            });

            Receive<AbleToAcceptJob>(job =>
            {
                _canAcceptJobSender.Tell(job);

                //start processing messages
                Sender.Tell(new GithubCoordinatorActor.BeginJob(job.Repo));

                //launch the new window to view results of the processing
                Context.ActorSelection(ActorPaths.MainFormActor.Path).Tell(
                    new MainFormActor.LaunchRepoResultsWindow(job.Repo, Sender));
            });

            BecomeReady();
        }

        private void BecomeReady()
        {
            Become(Ready);
            Stash.UnstashAll();
        }

        protected override void PreStart()
        {
            _coordinator = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor())
                .WithRouter(FromConfig.Instance), ActorPaths.GithubCoordinatorActor.Name);
            base.PreStart();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            var c1 = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor()),
                ActorPaths.GithubCoordinatorActor.Name + "1");
            var c2 = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor()),
                ActorPaths.GithubCoordinatorActor.Name + "2");
            var c3 = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor()),
                ActorPaths.GithubCoordinatorActor.Name + "3");
        }
    }
}
