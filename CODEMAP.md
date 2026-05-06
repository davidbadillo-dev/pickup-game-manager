# Code map — Pickup Game Manager

Quick reference for navigating the repository. Behavior is described as implemented today (Day 1 MVP: games only; no participants yet).

## Main folders and files

| Location | Purpose |
|----------|---------|
| `PickupGameManager.sln` | Solution; contains `src/PickupGameManager.Web`. |
| `PROJECT_CONTEXT.md` | Product goals, data model target, business rules (full MVP). |
| `TODO.md` | Phased work (Day 1–3). |
| `src/PickupGameManager.Web/PickupGameManager.Web.csproj` | Web app; Razor Pages, EF Core SQLite packages. |
| `src/PickupGameManager.Web/Program.cs` | App host: services, DB migration at startup, Razor endpoint mapping. |
| `src/PickupGameManager.Web/appsettings.json` | `ConnectionStrings:DefaultConnection` → SQLite file `pickup.db`. |
| `src/PickupGameManager.Web/Properties/launchSettings.json` | Dev URL profile (e.g. `http` on port 5245). |
| `src/PickupGameManager.Web/Models/` | Domain entities (`Game.cs`). |
| `src/PickupGameManager.Web/Data/AppDbContext.cs` | EF Core `DbContext`; `OnModelCreating` rules. |
| `src/PickupGameManager.Web/Data/Migrations/` | EF migrations and `AppDbContextModelSnapshot.cs`. |
| `src/PickupGameManager.Web/Pages/` | Razor Pages (`.cshtml`) and page models (`*.cshtml.cs`). |
| `src/PickupGameManager.Web/Pages/Shared/` | `_Layout.cshtml`, validation partials, etc. |
| `src/PickupGameManager.Web/wwwroot/` | Static assets (CSS, JS, lib). |

**Not present:** separate API project, MVC controllers folder, or Minimal API route definitions beyond what Razor Pages registers.

## Request flow: create a game

1. **GET** `/games/create` — route from `@page "/games/create"` in `Pages/Games/Create.cshtml`.
2. `CreateModel.OnGet()` (`Pages/Games/Create.cshtml.cs`) initializes `Input.ScheduledAtLocal` (default time).
3. User submits the form → **POST** to the same URL.
4. `CreateModel.OnPostAsync()` validates `Input`, converts local `ScheduledAtLocal` to UTC, builds a `Game`, calls `db.Games.Add` and `SaveChangesAsync`.
5. **Redirect** via `RedirectToPage("/Games/Details", new { id = game.Id })` → browser follows to the game details URL.

## Request flow: view a game

1. **GET** `/g/{id}` where `{id}` is a `Guid` — route from `@page "/g/{id:guid}"` in `Pages/Games/Details.cshtml`.
2. `DetailsModel.OnGetAsync(Guid id, …)` (`Pages/Games/Details.cshtml.cs`) loads the row with `db.Games.AsNoTracking().FirstOrDefaultAsync`.
3. If missing → `NotFound()`; otherwise `Page()` renders `Details.cshtml` (title, location, scheduled time, max players, shareable URL).

## Database entities

| Entity | File | Notes |
|--------|------|--------|
| **Game** | `Models/Game.cs` | `Id`, `Title`, `Location`, `ScheduledAt` (UTC), `MaxPlayers`, `CreatedAt`. |

**EF configuration:** `AppDbContext` — `DbSet<Game> Games`; key and string max lengths set in `OnModelCreating`.

**Storage:** SQLite; connection string in `appsettings.json`. Schema is updated with EF migrations under `Data/Migrations/` and applied on startup in `Program.cs` via `Database.MigrateAsync()`.

## Important conventions

- **Razor Pages:** Routes are declared with `@page` on the view. Handlers are `OnGet`, `OnPost`, `OnGetAsync`, `OnPostAsync`, etc., on classes inheriting `PageModel`.
- **Dependency injection:** Page models use primary-constructor injection (e.g. `CreateModel(AppDbContext db)`).
- **URLs:** Shareable game link is **`/g/{gameId}`**, not the `/Games/Details` file path (that path is for `RedirectToPage` / `Url.Page` only).
- **Time:** User input is treated as local time on create; persisted `Game.ScheduledAt` is UTC. Display uses `ToLocalTime()` on the details page.
- **Reads vs writes:** Details uses `AsNoTracking()` for a simple read; create uses a tracked add + save.
- **Stack:** No authentication or authorization policies configured; `UseAuthorization()` is in the pipeline for standard template shape.

## Where to add future features

Use `PROJECT_CONTEXT.md` and `TODO.md` for scope; below maps work to likely touch points.

| Feature (from roadmap) | Where to extend |
|------------------------|-----------------|
| **Participant** entity, Going / Waitlist | `Models/`; `AppDbContext` (`DbSet`, relationships, `OnModelCreating`); new migration in `Data/Migrations/`. |
| **Join game**, capacity rules, duplicate name checks | `Pages/Games/Details` (form + handler on `DetailsModel`) or a dedicated page under `Pages/Games/`; keep business logic close to the handler or extract a small service registered in `Program.cs`. |
| **Going / Waitlist lists, spots remaining** | `Pages/Games/Details.cshtml` (+ `DetailsModel` query with `Include` / projections). |
| **Leave game, waitlist promotion** | Same hub as join (`DetailsModel` or shared service); transactional `SaveChangesAsync` when reordering status. |
| **Cross-cutting** (validation, shared UI) | `Pages/Shared/`; optional `Services/` folder if logic outgrows page models. |

**Avoid** changing `Program.cs` wiring unless you add new framework features (e.g. controllers, minimal APIs, or extra `DbContext` types).
