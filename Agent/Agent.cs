// See https://aka.ms/new-console-template for more information
using Contracts;
using Microsoft.Extensions.Logging;

class Agent : IAgent
{
    private ILogger<Agent> _logger;

    public Agent(ILogger<Agent> logger)
    {
        _logger = logger;
    }

    public async Task<double> GetTemperature()
    {
        _logger.LogInformation("Getting temperature from device");

        // Fake delay checking the device temperature
        await Task.Delay(1000);

        return Random.Shared.Next(40, 100);
    }

    public Task Shutdown()
    {
        _logger.LogInformation("Shutting down device..");

        Environment.Exit(0);

        return Task.CompletedTask;
    }
}
