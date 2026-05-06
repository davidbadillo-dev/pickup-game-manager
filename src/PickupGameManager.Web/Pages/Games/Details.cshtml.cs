using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PickupGameManager.Web.Data;
using PickupGameManager.Web.Models;

namespace PickupGameManager.Web.Pages.Games;

public class DetailsModel(AppDbContext db) : PageModel
{
    public Game? Game { get; private set; }

    [BindProperty]
    public JoinInput Input { get; set; } = new();

    public int GoingCount { get; private set; }
    public int SpotsRemaining { get; private set; }
    public IReadOnlyList<Participant> GoingPlayers { get; private set; } = [];
    public IReadOnlyList<Participant> WaitlistedPlayers { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        await LoadDisplayAsync(id, cancellationToken);
        if (Game is null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostJoinAsync(Guid id, CancellationToken cancellationToken)
    {
        var gameMeta = await db.Games.AsNoTracking()
            .Where(g => g.Id == id)
            .Select(g => new { g.MaxPlayers })
            .FirstOrDefaultAsync(cancellationToken);

        if (gameMeta is null)
            return NotFound();

        Input.PlayerName = Input.PlayerName.Trim();
        if (string.IsNullOrWhiteSpace(Input.PlayerName))
            ModelState.AddModelError("Input.PlayerName", "Enter your name.");

        if (!ModelState.IsValid)
        {
            await LoadDisplayAsync(id, cancellationToken);
            return Page();
        }

        var goingCount = await db.Participants.CountAsync(
            p => p.GameId == id && p.Status == ParticipantStatus.Going,
            cancellationToken);

        var status = goingCount < gameMeta.MaxPlayers ? ParticipantStatus.Going : ParticipantStatus.Waitlist;

        db.Participants.Add(new Participant
        {
            Id = Guid.NewGuid(),
            GameId = id,
            Name = Input.PlayerName,
            Status = status,
            CreatedAt = DateTime.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);

        return RedirectToPage("/Games/Details", new { id });
    }

    private async Task LoadDisplayAsync(Guid id, CancellationToken cancellationToken)
    {
        Game = await db.Games
            .AsNoTracking()
            .Include(g => g.Participants)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        if (Game is null)
            return;

        PopulateLists(Game);
    }

    private void PopulateLists(Game game)
    {
        GoingCount = game.Participants.Count(p => p.Status == ParticipantStatus.Going);
        SpotsRemaining = Math.Max(0, game.MaxPlayers - GoingCount);
        GoingPlayers = game.Participants
            .Where(p => p.Status == ParticipantStatus.Going)
            .OrderBy(p => p.CreatedAt)
            .ToList();
        WaitlistedPlayers = game.Participants
            .Where(p => p.Status == ParticipantStatus.Waitlist)
            .OrderBy(p => p.CreatedAt)
            .ToList();
    }

    public sealed class JoinInput
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Your name")]
        public string PlayerName { get; set; } = string.Empty;
    }
}
