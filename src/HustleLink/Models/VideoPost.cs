namespace HustleLink.Models;

public class VideoPost
{
    public Guid Id { get; set; }
    public Guid CreatorId { get; set; }
    public required Creator Creator { get; set; }
    public required string Caption { get; set; }
    public required string VideoUrl { get; set; }
    public required string ThumbnailUrl { get; set; }
    public DateTimeOffset PostedAt { get; set; }
    public int ViewCount { get; set; }
}
