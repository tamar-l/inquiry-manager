using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using InquiryManager.API.DTOs;
using InquiryManager.API.Models;

namespace InquiryManager.API.Services;

/// <summary>
/// Decorator over IInquiryService שמוסיף Cache על Summary ורשימה.
/// Cache מתבטל מיידית בכל עדכון סטטוס – TTL של 5 דקות משמש כ-safety net בלבד.
/// </summary>
public class CachedInquiryService(InquiryService inner, IMemoryCache cache, ListCacheInvalidator invalidator) : IInquiryService
{
    private const string SummaryCacheKey = "inquiries_summary";
    private const string ListCachePrefix = "inquiries_list_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private string BuildListCacheKey(InquiryQueryParams q) =>
        $"{ListCachePrefix}{q.Search}_{q.Status}_{q.Priority}_{q.Page}_{q.PageSize}_{q.SortBy}_{q.SortDesc}";

    public async Task<PagedResult<InquiryDto>> GetInquiriesAsync(InquiryQueryParams q)
    {
        var key = BuildListCacheKey(q);
        if (cache.TryGetValue(key, out PagedResult<InquiryDto>? cached))
            return cached!;

        var result = await inner.GetInquiriesAsync(q);
        cache.Set(key, result, new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .SetSize(1)
            .AddExpirationToken(invalidator.GetChangeToken()));
        return result;
    }

    public async Task<InquirySummary> GetSummaryAsync()
    {
        if (cache.TryGetValue(SummaryCacheKey, out InquirySummary? cached))
            return cached!;

        var result = await inner.GetSummaryAsync();
        cache.Set(SummaryCacheKey, result, new MemoryCacheEntryOptions().SetAbsoluteExpiration(CacheDuration).SetSize(1));
        return result;
    }

    public async Task<(InquiryDto? Result, UpdateStatusError Error)> UpdateStatusAsync(int id, UpdateStatusRequest request)
    {
        var (result, error) = await inner.UpdateStatusAsync(id, request);

        if (error == UpdateStatusError.None)
        {
            cache.Remove(SummaryCacheKey);
            invalidator.Invalidate();
        }

        return (result, error);
    }
}
