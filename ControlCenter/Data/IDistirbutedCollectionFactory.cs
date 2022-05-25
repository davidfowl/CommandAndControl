
using Orleans;

public interface IDistirbutedCollectionFactory
{
    DistributedObservableCollection<T> CreateObservableCollection<T>(long key);
}

public class DistirbutedCollectionFactory : IDistirbutedCollectionFactory
{
    private readonly IGrainFactory _grainFactory;
    public DistirbutedCollectionFactory(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public DistributedObservableCollection<T> CreateObservableCollection<T>(long key)
    {
        return new DistributedObservableCollection<T>(_grainFactory, key);
    }
}