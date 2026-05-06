using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PickupGameManager.Web.Data;
using PickupGameManager.Web.Models;

namespace PickupGameManager.Web.Pages.Games;

public class DetailsModel(AppDbContext db) : PageModel
{
    public Game? Game { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        Game = await db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (Game is null)
            return NotFound();

        return Page();
    }
}
