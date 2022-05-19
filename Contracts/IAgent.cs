namespace Contracts;

public interface IAgent
{
    Task<double> GetTemperature();

    Task Shutdown();
}