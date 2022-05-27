namespace Orleans.Collections;

public interface ICollectionObserver : IGrainObserver
{
    Task OnCollectionChanged();
}
