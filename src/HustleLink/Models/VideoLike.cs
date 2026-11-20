namespace HustleLink.Models;

public class VideoLike
{
    public Guid VideoPostId { get; set; }
    public required VideoPost VideoPost { get; set; }
    public Guid CreatorId { get; set; }
    public required Creator Creator { get; set; }
    public DateTimeOffset LikedAt { get; set; }
}
