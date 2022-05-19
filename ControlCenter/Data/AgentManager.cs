using Contracts;
using System.Collections.ObjectModel;

public class AgentManager
{
    public ObservableCollection<(string, IAgent)> Agents { get; } = new();
}
