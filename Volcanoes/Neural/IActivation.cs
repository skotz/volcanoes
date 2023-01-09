namespace Volcano.Neural
{
    public interface IActivation
    {
        double Run(double value);

        double Derivative(double value);

        double StandardDeviation(int inputs, int outputs);
    }
}