# MVP TODO

## Day 1 — Game Creation

- [x] Create ASP.NET Core app
- [x] Add SQLite + EF Core
- [x] Create Game entity
- [x] Add create game page
- [x] Add game details page
- [x] Generate shareable `/g/{gameId}` link

## Day 2 — Join Flow

- [x] Add Participant entity
- [x] Add Join action
- [x] Assign Going or Waitlist based on capacity
- [x] Show Going list
- [x] Show Waitlist
- [x] Show spots remaining

## Day 3 — Leave + Waitlist Promotion

- [x] Add Leave action
- [x] Auto-promote oldest waitlisted player

## Day 4 — UX Optimization

- [x] Mobile-first layout
- [x] Show “Your status: Going / Waitlist”
- [x] Store player name in localStorage
- [x] Auto-fill name input from localStorage
- [x] Prevent duplicate joins at UI level
- [x] Improve spots display
- [x] Improve Going / Waitlist visual separation

## Day 5 — Identity Lite

- [x] Refine name handling
- [x] Add edit-name flow if needed
- [x] Handle duplicate names more gracefully
- [x] Improve empty/error states

## Day 6 — Organizer Convenience

- [ ] Add copy invite message button
- [ ] Generate shareable message with game title, time, location, spots, and link
- [ ] Optional: reminder helper text

## Day 7 — Polish + Deploy + Test

- [ ] Clean UI polish
- [ ] Basic validation/error handling
- [ ] Deploy app
- [ ] Create real test game
- [ ] Share with 3–5 real users
- [ ] Record feedback