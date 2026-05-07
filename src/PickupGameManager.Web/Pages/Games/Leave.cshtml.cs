using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PickupGameManager.Web.Data;
using PickupGameManager.Web.Models;

namespace PickupGameManager.Web.Pages.Games;

public class LeaveModel(AppDbContext db) : PageModel
{
    [BindProperty]
    public string PlayerName { get; set; } = string.Empty;

    public IActionResult OnGet(Guid id) =>
        RedirectToPage("/Games/Details", new { id });

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var gameExists = await db.Games.AnyAsync(g => g.Id == id, cancellationToken);
        if (!gameExists)
            return NotFound();

        PlayerName = (PlayerName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(PlayerName))
        {
            TempData["LeaveNotice"] = "Enter your name to leave.";
            TempData["LeaveNoticeName"] = string.Empty;
            return RedirectToPage("/Games/Details", new { id });
        }

        var roster = await db.Participants
            .Where(p => p.GameId == id)
            .ToListAsync(cancellationToken);

        var participant = roster.FirstOrDefault(p =>
            string.Equals(p.Name, PlayerName, StringComparison.OrdinalIgnoreCase));

        if (participant is null)
        {
            TempData["LeaveNotice"] = "No one with that name is on this game.";
            TempData["LeaveNoticeName"] = PlayerName;
            return RedirectToPage("/Games/Details", new { id });
        }

        if (participant.Status == ParticipantStatus.Going)
        {
            db.Participants.Remove(participant);

            var promote = roster
                .Where(p => p.Status == ParticipantStatus.Waitlist)
                .OrderBy(p => p.CreatedAt)
                .ThenBy(p => p.Id)
                .FirstOrDefault();

            if (promote is not null)
                promote.Status = ParticipantStatus.Going;
        }
        else
            db.Participants.Remove(participant);

        await db.SaveChangesAsync(cancellationToken);

        TempData["LeaveNotice"] = "You've left the game.";
        TempData["LeaveNoticeName"] = PlayerName;
        return RedirectToPage("/Games/Details", new { id });
    }
}
