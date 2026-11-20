namespace HustleLink.Models;

public class ServiceRating
{
    public Guid VideoPostId { get; set; }
    public required VideoPost VideoPost { get; set; }
    public Guid CreatorId { get; set; }
    public required Creator Creator { get; set; }
    public int Rating { get; set; }
    public DateTimeOffset RatedAt { get; set; }
}
