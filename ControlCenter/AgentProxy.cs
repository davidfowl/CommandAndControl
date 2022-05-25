

// Wrapper to workaround bug in signalr
using Contracts;
using Microsoft.AspNetCore.SignalR;

public class AgentProxy : IAgent
{
    private readonly ISingleClientProxy _clientProxy;
    private AgentProxy(ISingleClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public Task<double> GetTemperature()
    {
        return _clientProxy.InvokeAsync<double>(nameof(GetTemperature));
    }

    public Task Shutdown()
    {
        return _clientProxy.SendAsync(nameof(Shutdown));
    }

    public static IAgent Create(ISingleClientProxy clientProxy) => new AgentProxy(clientProxy);
}