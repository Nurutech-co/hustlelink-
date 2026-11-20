using HustleLink.Data;
using HustleLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HustleLink.Pages;

public class IndexModel(AppDbContext db) : PageModel
{
    public string Feed { get; private set; } = "discover";
    public IReadOnlyList<VideoCardViewModel> Videos { get; private set; } = [];
    public IReadOnlyList<CreatorCardViewModel> Suggestions { get; private set; } = [];
    public string? Message => TempData["Message"] as string;

    public async Task OnGetAsync(string feed = "discover")
    {
        Feed = feed == "following" ? "following" : "discover";
        var followingIds = await db.CreatorFollows
            .Where(follow => follow.FollowerId == DatabaseInitializer.CurrentCreatorId)
            .Select(follow => follow.FollowedId)
            .ToListAsync();

        var posts = db.VideoPosts
            .Include(post => post.Creator)
            .AsQueryable();
        if (Feed == "following")
        {
            posts = posts.Where(post => followingIds.Contains(post.CreatorId));
        }

        var feedPosts = await posts
            .OrderByDescending(post => post.PostedAt)
            .ToListAsync();
        var postIds = feedPosts.Select(post => post.Id).ToArray();
        var likeCounts = await db.VideoLikes
            .Where(like => postIds.Contains(like.VideoPostId))
            .GroupBy(like => like.VideoPostId)
            .Select(group => new { PostId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.PostId, item => item.Count);
        var ratingSummaries = await db.ServiceRatings
            .Where(rating => postIds.Contains(rating.VideoPostId))
            .GroupBy(rating => rating.VideoPostId)
            .Select(group => new
            {
                PostId = group.Key,
                Average = group.Average(rating => rating.Rating),
                Count = group.Count()
            })
            .ToDictionaryAsync(item => item.PostId, item => (item.Average, item.Count));
        var likedPostIds = await db.VideoLikes
            .Where(like => like.CreatorId == DatabaseInitializer.CurrentCreatorId
                && postIds.Contains(like.VideoPostId))
            .Select(like => like.VideoPostId)
            .ToHashSetAsync();
        var userRatings = await db.ServiceRatings
            .Where(rating => rating.CreatorId == DatabaseInitializer.CurrentCreatorId
                && postIds.Contains(rating.VideoPostId))
            .ToDictionaryAsync(rating => rating.VideoPostId, rating => rating.Rating);

        Videos = feedPosts.Select(post =>
        {
            var summary = ratingSummaries.GetValueOrDefault(post.Id);
            var userRating = userRatings.TryGetValue(post.Id, out var savedRating)
                ? savedRating
                : (int?)null;
            return new VideoCardViewModel
            {
                Post = post,
                IsFollowing = followingIds.Contains(post.CreatorId),
                IsLiked = likedPostIds.Contains(post.Id),
                LikeCount = likeCounts.GetValueOrDefault(post.Id),
                AverageRating = summary.Average,
                RatingCount = summary.Count,
                UserRating = userRating
            };
        }).ToList();

        Suggestions = await db.Creators
            .Where(creator => !creator.IsCurrentUser && !followingIds.Contains(creator.Id))
            .Select(creator => new CreatorCardViewModel
            {
                Creator = creator,
                FollowerCount = creator.Followers.Count
            })
            .OrderByDescending(creator => creator.FollowerCount)
            .ThenBy(creator => creator.Creator.Handle)
            .Take(3)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostToggleFollowAsync(Guid creatorId, string feed = "discover")
    {
        if (creatorId == DatabaseInitializer.CurrentCreatorId)
        {
            return BadRequest();
        }

        var creator = await db.Creators.FindAsync(creatorId);
        if (creator is null)
        {
            return NotFound();
        }

        var currentCreator = await db.Creators.FindAsync(DatabaseInitializer.CurrentCreatorId);
        if (currentCreator is null)
        {
            return NotFound();
        }

        var follow = await db.CreatorFollows.FindAsync(currentCreator.Id, creator.Id);
        if (follow is null)
        {
            db.CreatorFollows.Add(new CreatorFollow
            {
                FollowerId = currentCreator.Id,
                Follower = currentCreator,
                FollowedId = creator.Id,
                Followed = creator,
                FollowedAt = DateTimeOffset.UtcNow
            });
            TempData["Message"] = $"You are now following {creator.DisplayName}.";
        }
        else
        {
            db.CreatorFollows.Remove(follow);
            TempData["Message"] = $"You unfollowed {creator.DisplayName}.";
        }

        await db.SaveChangesAsync();
        return RedirectToPage(new { feed = feed == "following" ? "following" : "discover" });
    }

    public async Task<IActionResult> OnPostToggleLikeAsync(Guid videoPostId, string feed = "discover")
    {
        var post = await db.VideoPosts.FindAsync(videoPostId);
        var creator = await db.Creators.FindAsync(DatabaseInitializer.CurrentCreatorId);
        if (post is null || creator is null)
        {
            return NotFound();
        }

        var like = await db.VideoLikes.FindAsync(videoPostId, creator.Id);
        if (like is null)
        {
            db.VideoLikes.Add(new VideoLike
            {
                VideoPostId = post.Id,
                VideoPost = post,
                CreatorId = creator.Id,
                Creator = creator,
                LikedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            db.VideoLikes.Remove(like);
        }

        await db.SaveChangesAsync();
        return RedirectToPage(new { feed = feed == "following" ? "following" : "discover" });
    }

    public async Task<IActionResult> OnPostRateServiceAsync(Guid videoPostId, int rating, string feed = "discover")
    {
        if (rating is < 1 or > 5)
        {
            TempData["Message"] = "Choose a rating from 1 to 5 stars.";
            return RedirectToPage(new { feed = feed == "following" ? "following" : "discover" });
        }

        var post = await db.VideoPosts.FindAsync(videoPostId);
        var creator = await db.Creators.FindAsync(DatabaseInitializer.CurrentCreatorId);
        if (post is null || creator is null)
        {
            return NotFound();
        }

        var serviceRating = await db.ServiceRatings.FindAsync(videoPostId, creator.Id);
        if (serviceRating is null)
        {
            db.ServiceRatings.Add(new ServiceRating
            {
                VideoPostId = post.Id,
                VideoPost = post,
                CreatorId = creator.Id,
                Creator = creator,
                Rating = rating,
                RatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            serviceRating.Rating = rating;
            serviceRating.RatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync();
        TempData["Message"] = "Your service rating has been saved.";
        return RedirectToPage(new { feed = feed == "following" ? "following" : "discover" });
    }
}
