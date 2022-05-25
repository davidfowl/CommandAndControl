using Orleans;

public class AgentManager
{
    public AgentManager(IDistirbutedCollectionFactory factory)
    {
        Collection = factory.CreateObservableCollection<string>(0);
    }

    public DistributedObservableCollection<string> Collection { get; }
}

class StringCollection : CollectionGrain<string>
{

}
