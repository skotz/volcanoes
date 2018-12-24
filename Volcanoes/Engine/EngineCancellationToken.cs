using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Engine
{
    class EngineCancellationToken
    {
        public bool Cancelled { get { return _isCancelled(); } }

        private Func<bool> _isCancelled;

        public EngineCancellationToken(Func<bool> isCancelled)
        {
            _isCancelled = isCancelled;
        }

        public EngineCancellationToken(BackgroundWorker worker)
        {
            _isCancelled = () => worker.CancellationPending;
        }
    }
}
