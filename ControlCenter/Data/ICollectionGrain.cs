using Orleans;

public interface ICollectionGrain<T> : IGrainWithIntegerKey
{
    Task AddItem(T item);
    Task RemoveItem(T item);
    Task Subscribe(ICollectionObserver observer);
    Task Unsubscribe(ICollectionObserver observer);
    Task<T[]> GetItems();
}
