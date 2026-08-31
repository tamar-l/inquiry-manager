using Microsoft.EntityFrameworkCore;
using InquiryManager.API.Data;
using InquiryManager.API.DTOs;
using InquiryManager.API.Models;

namespace InquiryManager.API.Services;

public interface IInquiryService
{
    Task<PagedResult<InquiryDto>> GetInquiriesAsync(InquiryQueryParams q);
    Task<InquirySummary> GetSummaryAsync();
    Task<(InquiryDto? Result, UpdateStatusError Error)> UpdateStatusAsync(int id, UpdateStatusRequest request);
}

public class InquiryService(AppDbContext db, ILogger<InquiryService> logger) : IInquiryService
{
    public async Task<PagedResult<InquiryDto>> GetInquiriesAsync(InquiryQueryParams q)
    {
        var query = db.Inquiries.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(i => i.Title.Contains(q.Search) || i.OrganizationName.Contains(q.Search));
        if (q.Status.HasValue)
            query = query.Where(i => i.Status == q.Status.Value);
        if (q.Priority.HasValue)
            query = query.Where(i => i.Priority == q.Priority.Value);

        query = q.SortBy.ToLower() switch
        {
            "title"            => q.SortDesc ? query.OrderByDescending(i => i.Title) : query.OrderBy(i => i.Title),
            "organizationname" => q.SortDesc ? query.OrderByDescending(i => i.OrganizationName) : query.OrderBy(i => i.OrganizationName),
            "status"           => q.SortDesc ? query.OrderByDescending(i => i.Status) : query.OrderBy(i => i.Status),
            "priority"         => q.SortDesc ? query.OrderByDescending(i => i.Priority) : query.OrderBy(i => i.Priority),
            "updatedat"        => q.SortDesc ? query.OrderByDescending(i => i.UpdatedAt) : query.OrderBy(i => i.UpdatedAt),
            _                  => q.SortDesc ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(i => new InquiryDto(i.Id, i.Title, i.OrganizationName, i.Status.ToString(), i.Priority.ToString(), i.CreatedAt, i.UpdatedAt))
            .ToListAsync();

        return new PagedResult<InquiryDto>(items, totalCount, q.Page, q.PageSize);
    }

    public async Task<InquirySummary> GetSummaryAsync()
    {
        var groups = await db.Inquiries.AsNoTracking()
            .GroupBy(i => new { i.Status, i.Priority })
            .Select(g => new { g.Key.Status, g.Key.Priority, Count = g.Count() })
            .ToListAsync();

        var byStatus   = groups.GroupBy(g => g.Status.ToString()).ToDictionary(g => g.Key, g => g.Sum(x => x.Count));
        var byPriority = groups.GroupBy(g => g.Priority.ToString()).ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

        return new InquirySummary(groups.Sum(g => g.Count), byStatus, byPriority);
    }

    public async Task<(InquiryDto? Result, UpdateStatusError Error)> UpdateStatusAsync(int id, UpdateStatusRequest request)
    {
        var inquiry = await db.Inquiries.FirstOrDefaultAsync(i => i.Id == id);

        if (inquiry is null)
            return (null, UpdateStatusError.NotFound);

        if (!Enum.TryParse<InquiryStatus>(request.Status, ignoreCase: true, out var parsedStatus))
            return (null, UpdateStatusError.InvalidStatus);

        if (inquiry.Status == parsedStatus)
            return (ToDto(inquiry), UpdateStatusError.None);

        try
        {
            inquiry.Status    = parsedStatus;
            inquiry.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return (ToDto(inquiry), UpdateStatusError.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update status for inquiry {Id}", id);
            return (null, UpdateStatusError.ServerError);
        }
    }

    private static InquiryDto ToDto(Inquiry i) =>
        new(i.Id, i.Title, i.OrganizationName, i.Status.ToString(), i.Priority.ToString(), i.CreatedAt, i.UpdatedAt);
}
