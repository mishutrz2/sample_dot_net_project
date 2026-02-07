using Api.Models;

public class AppUserLeague
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;
    public Guid LeagueId { get; set; }
    public League League { get; set; } = default!;
    public string NicknameInLeague { get; set; } = default!;
    public DateTime JoinedAt { get; set; }

}
