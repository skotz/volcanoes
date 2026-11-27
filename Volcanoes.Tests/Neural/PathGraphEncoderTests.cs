using FluentAssertions;
using System.Security.Cryptography;
using TorchSharp;
using Volcano.Game;
using Volcano.Neural;

namespace Volcanoes.Tests.Neural
{
    public class PathGraphEncoderTests
    {
        private const string Transcript = "N26 N10 G N28 S39 G N12 S37 G N19 N18 G S17 N27 G N19+ N38 G N11 N40 G N11";

        private const string ExpectedHash = "F3A7C78DD897F97473E7B1279FD9E1AE7B29E81CE2C9AC79474EEC6B532548B0";

        [Fact]
        public void Encode_KnownPosition_MatchesExpectedEncoding()
        {
            var game = new VolcanoGame();
            game.LoadTranscript(Transcript).Should().BeTrue();

            var encoded = new PathGraphEncoder().Encode(new List<Board> { game.CurrentState }, torch.CPU);

            encoded.shape.Should().Equal(new long[] { 1, PathGraphEncoder.nodes, PathGraphEncoder.features });

            var data = encoded.data<float>().ToArray();
            var bytes = new byte[data.Length * sizeof(float)];
            Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);

            Convert.ToHexString(SHA256.HashData(bytes)).Should().Be(ExpectedHash);
        }
    }
}