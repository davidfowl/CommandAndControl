using Orleans;
using Orleans.Concurrency;

namespace Orleans.Collections;

[Reentrant]
public class CollectionGrain<T> : Grain, ICollectionGrain<T>
{
    private readonly List<T> _items = new();
    private readonly List<ICollectionObserver> _subs = new();

    public async Task AddItem(T item)
    {
        _items.Add(item);

        foreach (var s in _subs)
        {
            await s.OnCollectionChanged();
        }
    }

    public async Task RemoveItem(T item)
    {
        _items.Remove(item);

        foreach (var s in _subs)
        {
            await s.OnCollectionChanged();
        }
    }

    public Task<T[]> GetItems() => Task.FromResult(_items.ToArray());

    public Task Subscribe(ICollectionObserver observer)
    {
        _subs.Add(observer);
        return Task.CompletedTask;
    }

    public Task Unsubscribe(ICollectionObserver observer)
    {
        _subs.Remove(observer);
        return Task.CompletedTask;
    }
}
