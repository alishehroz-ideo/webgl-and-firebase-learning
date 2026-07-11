# Task 1 — Interactive Content Creation (WebGL / Firebase)
### Handover Document — DRAFT (living notes, updated as we build)

> Sections below map to Adeeb's required deliverable checklist, so this doc grows
> into the final handover writeup + video script.
>
> **Build status:** Backend done + verified against **LIVE Firebase**. UI working
> in Unity: **Main Menu** shelf ✅, **Playback** (page flip, renders background +
> stickers) ✅, **Editor** (pick background, drop + **drag** stickers, save) ✅ —
> all via event-driven navigation (EventBus/AppRoot). Firebase project
> `adeeb-booklab-07111926` (Spark/free). Remaining: sticker **scale/delete** +
> **multiple pages** in the editor, WebGL build, live deploy (Firebase Hosting),
> handover doc + video.

---

## 1. Overview

A browser-based **"book maker" for kids**:

- **Main Menu** — a shelf of saved books shown as cover-cards, plus a "Create New" button.
- **Create/Editor** — pick a background, add stickers/furniture, drag + scale them, add multiple pages, save.
- **Save** — auto-generates a cover snapshot; the book appears on the shelf.
- **Playback** — tap a cover to flip through the pages read-only.

Runs as a **Unity WebGL** build, designed for **1920×1080**, scaling to any screen.
Data and assets come from **Firebase**, accessed over **REST** (see §3).

## 2. Architecture (MVC + Event-Driven)

Clean, layered structure (mirrors a proven REST-in-WebGL pattern we already had working):

- **Core/Net** — `FirebaseClient` (the ONLY code that touches the internet: `GetJson` / `PutJson` / `GetBytes`) + `FirebaseEndpoints` (every URL in one place).
- **Core/Assets** — `AssetService` (two-tier image cache).
- **Models** — `BookModel → PageModel → PlacedObjectModel`, plus `AssetCatalog` / `AssetDefinition`. Pure data (the "M").
- **Services** — `ContentApi` (typed front door: `Save` / `Load` / `List` / `Catalog`).
- **Features** — `MainMenu` / `Editor` / `Playback`, each split into **View** (shows) + **Controller** (decides).
- **Events** — an `EventBus` so parts announce instead of calling each other directly.

**Why MVC:** separates data / screen / logic, so one can change without breaking the others.
**Why event-driven:** parts announce events ("BookSaved") instead of wiring into each other → decoupled and easy to extend.

## 3. Firebase in WebGL — the key decision

- The **Firebase Unity SDK does NOT support WebGL** (only Android / iOS / desktop). Using it would silently break the browser build.
- **Solution:** talk to Firebase over its **REST API** using `UnityWebRequest` — works natively in WebGL, no plugins.
  - Realtime Database: `GET` / `PUT` on `https://<db>.firebaseio.com/<path>.json`
  - Images: hosted on a free host / CDN, downloaded via `UnityWebRequest`.
- Networking is wrapped as `async/await` via `TaskCompletionSource` (works fine in single-threaded WebGL).

## 4. Data model — the "recipe"

A saved book stores **references + numbers, never images**:

```json
{
  "id": "book_ab12cd",
  "title": "My Room",
  "createdAt": "2026-07-12T10:30:00Z",
  "updatedAt": "2026-07-12T10:45:00Z",
  "coverBase64": "data:image/jpeg;base64,...",
  "pages": [
    { "backgroundId": "bg_livingroom",
      "objects": [
        { "assetId": "furn_sofa", "x": 0.30, "y": 0.55, "scale": 1.2, "rotation": 0, "z": 0 }
      ] }
  ]
}
```

Design choices baked in:
- **Positions normalized 0..1**, not pixels → replays correctly on any screen size.
- **`assetId` references**, not images → recipe stays tiny; images cached separately.
- **`coverBase64` stored inline** → avoids paid Firebase Storage.

Sits in Realtime DB as a tree: `/assetCatalog` (the picker menu) and `/books/{kidId}/{bookId}` (per-kid).

## 5. Asset loading & caching (marked CRITICAL)

- **On-demand:** an image downloads only when first shown — not all upfront.
- **Two-tier cache** (`AssetService`):
  - **Memory** (Dictionary) — instant reuse this session; lost on close.
  - **Disk** (IndexedDB via `persistentDataPath`) — survives reloads; per-browser.
- **Flow:** check memory → check disk → download, then save into both.
- **In-flight de-dupe:** the same url requested twice at once shares ONE download.

## 6. WebGL / IndexedDB limitations (required explanation)

- `persistentDataPath` isn't a real disk in a browser — it's **IndexedDB**, flushed lazily → a copy may be lost if the tab closes the instant after a download.
- **Per-browser + per-site** — not shared across browsers/devices → a new browser re-downloads.
- **Quota limits;** the browser can **evict** under low-disk pressure; the user can clear it; **incognito** wipes on close; some browsers clear storage for sites unvisited for a while.
- **Memory ceiling** in WebGL — too many big textures can run the tab out of memory → needs eviction at scale.
- **No background threads** — all downloads on one thread → caching is what keeps it smooth.

## 7. Scalability thinking (feeds Task 2's required section too)

- **Data splits two ways:** *shared* (catalog + images — one copy for everyone, CDN-served, doesn't grow with users) vs *per-kid* (each kid loads only their own books via `/books/{kidId}`).
- **8000 kids:** shared art is one cached copy for all; each kid pulls only their slice; recipes are tiny text.
- **50,000 images:** IndexedDB is a **cache (fridge)**, not the **warehouse (cloud/CDN)**. Only the recently-used subset is cached; evicted items simply re-download. Add an **LRU cap** so we never blow the quota.
- **Redesign at scale:** fetch only `/books/{kidId}`; **paginate**; add an **index / search service** for discovery; rely on **CDN + browser cache** for images.
- Analogy: IndexedDB = your **fridge** (what you're using now, limited, old stuff tossed); cloud/CDN = the **grocery store** (has everything, always restockable). Never fit the store in the fridge.

## 8. Key decisions & challenges

- **REST over the SDK** (the WebGL blocker).
- **Fresh-for-data, cached-for-assets** — cache-bust the book list (always current); cache the immutable images.
- **`coverBase64`** to stay off paid Storage.
- **Per-kid data with a single `demo` id** — no login to build, but structured to scale.
- **Free Spark plan; no Cloud Functions** (see §10).

## 9. Use of AI tools

See [ai-usage-log.md](ai-usage-log.md).

## 10. Cost

Entire app runs on Firebase's **free (Spark) plan** — Realtime DB free tier, images on a free host/Hosting, covers as base64, the WebGL build on Firebase Hosting. **No credit card, no Cloud Functions.**
