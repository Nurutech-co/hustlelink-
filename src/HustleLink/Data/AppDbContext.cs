using HustleLink.Models;
using Microsoft.EntityFrameworkCore;

namespace HustleLink.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Creator> Creators => Set<Creator>();
    public DbSet<VideoPost> VideoPosts => Set<VideoPost>();
    public DbSet<CreatorFollow> CreatorFollows => Set<CreatorFollow>();
    public DbSet<VideoLike> VideoLikes => Set<VideoLike>();
    public DbSet<ServiceRating> ServiceRatings => Set<ServiceRating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Creator>(entity =>
        {
            entity.HasKey(creator => creator.Id);
            entity.HasIndex(creator => creator.Handle).IsUnique();
            entity.Property(creator => creator.DisplayName).HasMaxLength(100);
            entity.Property(creator => creator.Handle).HasMaxLength(40);
            entity.Property(creator => creator.Skill).HasMaxLength(80);
            entity.Property(creator => creator.Location).HasMaxLength(100);
            entity.Property(creator => creator.AvatarUrl).HasMaxLength(500);
            entity.Property(creator => creator.ContactEmail).HasMaxLength(254);
        });

        modelBuilder.Entity<VideoPost>(entity =>
        {
            entity.HasKey(post => post.Id);
            entity.Property(post => post.Caption).HasMaxLength(500);
            entity.Property(post => post.VideoUrl).HasMaxLength(1000);
            entity.Property(post => post.ThumbnailUrl).HasMaxLength(1000);
            entity.HasOne(post => post.Creator)
                .WithMany(creator => creator.Videos)
                .HasForeignKey(post => post.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CreatorFollow>(entity =>
        {
            entity.HasKey(follow => new { follow.FollowerId, follow.FollowedId });
            entity.HasOne(follow => follow.Follower)
                .WithMany(creator => creator.Following)
                .HasForeignKey(follow => follow.FollowerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(follow => follow.Followed)
                .WithMany(creator => creator.Followers)
                .HasForeignKey(follow => follow.FollowedId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VideoLike>(entity =>
        {
            entity.HasKey(like => new { like.VideoPostId, like.CreatorId });
            entity.HasOne(like => like.VideoPost)
                .WithMany()
                .HasForeignKey(like => like.VideoPostId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(like => like.Creator)
                .WithMany()
                .HasForeignKey(like => like.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceRating>(entity =>
        {
            entity.HasKey(rating => new { rating.VideoPostId, rating.CreatorId });
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_ServiceRatings_Rating",
                "\"Rating\" >= 1 AND \"Rating\" <= 5"));
            entity.HasOne(rating => rating.VideoPost)
                .WithMany()
                .HasForeignKey(rating => rating.VideoPostId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rating => rating.Creator)
                .WithMany()
                .HasForeignKey(rating => rating.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
