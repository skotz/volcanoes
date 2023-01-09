namespace Volcano.Neural
{
    public interface ISample
    {
        double[,,] Inputs { get; set; }

        double[,,] Outputs { get; set; }

        ISample Clone();

        string OutputsToString();
    }
}