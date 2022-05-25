using Orleans;

public interface ICollectionObserver : IGrainObserver
{
    Task OnCollectionChanged();
}
