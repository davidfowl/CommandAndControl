using Contracts;
using ControlCenter.Hubs;
using Microsoft.AspNetCore.SignalR;
using Orleans;
using Orleans.Concurrency;

public class AgentManager : IAgentEventSubscriber, IAsyncDisposable
{
    private readonly IAgentGrain _agentGrain;
    private readonly IGrainFactory _grainFactory;
    private Func<Task> _agentsChanged = () => { return Task.CompletedTask; };
    private IAgentEventSubscriber? _subscriberProxy;
    private IHubContext<AgentHub> _hubContext;

    public AgentManager(IGrainFactory grainFactory, IHubContext<AgentHub> hubContext)
    {
        _grainFactory = grainFactory;
        _hubContext = hubContext;
        _agentGrain = grainFactory.GetGrain<IAgentGrain>(0);
    }

    public Task AddAgent(string connectionId)
    {
        return _agentGrain.AddAgent(connectionId);
    }

    public Task RemoveAgent(string connectionId)
    {
        return _agentGrain.RemoveAgent(connectionId);
    }

    public async Task<(string, IAgent)[]> GetAgents()
    {
        var agents = await _agentGrain.GetAgents();

        return agents.Select(a => (a, AgentProxy.Create(_hubContext.Clients.Single(a)))).ToArray();
    }

    public Task OnAgentsChanged()
    {
        return _agentsChanged.Invoke();
    }

    public async Task<IAsyncDisposable> SubscribeAsync(Func<Task> onChanged)
    {
        _agentsChanged += onChanged;

        if (_subscriberProxy is null)
        {
            _subscriberProxy = await _grainFactory.CreateObjectReference<IAgentEventSubscriber>(this);
            await _agentGrain.Subscribe(_subscriberProxy);
        }

        return new AsyncDisposable(async () =>
        {
            _agentsChanged -= onChanged;
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_subscriberProxy is not null)
        {
            await _agentGrain.Unsubscribe(_subscriberProxy);
        }
    }

    private class AsyncDisposable : IAsyncDisposable
    {
        private readonly Func<ValueTask> _callback;
        public AsyncDisposable(Func<ValueTask> callback)
        {
            _callback = callback;
        }
        public ValueTask DisposeAsync()
        {
            return _callback();
        }
    }

    // Wrapper to workaround bug in signalr
    private class AgentProxy : IAgent
    {
        private readonly ISingleClientProxy _clientProxy;
        public AgentProxy(ISingleClientProxy clientProxy)
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
}

public interface IAgentEventSubscriber : IGrainObserver
{
    Task OnAgentsChanged();
}

interface IAgentGrain : IGrainWithIntegerKey
{
    Task AddAgent(string connectionId);
    Task RemoveAgent(string connectionId);
    Task Subscribe(IAgentEventSubscriber agentEventSubscriber);
    Task Unsubscribe(IAgentEventSubscriber agentEventSubscriber);

    Task<string[]> GetAgents();
}

[Reentrant]
public class AgentGrain : Grain, IAgentGrain
{
    private readonly List<string> _agentIds = new();
    private readonly List<IAgentEventSubscriber> _subs = new();

    public async Task AddAgent(string connectionId)
    {
        _agentIds.Add(connectionId);

        foreach (var s in _subs)
        {
            await s.OnAgentsChanged();
        }
    }

    public async Task RemoveAgent(string connectionId)
    {
        _agentIds.Remove(connectionId);

        foreach (var s in _subs)
        {
            await s.OnAgentsChanged();
        }
    }

    public Task<string[]> GetAgents() => Task.FromResult(_agentIds.ToArray());

    public Task Subscribe(IAgentEventSubscriber agentEventSubscriber)
    {
        _subs.Add(agentEventSubscriber);
        return Task.CompletedTask;
    }

    public Task Unsubscribe(IAgentEventSubscriber agentEventSubscriber)
    {
        _subs.Remove(agentEventSubscriber);
        return Task.CompletedTask;
    }
}
