using Contracts;
using ControlCenter.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

public class AgentManager
{
    private readonly IHubContext<AgentHub, IAgent> _hubContext;
    private readonly ObservableCollection<(string, IAgent)> _agents = new();

    public AgentManager(IHubContext<AgentHub, IAgent> hubContext)
    {
        _hubContext = hubContext;
    }

    public IEnumerable<(string, IAgent)> Agents => _agents;

    public event NotifyCollectionChangedEventHandler StateChanged
    {
        add => _agents.CollectionChanged += value;
        remove => _agents.CollectionChanged -= value;
    }

    public void Add(string connectionId)
    {
        lock (_agents)
        {
            _agents.Add((connectionId, _hubContext.Clients.Client(connectionId)));
        }
    }

    public void Remove(string connectionId)
    {
        lock (_agents)
        {
            var item = _agents.FirstOrDefault(a => a.Item1 == connectionId);
            _agents.Remove(item);
        }
    }
}
