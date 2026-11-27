using System;
using TorchSharp;
using static TorchSharp.torch;

namespace Volcano.Neural
{
    internal static class PolicyValueLoss
    {
        public static (Tensor Policy, Tensor Value, Tensor Total) Compute(torch.nn.Module<Tensor, Tuple<Tensor, Tensor>> model, Tuple<Tensor, Tensor> output, Tensor policyTarget, Tensor valueTarget)
        {
            var valueLoss = torch.nn.functional.mse_loss(output.Item2, valueTarget);
            var policyLoss = model is GraphResNet
                ? GraphPolicyLoss(output.Item1, policyTarget)
                : torch.nn.functional.cross_entropy(output.Item1, policyTarget);
            return (policyLoss, valueLoss, policyLoss + 0.5 * valueLoss);
        }

        private static Tensor GraphPolicyLoss(Tensor logits, Tensor policyTarget)
        {
            var logProbs = torch.nn.functional.log_softmax(logits, dim: 1);
            var elementWiseLoss = -(policyTarget * logProbs);
            return elementWiseLoss.sum(new long[] { 1 }).mean();
        }
    }
}