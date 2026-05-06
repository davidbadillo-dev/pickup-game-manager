namespace PickupGameManager.Web.Models;

/// <summary>Pickup game scheduled by an organizer.</summary>
public class Game
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    /// <summary>When the game starts (stored as UTC).</summary>
    public DateTime ScheduledAt { get; set; }

    public int MaxPlayers { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
}
