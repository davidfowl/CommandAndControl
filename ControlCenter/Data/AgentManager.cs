using Orleans.Collections;

public class AgentManager
{
    public AgentManager(IDistributedCollectionFactory factory)
    {
        Collection = factory.CreateObservableCollection<string>(0);
    }

    public DistributedObservableCollection<string> Collection { get; }
}