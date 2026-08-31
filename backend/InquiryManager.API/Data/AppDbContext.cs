using Microsoft.EntityFrameworkCore;
using InquiryManager.API.Models;

namespace InquiryManager.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inquiry>(e =>
        {
            e.HasIndex(i => i.Status);
            e.HasIndex(i => i.Priority);
            e.HasIndex(i => i.CreatedAt);
            e.HasIndex(i => i.OrganizationName);
            e.HasIndex(i => i.Title);
        });

        var organizations = new[] { "אלפא בע\"מ", "בטא טכנולוגיות", "גמא פתרונות", "דלתא מערכות", "אפסילון קבוצה" };
        var titles = new[] { "בקשת תמיכה", "דיווח תקלה", "בקשת מידע", "פנייה כללית", "בקשת שירות" };
        var statuses = Enum.GetValues<InquiryStatus>();
        var priorities = Enum.GetValues<InquiryPriority>();
        var rng = new Random(42);
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var inquiries = Enumerable.Range(1, 10000).Select(i =>
        {
            var created = baseDate.AddMinutes(rng.Next(0, 525600));
            return new Inquiry
            {
                Id = i,
                Title = $"{titles[rng.Next(titles.Length)]} #{i}",
                OrganizationName = organizations[rng.Next(organizations.Length)],
                Status = statuses[rng.Next(statuses.Length)],
                Priority = priorities[rng.Next(priorities.Length)],
                CreatedAt = created,
                UpdatedAt = created.AddMinutes(rng.Next(0, 10080))
            };
        }).ToArray();

        modelBuilder.Entity<Inquiry>().HasData(inquiries);
    }
}
