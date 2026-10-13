using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Infrastructure.Messaging;

public sealed class SnsOptions
{
    public const string SectionName = "Sns";

    [Required]
    public string Region { get; set; } = string.Empty;

    [Required]
    public string TopicArn { get; set; } = string.Empty;
    
    public string? ServiceUrl { get; set; }
    
    [Range(typeof(TimeSpan), "00:00:01", "00:01:00")]
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    [Range(typeof(TimeSpan), "00:00:01", "00:00:30")]
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(2);

    [Range(0, 10)]
    public int MaxErrorRetry { get; set; } = 3;
}
