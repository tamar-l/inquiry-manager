using System.ComponentModel.DataAnnotations;
using InquiryManager.API.Models;

namespace InquiryManager.API.DTOs;

public record InquiryDto(
    int Id,
    string Title,
    string OrganizationName,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record InquirySummary(
    int Total,
    Dictionary<string, int> ByStatus,
    Dictionary<string, int> ByPriority
);

public class InquiryQueryParams
{
    public string? Search { get; set; }
    public InquiryStatus? Status { get; set; }
    public InquiryPriority? Priority { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public bool SortDesc { get; set; } = false;
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}

public record UpdateStatusRequest(
    [Required] string Status
);
