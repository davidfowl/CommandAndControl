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
        _agentManager.AddAgent(Context.ConnectionId);

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _agentManager.RemoveAgent(Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }
}