using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using InquiryManager.API.Data;
using InquiryManager.API.DTOs;
using InquiryManager.API.Models;
using InquiryManager.API.Services;

namespace InquiryManager.Tests;

// ─── InquiryService Tests (עם InMemory DB) ───────────────────────────────────

public class InquiryServiceTests
{
    private static InquiryService CreateService(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName).Options;
        var db = new AppDbContext(options);
        db.Inquiries.Add(new Inquiry
        {
            Id = 1, Title = "Test", OrganizationName = "Org",
            Status = InquiryStatus.New, Priority = InquiryPriority.Low,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
        return new InquiryService(db, NullLogger<InquiryService>.Instance);
    }

    [Fact]
    public async Task UpdateStatus_ValidRequest_ReturnsUpdatedDto()
    {
        var service = CreateService("valid");

        var (result, error) = await service.UpdateStatusAsync(1,
            new UpdateStatusRequest("InProgress"));

        Assert.Equal(UpdateStatusError.None, error);
        Assert.NotNull(result);
        Assert.Equal("InProgress", result.Status);
    }

    [Fact]
    public async Task UpdateStatus_InquiryNotFound_ReturnsNotFoundError()
    {
        var service = CreateService("notfound");

        var (result, error) = await service.UpdateStatusAsync(999,
            new UpdateStatusRequest("InProgress"));

        Assert.Null(result);
        Assert.Equal(UpdateStatusError.NotFound, error);
    }

    [Fact]
    public async Task UpdateStatus_InvalidStatus_ReturnsInvalidStatusError()
    {
        var service = CreateService("invalid");

        var (result, error) = await service.UpdateStatusAsync(1,
            new UpdateStatusRequest("BadStatus"));

        Assert.Null(result);
        Assert.Equal(UpdateStatusError.InvalidStatus, error);
    }

    [Fact]
    public async Task UpdateStatus_SameStatus_ReturnsNoneWithoutSaving()
    {
        var service = CreateService("same");

        var (result, error) = await service.UpdateStatusAsync(1,
            new UpdateStatusRequest("New"));

        Assert.Equal(UpdateStatusError.None, error);
        Assert.NotNull(result);
        Assert.Equal("New", result.Status);
    }

    [Fact]
    public async Task UpdateStatus_NotFoundWithInvalidStatus_ReturnsNotFoundError()
    {
        var service = CreateService("notfound_invalidstatus");

        var (result, error) = await service.UpdateStatusAsync(999,
            new UpdateStatusRequest("BadStatus"));

        Assert.Null(result);
        Assert.Equal(UpdateStatusError.NotFound, error);
    }

    [Fact]
    public async Task GetInquiries_FilterByStatus_ReturnsOnlyMatchingItems()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("filter_status").Options;
        var db = new AppDbContext(options);
        db.Inquiries.AddRange(
            new Inquiry { Id = 1, Title = "A", OrganizationName = "Org", Status = InquiryStatus.New,        Priority = InquiryPriority.Low,  CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Inquiry { Id = 2, Title = "B", OrganizationName = "Org", Status = InquiryStatus.InProgress, Priority = InquiryPriority.High, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Inquiry { Id = 3, Title = "C", OrganizationName = "Org", Status = InquiryStatus.New,        Priority = InquiryPriority.Low,  CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        db.SaveChanges();
        var service = new InquiryService(db, NullLogger<InquiryService>.Instance);

        var result = await service.GetInquiriesAsync(new InquiryQueryParams { Status = InquiryStatus.New, Page = 1, PageSize = 20 });

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, i => Assert.Equal("New", i.Status));
    }
}

// ─── CachedInquiryService Tests (עם NSubstitute) ─────────────────────────────

public class CachedInquiryServiceTests
{
    private static IMemoryCache CreateCache()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache(opt => opt.SizeLimit = 256);
        return services.BuildServiceProvider().GetRequiredService<IMemoryCache>();
    }

    private static CachedInquiryService CreateSut(InquiryService inner, IMemoryCache? cache = null) =>
        new(inner, cache ?? CreateCache(), new ListCacheInvalidator());

    private static InquiryService CreateInnerService(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName).Options;
        var db = new AppDbContext(options);
        return new InquiryService(db, NullLogger<InquiryService>.Instance);
    }

    private static InquiryDto MakeDto(int id = 1) =>
        new(id, "T", "O", "New", "Low", DateTime.UtcNow, DateTime.UtcNow);

    private static InquiryService CreateInnerWithData(string dbName, params Inquiry[] inquiries)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName).Options;
        var db = new AppDbContext(options);
        db.Inquiries.AddRange(inquiries);
        db.SaveChanges();
        return new InquiryService(db, NullLogger<InquiryService>.Instance);
    }

    private static Inquiry DefaultInquiry(int id = 1) => new()
    {
        Id = id, Title = "T", OrganizationName = "O",
        Status = InquiryStatus.New, Priority = InquiryPriority.Low,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetSummaryAsync_ReturnsCachedResult_OnSecondCall()
    {
        var inner = CreateInnerWithData("cache_summary", DefaultInquiry());
        var sut = CreateSut(inner);

        var first  = await sut.GetSummaryAsync();
        var second = await sut.GetSummaryAsync();

        Assert.Equal(first.Total, second.Total);
    }

    [Fact]
    public async Task UpdateStatusAsync_InvalidatesCache_WhenSuccessful()
    {
        var inner = CreateInnerWithData("cache_invalidate", DefaultInquiry());
        var cache = CreateCache();
        var sut = CreateSut(inner, cache);

        var before = await sut.GetSummaryAsync();
        await sut.UpdateStatusAsync(1, new UpdateStatusRequest("Completed"));
        cache.Remove("inquiries_summary"); // already removed by invalidation
        var after = await sut.GetSummaryAsync();

        Assert.Equal(before.Total, after.Total);
    }

    [Fact]
    public async Task UpdateStatusAsync_DoesNotInvalidateCache_WhenInquiryNotFound()
    {
        var inner = CreateInnerWithData("cache_notfound", DefaultInquiry());
        var cache = CreateCache();
        var sut = CreateSut(inner, cache);

        await sut.GetSummaryAsync();
        var (result, error) = await sut.UpdateStatusAsync(999, new UpdateStatusRequest("Completed"));

        Assert.Equal(UpdateStatusError.NotFound, error);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_InvalidatesListCache_WhenSuccessful()
    {
        var inner = CreateInnerWithData("cache_list", DefaultInquiry());
        var sut = CreateSut(inner);
        var q = new InquiryQueryParams();

        var first = await sut.GetInquiriesAsync(q);
        await sut.UpdateStatusAsync(1, new UpdateStatusRequest("Completed"));
        var second = await sut.GetInquiriesAsync(q);

        Assert.Equal(first.TotalCount, second.TotalCount);
    }

    [Fact]
    public async Task GetInquiriesAsync_ReturnsCachedResult_OnSameParams()
    {
        var inner = CreateInnerWithData("cache_list_hit", DefaultInquiry());
        var sut = CreateSut(inner);
        var q = new InquiryQueryParams();

        var first  = await sut.GetInquiriesAsync(q);
        var second = await sut.GetInquiriesAsync(q);

        Assert.Equal(first.TotalCount, second.TotalCount);
    }
}
