using Microsoft.Extensions.Primitives;

namespace InquiryManager.API.Services;

/// <summary>
/// Singleton שמחזיק את ה-CancellationToken לביטול list cache.
/// חייב להיות Singleton כדי שה-token יהיה משותף בין כל ה-requests.
/// </summary>
public class ListCacheInvalidator
{
    private CancellationTokenSource _cts = new();
    private readonly object _lock = new();

    public IChangeToken GetChangeToken() => new CancellationChangeToken(_cts.Token);

    public void Invalidate()
    {
        lock (_lock)
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();
        }
    }
}
