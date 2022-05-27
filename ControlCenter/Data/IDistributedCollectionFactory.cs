
namespace Orleans.Collections;

public interface IDistributedCollectionFactory
{
    DistributedObservableCollection<T> CreateObservableCollection<T>(long key);
}
