using System;

namespace Volcano.Engine
{
    internal interface ILearn
    {
        event EventHandler<LearnStatus> OnDebug;

        void Train();
    }
}