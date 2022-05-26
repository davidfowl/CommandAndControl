
using Orleans;
using Orleans.Collections;

public class DistributedObservableCollection<T>
{
    private readonly ICollectionGrain<T> _collectionGrain;
    private readonly IGrainFactory _grainFactory;
    private ICollectionObserver? _collectionObserverReference;
    private Func<Task>? _changed;
    private readonly Observer _observer;

    public DistributedObservableCollection(IGrainFactory grainFactory, long key)
    {
        _grainFactory = grainFactory;
        _collectionGrain = grainFactory.GetGrain<ICollectionGrain<T>>(key);
        _observer = new Observer(this);
    }

    public Task AddItem(T item) => _collectionGrain.AddItem(item);
    public Task RemoveItem(T item) => _collectionGrain.RemoveItem(item);
    public Task<T[]> GetItems() => _collectionGrain.GetItems();

    private Task OnCollectionChanged()
    {
        return _changed?.Invoke() ?? Task.CompletedTask;
    }

    public async Task<IDisposable> SubscribeAsync(Func<Task> onChanged)
    {
        _changed += onChanged;

        if (_collectionObserverReference is null)
        {
            _collectionObserverReference = await _grainFactory.CreateObjectReference<ICollectionObserver>(_observer);
            await _collectionGrain.Subscribe(_collectionObserverReference);
        }

        return new Disposable(() =>
        {
            _changed -= onChanged;
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_collectionObserverReference is not null)
        {
            await _collectionGrain.Unsubscribe(_collectionObserverReference);
        }
    }

    private class Observer : ICollectionObserver
    {
        private readonly DistributedObservableCollection<T> _parent;
        public Observer(DistributedObservableCollection<T> parent)
        {
            _parent = parent;
        }

        public Task OnCollectionChanged() => _parent.OnCollectionChanged();
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

