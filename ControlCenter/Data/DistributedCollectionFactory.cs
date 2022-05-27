
namespace Orleans.Collections;

public class DistributedCollectionFactory : IDistributedCollectionFactory
{
    private readonly IGrainFactory _grainFactory;
    public DistributedCollectionFactory(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public DistributedObservableCollection<T> CreateObservableCollection<T>(long key)
    {
        return new DistributedObservableCollection<T>(_grainFactory, key);
    }
}