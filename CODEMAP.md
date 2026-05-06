# Code map — Pickup Game Manager

Quick reference for navigating the repository. Behavior is described as implemented today (Days 1–3: create/view/join/leave; waitlist auto-promotion when a **Going** player leaves; no duplicate-name enforcement).

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
5. **Redirect** via `RedirectToPage("/Games/Details", new { id = game.Id })` → browser follows to **`/g/{guid}`**.

## Request flow: view a game

1. **GET** `/g/{id}` where `{id}` is a `Guid` — `@page "/g/{id:guid}"` in `Pages/Games/Details.cshtml`.
2. `DetailsModel.OnGetAsync(Guid id, …)` (`Pages/Games/Details.cshtml.cs`) loads `Game` with **`Include(g => g.Participants)`**, **`AsNoTracking()`**.
3. If missing → `NotFound()`; otherwise `PopulateLists(Game)` sets `GoingCount`, `SpotsRemaining`, `GoingPlayers`, `WaitlistedPlayers`, then `Page()` renders the view.
4. Optional **`TempData["LeaveNotice"]`** (set after leave attempts) renders as a short status message at the top.

## Request flow: join a game

1. User submits the **Join** form on `/g/{id}` → **POST** with `handler=Join` (`asp-page-handler="Join"` in `Details.cshtml`).
2. `DetailsModel.OnPostJoinAsync(Guid id, …)` loads **`MaxPlayers`** via **`AsNoTracking`**. **`Going`** count uses **`db.Participants.CountAsync`** (`Status == Going`).
3. **`Input.PlayerName`** trimmed; empty/whitespace → **`ModelState`** error and re-render.
4. **`db.Participants.Add(...)`** with **`GameId = id`**; **`SaveChangesAsync`**; redirect **`/g/{id}`**.

## Request flow: leave a game

1. User submits **Leave** from the game page → **POST** **`/games/{id}/leave`** (`asp-page="/Games/Leave"` `asp-route-id` in `Details.cshtml`; `@page "/games/{id:guid}/leave"` on `Leave.cshtml`).
2. **`LeaveModel.OnPostAsync`** (`Pages/Games/Leave.cshtml.cs`): trim **`PlayerName`**; empty → **`TempData["LeaveNotice"]`** + redirect **`/g/{id}`**.
3. Load all **`Participant`** rows for **`GameId`** into one tracked **`roster`** list.
4. Match by **`GameId` + name** with **`StringComparison.OrdinalIgnoreCase`**. No match → **`TempData`** message + redirect (no exception).
5. **If `Going`:** **`Remove`** that participant; among **`roster`** entries still **`Waitlist`**, pick **`OrderBy(CreatedAt)`, `ThenBy(Id)`** — first is promoted to **`Going`** (same **`SaveChangesAsync`**).
6. **If `Waitlist`:** **`Remove`** only.
7. **`TempData["LeaveNotice"]`** + redirect **`/g/{id}`** so lists refresh (PRG).

**GET** **`/games/{id}/leave`** → **`OnGet`** redirects to **`/g/{id}`** (no standalone UI).

### Promotion logic (Day 3)

When the leaving player was **Going**:

1. They are **deleted** from **`Participants`**.
2. The next **Going** slot is filled by the **waitlisted** player with the **smallest `CreatedAt`** (joined earliest); ties broken by **`Id`**.
3. That row’s **`Status`** changes **`Waitlist` → `Going`**.

When the leaving player was **Waitlist**, only **delete** — **no** promotion (capacity unchanged).

## Database entities

| Entity | File | Notes |
|--------|------|--------|
| **Game** | `Models/Game.cs` | `Id`, `Title`, `Location`, `ScheduledAt` (UTC), `MaxPlayers`, `CreatedAt`, **`Participants`**. |
| **Participant** | `Models/Participant.cs` | `Id`, `GameId`, `Name`, **`Status`**, **`CreatedAt`** (used for promotion order). |
| **ParticipantStatus** | `Models/ParticipantStatus.cs` | **`Going`**, **`Waitlist`** — stored as **`TEXT`**. |

**EF:** **`Participant`** → **`Game`** with cascade delete.

## Important conventions

- **Share URL:** **`/g/{gameId}`** for viewing/joining; **leave** uses **`/games/{id}/leave`** POST only from the details page form.
- **Join POST:** avoids tracked **`Game`** parent (insert **`Participant`** only).
- **Leave POST:** one tracked **`roster`** list so promotion updates an instance EF is already tracking.
- **Spots remaining:** **`Math.Max(0, MaxPlayers - GoingCount)`** on display.
- **Auth:** none.

## Where to add future features

| Idea | Where |
|------|--------|
| **Duplicate-name prevention**, unique **`(GameId, Name)`** | **`AppDbContext`** index + join/leave validation |
| **localStorage** display name | **`Details.cshtml`** scripts / **`site.js`** |
| **Polish / reminders / deploy** | per **`TODO.md`** / **`PROJECT_CONTEXT.md`** |

**Avoid** changing **`Program.cs`** unless adding new hosting features.
