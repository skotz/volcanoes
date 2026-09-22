using System;
using Volcano.Game;

namespace Volcano.Engine
{
    internal class QLearningEngine : IEngine, IStatus, ILearn
    {
        public event EventHandler<LearnStatus> OnDebug;

        public event EventHandler<EngineStatus> OnStatus;

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            throw new NotImplementedException();
        }

        private void Debug(string status)
        {
            OnDebug?.Invoke(this, new LearnStatus(status));
        }
        private void Report(EngineStatus status)
        {
            OnStatus?.Invoke(this, status);
        }

        public void Train()
        {
            Debug("Starting");
        }
    }
}