namespace HustleLink.Models;

public class CreatorFollow
{
    public Guid FollowerId { get; set; }
    public required Creator Follower { get; set; }
    public Guid FollowedId { get; set; }
    public required Creator Followed { get; set; }
    public DateTimeOffset FollowedAt { get; set; }
}
