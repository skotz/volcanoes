using System.Collections.Generic;
using TorchSharp;
using Volcano.Game;

namespace Volcano.Neural
{
    internal interface IBoardEncoder
    {
        int InputChannels { get; }

        torch.Tensor Encode(IReadOnlyList<Board> states, torch.Device device);

        torch.Tensor Encode(Board state, torch.Device device);
    }
}