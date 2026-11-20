using HustleLink.Models;
using Microsoft.EntityFrameworkCore;

namespace HustleLink.Data;

public static class DatabaseInitializer
{
    public static readonly Guid CurrentCreatorId = Guid.Parse("a0000000-0000-0000-0000-000000000001");

    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync(
            """ALTER TABLE "Creators" ADD COLUMN IF NOT EXISTS "ContactEmail" character varying(254) NOT NULL DEFAULT '';""");
        await db.Database.ExecuteSqlRawAsync(
            """
            UPDATE "Creators"
            SET "ContactEmail" = CASE "Handle"
                WHEN 'your_hustle' THEN 'you@example.com'
                WHEN 'achieng.woodworks' THEN 'achieng@example.com'
                WHEN 'musa.bakes' THEN 'musa@example.com'
                WHEN 'wanjiku.curls' THEN 'wanjiku@example.com'
                WHEN 'brian.builds' THEN 'brian@example.com'
                ELSE "ContactEmail"
            END
            WHERE "ContactEmail" = '';
            """);
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "VideoLikes" (
                "VideoPostId" uuid NOT NULL REFERENCES "VideoPosts" ("Id") ON DELETE CASCADE,
                "CreatorId" uuid NOT NULL REFERENCES "Creators" ("Id") ON DELETE CASCADE,
                "LikedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_VideoLikes" PRIMARY KEY ("VideoPostId", "CreatorId")
            );
            CREATE INDEX IF NOT EXISTS "IX_VideoLikes_CreatorId" ON "VideoLikes" ("CreatorId");

            CREATE TABLE IF NOT EXISTS "ServiceRatings" (
                "VideoPostId" uuid NOT NULL REFERENCES "VideoPosts" ("Id") ON DELETE CASCADE,
                "CreatorId" uuid NOT NULL REFERENCES "Creators" ("Id") ON DELETE CASCADE,
                "Rating" integer NOT NULL,
                "RatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ServiceRatings" PRIMARY KEY ("VideoPostId", "CreatorId"),
                CONSTRAINT "CK_ServiceRatings_Rating" CHECK ("Rating" >= 1 AND "Rating" <= 5)
            );
            CREATE INDEX IF NOT EXISTS "IX_ServiceRatings_CreatorId" ON "ServiceRatings" ("CreatorId");
            """);

        if (await db.Creators.AnyAsync())
        {
            return;
        }

        var creators = new[]
        {
            new Creator
            {
                Id = CurrentCreatorId,
                DisplayName = "You",
                Handle = "your_hustle",
                Skill = "Your creative journey",
                Location = "Nairobi, Kenya",
                AvatarUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=160&q=80",
                ContactEmail = "you@example.com",
                IsCurrentUser = true
            },
            new Creator
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000002"),
                DisplayName = "Achieng Otieno",
                Handle = "achieng.woodworks",
                Skill = "Furniture maker",
                Location = "Nairobi, Kenya",
                AvatarUrl = "https://images.unsplash.com/photo-1531123897727-8f129e1688ce?auto=format&fit=crop&w=160&q=80",
                ContactEmail = "achieng@example.com"
            },
            new Creator
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000003"),
                DisplayName = "Musa Kiptoo",
                Handle = "musa.bakes",
                Skill = "Baker",
                Location = "Kisumu, Kenya",
                AvatarUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=160&q=80",
                ContactEmail = "musa@example.com"
            },
            new Creator
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000004"),
                DisplayName = "Wanjiku Njeri",
                Handle = "wanjiku.curls",
                Skill = "Natural hair stylist",
                Location = "Mombasa, Kenya",
                AvatarUrl = "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=160&q=80",
                ContactEmail = "wanjiku@example.com"
            },
            new Creator
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000005"),
                DisplayName = "Brian Mwangi",
                Handle = "brian.builds",
                Skill = "Welding & metalwork",
                Location = "Nakuru, Kenya",
                AvatarUrl = "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=160&q=80",
                ContactEmail = "brian@example.com"
            }
        };

        db.Creators.AddRange(creators);
        await db.SaveChangesAsync();

        var now = DateTimeOffset.UtcNow;
        db.VideoPosts.AddRange(
            CreateVideo(creators[1], "A reclaimed timber table, made by hand from start to finish. Which finish would you choose?", "photo-1533090481720-856c6e3c1fdc", "ForBiggerEscapes", now.AddHours(-2), 1280),
            CreateVideo(creators[2], "Fresh mandazi, straight from the pan. The secret is giving the dough time to rest.", "photo-1509440159596-0249088772ff", "ForBiggerFun", now.AddHours(-5), 846),
            CreateVideo(creators[3], "A protective style that keeps your curls happy. Save this look for your next appointment.", "photo-1522337360788-8b13dee7a37e", "ForBiggerJoyrides", now.AddDays(-1), 2310),
            CreateVideo(creators[4], "Turning a simple sketch into a sturdy custom gate. Built to fit, not just off the shelf.", "photo-1504917595217-d4dc5ebe6122", "ForBiggerMeltdowns", now.AddDays(-1).AddHours(-4), 672));

        db.CreatorFollows.AddRange(
            new CreatorFollow { FollowerId = CurrentCreatorId, Follower = creators[0], FollowedId = creators[1].Id, Followed = creators[1], FollowedAt = now.AddDays(-4) },
            new CreatorFollow { FollowerId = CurrentCreatorId, Follower = creators[0], FollowedId = creators[3].Id, Followed = creators[3], FollowedAt = now.AddDays(-2) });

        await db.SaveChangesAsync();
    }

    private static VideoPost CreateVideo(
        Creator creator,
        string caption,
        string thumbnailId,
        string clip,
        DateTimeOffset postedAt,
        int views) =>
        new()
        {
            Id = Guid.NewGuid(),
            CreatorId = creator.Id,
            Creator = creator,
            Caption = caption,
            VideoUrl = $"https://storage.googleapis.com/gtv-videos-bucket/sample/{clip}.mp4",
            ThumbnailUrl = $"https://images.unsplash.com/{thumbnailId}?auto=format&fit=crop&w=900&q=85",
            PostedAt = postedAt,
            ViewCount = views
        };
}
