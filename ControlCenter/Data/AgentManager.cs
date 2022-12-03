using Contracts;
using ControlCenter.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.ObjectModel;

public class AgentManager
{
    private IHubContext<AgentHub, IAgent> _hubContext;

    public AgentManager(IHubContext<AgentHub, IAgent> hubContext)
    {
        _hubContext = hubContext;
    }

    public ObservableCollection<(string, IAgent)> Agents { get; } = new();

    public void Add(string connectionId)
    {
        lock (Agents)
        {
            Agents.Add((connectionId, _hubContext.Clients.Client(connectionId)));
        }
    }

    public void Remove(string connectionId)
    {
        lock (Agents)
        {
            var item = Agents.FirstOrDefault(a => a.Item1 == connectionId);
            Agents.Remove(item);
        }
    }
}
