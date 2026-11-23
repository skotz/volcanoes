using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using FluentAssertions;
using TorchSharp;
using Volcano.Game;
using Volcano.Neural;
using Xunit;

namespace Volcanoes.Tests.Neural
{
    public class EncoderSnapshotTests
    {
        private const string Transcript = "N26 N10 G N28 S39 G N12 S37 G N19 N18 G S17 N27 G N19+ N38 G N11 N40 G N11";

        private const string EncoderExpectedHash = "6CAA77B6FF3BF81640AB48CB7BD063F394EE5709D5AA69EFF8BA21D587D1B89E";
        private const string GrowthEncoderExpectedHash = "FECE04D38599578BE207C6060A88168E8B6C6F7612C9A215D18AEF7C09D326CE";
        private const string GraphEncoderExpectedHash = "D0634A305CAFD64AF78DF5DF3F197177773362875CC0BD742B3E9EDD06C86D3D";
        private const string SimpleEncoderExpectedHash = "9B71404635256292032E7E72A19B9AF5A531511BE2CE712EB7062F8519F26A78";

        [Fact]
        public void Encoder_KnownPosition_MatchesExpectedEncoding()
        {
            AssertEncodingMatches(new Encoder(), EncoderExpectedHash);
        }

        [Fact]
        public void GrowthEncoder_KnownPosition_MatchesExpectedEncoding()
        {
            AssertEncodingMatches(new GrowthEncoder(), GrowthEncoderExpectedHash);
        }

        [Fact]
        public void GraphEncoder_KnownPosition_MatchesExpectedEncoding()
        {
            AssertEncodingMatches(new GraphEncoder(), GraphEncoderExpectedHash);
        }

        [Fact]
        public void SimpleEncoder_KnownPosition_MatchesExpectedEncoding()
        {
            AssertEncodingMatches(new SimpleEncoder(), SimpleEncoderExpectedHash);
        }

        private static void AssertEncodingMatches(IBoardEncoder encoder, string expectedHash)
        {
            var game = new VolcanoGame();
            game.LoadTranscript(Transcript).Should().BeTrue();

            var encoded = encoder.Encode(new List<Board> { game.CurrentState }, torch.CPU);

            encoded.shape[0].Should().Be(1);

            var data = encoded.data<float>().ToArray();
            var bytes = new byte[data.Length * sizeof(float)];
            Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);

            Convert.ToHexString(SHA256.HashData(bytes)).Should().Be(expectedHash);
        }
    }
}
