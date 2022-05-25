using Orleans;
using Orleans.Concurrency;

public class AgentManager : IAgentEventSubscriber, IAsyncDisposable
{
    private readonly IAgentGrain _agentGrain;
    private readonly IGrainFactory _grainFactory;
    private Func<Task> _agentsChanged = () => { return Task.CompletedTask; };
    private IAgentEventSubscriber? _subscriberProxy;

    public AgentManager(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
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

    public async Task<string[]> GetAgents()
    {
        return await _agentGrain.GetAgents();
    }

    public Task OnAgentsChanged()
    {
        return _agentsChanged.Invoke();
    }

    public async Task<IDisposable> SubscribeAsync(Func<Task> onChanged)
    {
        _agentsChanged += onChanged;

        if (_subscriberProxy is null)
        {
            _subscriberProxy = await _grainFactory.CreateObjectReference<IAgentEventSubscriber>(this);
            await _agentGrain.Subscribe(_subscriberProxy);
        }

        return new AsyncDisposable(() =>
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

    private class AsyncDisposable : IDisposable
    {
        private readonly Action _callback;
        public AsyncDisposable(Action callback)
        {
            _callback = callback;
        }

        public void Dispose()
        {
            _callback();
        }
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
