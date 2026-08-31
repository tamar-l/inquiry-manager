namespace InquiryManager.API.Models;

public enum InquiryStatus { New, InProgress, Waiting, Completed }
public enum InquiryPriority { Low, Medium, High }

public class Inquiry
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public InquiryStatus Status { get; set; }
    public InquiryPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
