namespace HustleLink.Models;

public class VideoFeedViewModel
{
    public required string Feed { get; init; }
    public required IReadOnlyList<VideoCardViewModel> Videos { get; init; }
    public required IReadOnlyList<CreatorCardViewModel> Suggestions { get; init; }
}

public class VideoCardViewModel
{
    public required VideoPost Post { get; init; }
    public bool IsFollowing { get; init; }
    public bool IsLiked { get; init; }
    public int LikeCount { get; init; }
    public double AverageRating { get; init; }
    public int RatingCount { get; init; }
    public int? UserRating { get; init; }
}

public class CreatorCardViewModel
{
    public required Creator Creator { get; init; }
    public int FollowerCount { get; init; }
}
