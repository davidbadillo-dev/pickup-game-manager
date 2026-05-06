using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PickupGameManager.Web.Data;
using PickupGameManager.Web.Models;

namespace PickupGameManager.Web.Pages.Games;

public class CreateModel(AppDbContext db) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
        Input.ScheduledAtLocal = RoundToMinutes(DateTime.Now.AddHours(1));
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        var utc = DateTime.SpecifyKind(Input.ScheduledAtLocal, DateTimeKind.Local).ToUniversalTime();

        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = Input.Title.Trim(),
            Location = Input.Location.Trim(),
            ScheduledAt = utc,
            MaxPlayers = Input.MaxPlayers,
            CreatedAt = DateTime.UtcNow,
        };

        db.Games.Add(game);
        await db.SaveChangesAsync(cancellationToken);

        return RedirectToPage("/Games/Details", new { id = game.Id });
    }

    private static DateTime RoundToMinutes(DateTime dt) =>
        new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, DateTimeKind.Local);

    public sealed class InputModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date & time")]
        public DateTime ScheduledAtLocal { get; set; }

        [Required]
        [Range(1, 500)]
        [Display(Name = "Max players")]
        public int MaxPlayers { get; set; } = 10;
    }
}
