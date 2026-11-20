namespace HustleLink.Models;

public class Creator
{
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public required string Handle { get; set; }
    public required string Skill { get; set; }
    public required string Location { get; set; }
    public required string AvatarUrl { get; set; }
    public required string ContactEmail { get; set; }
    public bool IsCurrentUser { get; set; }
    public ICollection<VideoPost> Videos { get; set; } = new List<VideoPost>();
    public ICollection<CreatorFollow> Followers { get; set; } = new List<CreatorFollow>();
    public ICollection<CreatorFollow> Following { get; set; } = new List<CreatorFollow>();
}
