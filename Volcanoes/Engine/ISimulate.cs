namespace Volcano.Engine
{
    internal interface ISimulate
    {
        /// <summary>
        /// Run an exact number of simulations
        /// </summary>
        int ForcedSimulations { get; set; }
    }
}