using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Engine
{
    internal interface ILearn
    {
        event EventHandler<LearnStatus> OnStatus;

        event EventHandler<LearnResult> OnComplete;

        void Train();
    }
}