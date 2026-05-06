# Code map — Pickup Game Manager

Quick reference for navigating the repository. Behavior is described as implemented today (Day 1–2: games + join / Going / Waitlist; no leave or duplicate-name enforcement yet).

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
| `src/PickupGameManager.Web/Models/` | Domain types: `Game.cs`, `Participant.cs`, `ParticipantStatus.cs`. |
| `src/PickupGameManager.Web/Data/AppDbContext.cs` | EF Core `DbContext`; `OnModelCreating` rules and relationships. |
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
5. **Redirect** via `RedirectToPage("/Games/Details", new { id = game.Id })` → browser follows to the game details URL (`/g/{guid}`).

## Request flow: view a game

1. **GET** `/g/{id}` where `{id}` is a `Guid` — `@page "/g/{id:guid}"` in `Pages/Games/Details.cshtml`.
2. `DetailsModel.OnGetAsync(Guid id, …)` (`Pages/Games/Details.cshtml.cs`) loads `Game` with **`Include(g => g.Participants)`**, **`AsNoTracking()`**.
3. If missing → `NotFound()`; otherwise `PopulateLists(Game)` sets `GoingCount`, `SpotsRemaining`, `GoingPlayers`, `WaitlistedPlayers`, then `Page()` renders the view (game info, join form, lists, share URL).

## Request flow: join a game

1. User submits the **Join** form on `/g/{id}` → **POST** with `handler=Join` (see `asp-page-handler="Join"` in `Details.cshtml`).
2. `DetailsModel.OnPostJoinAsync(Guid id, …)` loads **`MaxPlayers`** via **`AsNoTracking`** (projected query). **`Going`** count uses **`db.Participants.CountAsync`** for that **`GameId`**.
3. `Input.PlayerName` is trimmed; whitespace-only names get a **`ModelState`** error.
4. If validation fails → **`LoadDisplayAsync`** reloads the game + participants (**`AsNoTracking`**) and renders the page.
5. If **`goingCount < MaxPlayers`**, new participant is **`Going`**; else **`Waitlist`**.
6. **`db.Participants.Add(...)`** with **`GameId = id`** only (no tracked **`Game`** parent); **`SaveChangesAsync`**, then **PRG** **`RedirectToPage("/Games/Details", new { id })`**.  
   *(Avoids tracking **`Game`** + collection mutation, which could trigger a stray **`Games`** **UPDATE** and **`DbUpdateConcurrencyException`** on SQLite.)*

## Database entities

| Entity | File | Notes |
|--------|------|--------|
| **Game** | `Models/Game.cs` | `Id`, `Title`, `Location`, `ScheduledAt` (UTC), `MaxPlayers`, `CreatedAt`, **`Participants`** collection. |
| **Participant** | `Models/Participant.cs` | `Id`, `GameId`, `Name`, **`Status`** (`ParticipantStatus`), `CreatedAt`, navigation **`Game`**. |
| **ParticipantStatus** | `Models/ParticipantStatus.cs` | **`Going`**, **`Waitlist`** — stored in SQLite as **`TEXT`** via **`HasConversion<string>()`**. |

**EF configuration:** `AppDbContext.OnModelCreating` — **`Game`** key/strings; **`Participant`** FK **`GameId`**, **`WithMany(g => g.Participants)`**, **`OnDelete(DeleteBehavior.Cascade)`**.

**Storage:** SQLite; migrations under `Data/Migrations/`; **`Database.MigrateAsync()`** in **`Program.cs`** at startup.

## Important conventions

- **Razor Pages:** `@page` defines routes; handlers **`OnGetAsync`**, **`OnPostJoinAsync`**, etc.
- **Join handler name:** form **`asp-page-handler="Join"`** maps to **`OnPostJoinAsync`**.
- **URLs:** shareable game URL remains **`/g/{gameId}`**.
- **Reads:** game details **GET** uses **`AsNoTracking()`**.
- **Writes:** **join POST** inserts **`Participant`** rows via **`DbSet<Participant>`**; **`Game`** is not tracked on that path.
- **Ordering:** Going and waitlist display **`OrderBy(p => p.CreatedAt)`** (helps future waitlist promotion).
- **Spots remaining:** **`Math.Max(0, MaxPlayers - goingCount)`** (not capped by waitlist size).
- **Stack:** no authentication configured.

## Where to add future features

| Feature (from roadmap) | Where to extend |
|------------------------|-----------------|
| **Leave game**, **promote waitlist**, **duplicate join prevention** | `DetailsModel` (new **`OnPostLeaveAsync`** or similar) and/or small service; transactional **`SaveChangesAsync`**; optional unique index **`(GameId, Name)`** in **`AppDbContext`**. |
| **localStorage name** (UX) | `Details.cshtml` **`Scripts`** section or **`site.js`**. |
| **Cross-cutting** | `Pages/Shared/`; optional `Services/` if page models grow. |

**Avoid** changing `Program.cs` unless you add APIs, another `DbContext`, or new middleware.
