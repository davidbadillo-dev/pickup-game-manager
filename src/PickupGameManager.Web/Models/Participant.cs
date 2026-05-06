namespace PickupGameManager.Web.Models;

public class Participant
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public ParticipantStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
