# PickupGameManager - Project Context

## Goal

Build a minimal web app for organizing pickup sports games.

The app replaces manual RSVP tracking in WhatsApp, SMS, Discord, or spreadsheets.

Core flow:

Organizer creates a game → app generates shareable link → players join → system assigns Going or Waitlist → if someone leaves, waitlist auto-promotes.

## MVP Constraints

Must include:

- Create game
- Shareable game page
- Join game
- Leave game
- Going list
- Waitlist
- Max player cap
- Auto-promote oldest waitlisted participant when a Going player leaves

Must NOT include:

- Login/auth
- Payments
- Mobile app
- Complex UI
- Multi-team scheduling
- Recurring games

## Tech Stack

- ASP.NET Core
- Minimal APIs or Razor Pages
- SQLite
- Entity Framework Core
- Simple server-rendered UI preferred
- Mobile-first layout

## Data Model

### Game

- Id: Guid
- Title: string
- Location: string
- DateTime: DateTime
- MaxPlayers: int
- CreatedAt: DateTime

### Participant

- Id: Guid
- GameId: Guid
- Name: string
- Status: Going | Waitlist
- CreatedAt: DateTime

## Business Rules

When joining:

- If current Going count is less than MaxPlayers, new participant becomes Going.
- Otherwise, new participant becomes Waitlist.
- Prevent duplicate joins by same name for same game.

When leaving:

- Remove participant from game.
- If participant was Going and waitlist has people, promote oldest Waitlist participant to Going.

## UX Goals

- Joining should take less than 3 seconds.
- Mobile-first.
- Clear spots remaining.
- Show Going and Waitlist lists.
- No accounts required.
- Store player name in browser localStorage if needed.

## Development Philosophy

Build the simplest working version first.
Avoid over-engineering.
Prefer readable code over abstraction.
Do not add features outside MVP unless explicitly requested.