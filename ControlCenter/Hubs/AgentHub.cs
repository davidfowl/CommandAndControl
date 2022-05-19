using Contracts;
using Microsoft.AspNetCore.SignalR;

namespace ControlCenter.Hubs;

// Agents connect to the hub for command and control
public class AgentHub : Hub<IAgent>
{
    private readonly AgentManager _agentManager;

    public AgentHub(AgentManager agentManager)
    {
        _agentManager = agentManager;
    }

    public override Task OnConnectedAsync()
    {
        lock (_agentManager.Agents)
        {
            _agentManager.Agents.Add((Context.ConnectionId, Clients.Single(Context.ConnectionId)));
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        lock (_agentManager.Agents)
        {
            var item = _agentManager.Agents.FirstOrDefault(a => a.Item1 == Context.ConnectionId);
            _agentManager.Agents.Remove(item);
        }

        return base.OnDisconnectedAsync(exception);
    }
}