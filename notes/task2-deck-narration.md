# BookLab — Task 2 Deck Voice-over (per slide)

The Task 2 deck (`video/task2-deck-standalone.html`), recorded slide by slide with the
arrow-key spotlight stepping. Convert each **SAY** block to voice (Speechma / ElevenLabs)
and lay it under the matching slide recording.

Covers the six required points (overview · architecture · asset-loading & caching ·
data saved/loaded · key decisions · AI tools) plus the required scalability answer, and
shines on the four graded criteria (parsing robustness · search performance · code
architecture · scalability).

---

## Slide 1 — Cover / title
🎙 **SAY:**
> "Hi — this is my submission for Task 2 of the assessment: Search and Content Discovery. It's a search feature over a real, messy content library in Firebase — the app pulls clean results out of inconsistent data, searches them instantly, and I've designed it to scale. Built in Unity WebGL, MVC and event-driven. Let me walk you through it."

## Slide 2 — Overview  (search screen + the 4 points; hold)
🎙 **SAY:**
> "Here's the whole feature at a glance. You type a content name or an author into the box, hit Search, and the matches come back as cards — each showing the name, author, and date. On the right is the real screen: I searched one author and got three results. The important part is what sits behind it — the data is remote, living in Firebase, and it's genuinely messy. So the app does all the tidying: it turns inconsistent, real-world strings into these clean cards, and never crashes on a bad entry."

## Slide 3 — The challenge (messy data)  (Firebase shot + 4 characteristics; hold)
🎙 **SAY:**
> "Here's the real data. Everything — name, author, and date — is crammed into one CoverInfo field, and the rules say I can't clean it at the source. It's messy every way you can imagine — mixed separators, reordered, missing fields, junk — so it all comes down to parsing this robustly."

## Slide 4 — Parsing robustness  (code card + 4 real examples — 5 spotlight steps)
🎙 **SAY:**
> "This is the heart of the task — and my key decision was to parse by shape, not position. The naive way is to split the string and just grab the second piece as the author — but the moment a field is missing or reordered, that grabs the wrong thing, or crashes.
>
> **[↓ code card]** So instead: I split the string, then find the date by its shape — a day-month-year pattern — wherever it happens to sit. The author is the field right before the date; the name is everything before that, re-joined so an underscore inside a name survives. And if there's no date at all, it falls back safely and flags low confidence. Nothing is hardcoded to any entry.
>
> **[↓ example 1]** Here it is on real data. This one's clean — name, author, and date come out, and all the junk after the date is simply dropped.
>
> **[↓ example 2]** This one has no author — just a double underscore where it should be. Position parsing would misalign; the shape approach gives a clean 'no author' and moves on.
>
> **[↓ example 3]** Here the trailing tag even contains an underscore of its own — but since everything after the date is ignored, it doesn't matter.
>
> **[↓ example 4]** And this is a genuine oddity — the author is literally the words 'No Name.' We keep it exactly as-is. The parser never assumes, never hardcodes, and never crashes."

## Slide 5 — Data loading & error states  (SearchService.LoadAsync card + 3 states — 4 spotlight steps)
🎙 **SAY:**
> "The brief asks how assets load and cache, and how data is saved and loaded. This feature is read-only — it saves nothing — so here's the load-and-cache side, which is where the real work is.
>
> **[↓ code card]** It's a single request. I reuse the same Firebase client from Task 1 to pull the whole library in one call, then parse every item once and hold the clean list in memory — that's the cache, and it's why searching afterwards is instant, with no re-fetching. And loading is handled defensively: if Firebase is empty or unreachable it returns a clean failure, and the whole parse is wrapped in a try-catch — so one malformed entry logs an error and returns safely, never a crash.
>
> **[↓ Loading]** That gives three clear states the user actually sees. First — Loading — 'Loading…' while the library is fetched.
>
> **[↓ Error]** Second — Error — if it fails, a plain message: 'Couldn't load the library — try again.' No blank screen.
>
> **[↓ Ready]** And third — Ready — the results appear as cards, and every search from then on is instant."

## Slide 6 — Architecture (MVC + event-driven)  (Features/Search shot + legend + SearchEvents card — 5 spotlight steps)
🎙 **SAY:**
> "The architecture is MVC plus event-driven — and it reuses the Core I built for Task 1, so the Firebase client, the event bus, and the UI helpers were already there. On the left is the real Search folder; on the right, what each piece does.
>
> **[↓ Model]** The Model is the SearchService and the parser — it loads from Firebase, parses, and holds and searches the clean list. All the logic, no UI.
>
> **[↓ View]** The View is SearchView — the box, the button, and the result cards. It just renders whatever arrives; it knows nothing about how search works.
>
> **[↓ Controller]** The Controller, SearchController, sits between them — it turns a search into results by asking the service.
>
> **[↓ Events]** And here's the key: the View and Controller never call each other directly. They talk through two events.
>
> **[↓ code card]** The View publishes SearchRequested when you hit Search; the Controller publishes SearchResults when they're ready — carrying the results, plus the loading and error flags. That's the whole wiring — it keeps every piece decoupled and easy to change."

## Slide 7 — Search performance  (Search() code card — 1 spotlight step)
🎙 **SAY:**
> "Search performance came down to one decision: do the expensive work once. All that messy parsing happens a single time, when the library first loads. After that, every search is just a quick in-memory scan of the already-clean list — no network trip, and no re-parsing.
>
> **[↓ code card]** The search itself is simple and fast — it lowercases the query once, then checks each item's name and author for a match, either one. For the thousand-plus items in this task, that's instant and completely smooth, even as you type. The heavy lifting is already done, so searching stays cheap."

## Slide 8 — Scaling to 10,000+  (before/after + 3 steps — 5 spotlight steps)
🎙 **SAY:**
> "This is the required question — what happens at ten thousand items and beyond. The honest answer is that the demo's approach doesn't scale as-is, and here's how I'd redesign it.
>
> **[↓ ✗ simple]** The problem: right now every visitor's browser downloads the whole library and parses all of it, on a single WebGL thread, every visit. At a thousand items that's fine. At ten thousand or more, that's a big download and a noticeable freeze — repeated for every user.
>
> **[↓ ✓ redesign]** The redesign flips it: do the heavy work once, on the server, so each browser only ever downloads the handful of results it actually needs.
>
> **[↓ step 1]** Concretely, three moves. First — precompute a clean index. A Firebase Cloud Function runs the same parser once, and re-runs only when an item changes, writing clean records into a separate searchIndex node. The original messy data stays untouched, and the browser now reads clean data with zero parsing on load.
>
> **[↓ step 2]** Second — search on the server, not the browser. With an indexed field, the browser asks Firebase for just the matches — 'authors starting with this, give me twenty' — and Firebase jumps straight to them using the index, instead of scanning everything.
>
> **[↓ step 3]** And third — if we needed match-in-the-middle or typo-tolerance, a dedicated search service like Algolia or Typesense handles that at any scale. Same principle throughout: the browser only ever gets the few matches."

## Slide 9 — Key decisions & challenges  (4 cards — one liner each)
🎙 **SAY:**
> "Four decisions worth calling out.
>
> **[↓ card 1]** I parse into the app's memory, never touching the messy source — exactly as the rules require.
>
> **[↓ card 2]** I parse by shape, not position, so reordered or missing fields never break it.
>
> **[↓ card 3]** The data's Arabic, so I bundled the Amiri font with TextMeshPro to render it properly in WebGL.
>
> **[↓ card 4]** And loading, errors, and bad data are all real states — always a clear message, never a crash."

## Slide 10 — AI tools  (4 points — one liner each)
🎙 **SAY:**
> "On AI — I used Claude Code as a pair-programmer.
>
> **[↓ 1]** We designed the parser together, talking through the messy-data edge cases.
>
> **[↓ 2]** It wrote much of the Search feature alongside me, which I reviewed and adjusted.
>
> **[↓ 3]** It was a real help on the tricky Arabic-in-WebGL bug.
>
> **[↓ 4]** And it shaped where AI belongs at scale — offline, to build the clean index, not in the live search.
>
> It sped things up — but the direction and the decisions are mine."

## Slide 11 — Wrap  (recap + live link; hold)
🎙 **SAY:**
> "And that's Task 2 — a search feature that takes real, messy data, parses it robustly by shape, searches it instantly in memory, handles every state cleanly, and has a clear path to ten thousand items and beyond. All MVC and event-driven, reusing the Core from Task 1. It's live and playable. Thanks very much for watching."
