using System;
using System.Collections.Generic;
using System.Text;
using static TorchSharp.torch;

namespace Volcano.Neural
{
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.

    public static class skotz
    {
        public static class nn
        {
            public static GraphConvLayer ConvGraph(long inFeatures, long outFeatures, Tensor boardTopology)
            {
                return new GraphConvLayer(inFeatures, outFeatures, boardTopology);
            }

            public static CylindricalConv2d CylConv2d(long inFeatures, long outFeatures)
            {
                return new CylindricalConv2d(inFeatures, outFeatures);
            }
        }
    }

#pragma warning restore CS8981
}