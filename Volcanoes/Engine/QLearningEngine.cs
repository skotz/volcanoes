using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    internal class QLearningEngine : IEngine, ILearn
    {
        public event EventHandler<LearnStatus> OnStatus;

        public event EventHandler<LearnResult> OnComplete;

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void Train()
        {
            OnStatus.Invoke(this, new LearnStatus("Starting"));
            Thread.Sleep(5000);
            OnStatus.Invoke(this, new LearnStatus("Blah"));
        }
    }
}