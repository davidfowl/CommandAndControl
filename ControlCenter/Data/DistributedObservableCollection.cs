
using Orleans;

public class DistributedObservableCollection<T> : ICollectionObserver
{
    private readonly ICollectionGrain<T> _collectionGrain;
    private readonly IGrainFactory _grainFactory;
    private ICollectionObserver? _collectionObserver;
    private Func<Task>? _changed;

    public DistributedObservableCollection(IGrainFactory grainFactory, long key)
    {
        _grainFactory = grainFactory;
        _collectionGrain = grainFactory.GetGrain<ICollectionGrain<T>>(key);
    }

    public Task AddItem(T item) => _collectionGrain.AddItem(item);
    public Task RemoveItem(T item) => _collectionGrain.RemoveItem(item);
    public Task<T[]> GetItems() => _collectionGrain.GetItems();

    Task ICollectionObserver.OnCollectionChanged()
    {
        return _changed?.Invoke() ?? Task.CompletedTask;
    }

    public async Task<IDisposable> SubscribeAsync(Func<Task> onChanged)
    {
        _changed += onChanged;

        if (_collectionObserver is null)
        {
            _collectionObserver = await _grainFactory.CreateObjectReference<ICollectionObserver>(this);
            await _collectionGrain.Subscribe(_collectionObserver);
        }

        return new Disposable(() =>
        {
            _changed -= onChanged;
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_collectionObserver is not null)
        {
            await _collectionGrain.Unsubscribe(_collectionObserver);
        }
    }

    private class Disposable : IDisposable
    {
        private readonly Action _callback;
        public Disposable(Action callback)
        {
            _callback = callback;
        }

        public void Dispose()
        {
            _callback();
        }
    }
}

