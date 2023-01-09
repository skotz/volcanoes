using System;

namespace Volcano.Engine
{
    internal interface ITrainable
    {
        event EventHandler<TrainStatus> OnTrainStatus;

        void SaveSamples();

        void Train();
    }
}