# BookLab — Handover Document

**Task 1: Interactive Content Creation (WebGL / Firebase)**

| | |
|---|---|
| **Author** | Ali Shehroz |
| **For** | Adeeb · Phase 1 Technical Assessment |
| **Live app** | https://adeeb-booklab-07111926.web.app |
| **Repository** | https://github.com/alishehroz-ideo/webgl-and-firebase-learning |
| **Stack** | Unity 6 WebGL · Firebase (Realtime Database + Hosting) · REST |

---

## 1. Overview

BookLab is a browser-based storybook maker for children. A child picks a scene, drags and scales stickers across one or more pages, names and saves the book to the cloud, and it appears as a cover-card on their shelf to read back.

It is a **Unity WebGL** application served in the browser, with all content and assets on **Firebase** — running entirely on the free (Spark) plan. The whole experience is a single **create → save → read** loop across three screens:

- **Main Menu** — the shelf of saved books, shown as cover-cards, plus a *Create New* button.
- **Editor** — pick a background, drop / drag / scale / delete stickers, add multiple pages, name it, save.
- **Playback** — open a saved book and flip through its pages, read-only.

It targets **1920×1080** and scales to any screen via a CanvasScaler reference resolution.

## 2. Architecture

The code follows a clear **MVC + event-driven** pattern, organised into layers where each layer has one responsibility and only depends downward.

| Layer | Responsibility |
|---|---|
| **App** | `Bootstrap` auto-starts the app and builds the Full-HD canvas + input; `AppRoot` owns the single canvas and swaps the active screen. |
| **Features · MVC** | `MainMenu`, `Editor`, `Playback` — each a Controller (logic) plus a code-built View. |
| **Services** | `ContentApi` — the typed "front door": `SaveBook` / `LoadBook` / `ListBooks` / `GetCatalog`. The app never touches URLs or JSON directly. |
| **Core** | `FirebaseClient` (the only code that touches the network) + `FirebaseEndpoints` (all URLs in one place); `EventBus` (publish/subscribe); `AssetService` (the two-tier image cache); `UiFactory` (code-built UI helpers). |
| **Models · "M"** | `BookModel → PageModel → PlacedObjectModel` and `AssetCatalog` — pure data, no Unity dependencies. |

**Event-driven navigation.** Screens never reference one another. They *publish* navigation events on the `EventBus` — `OpenBookRequest`, `CreateBookRequest`, `GoHomeRequest` — and `AppRoot` subscribes and swaps the screen. This keeps every feature decoupled and independently testable, and makes adding a screen a local change.

> **Why this matters.** MVC keeps data, view, and logic separate; the event bus keeps the screens from knowing about each other. Together they make the codebase easy to read, extend, and reason about — which is what "scalable systems" means here.

## 3. Asset loading & caching

Backgrounds and stickers are **not baked into the build**. A catalog in Firebase lists each asset's `id`, `name`, and `url`; the picker menus are built from it, and an image is downloaded **only when it is first shown** (opening a picker, placing or viewing an item).

**Two-tier cache (`AssetService`):**

1. **Memory** — a `Dictionary<url, Sprite>`, instant reuse for the session.
2. **Disk** — the downloaded bytes are written to `Application.persistentDataPath`, which in WebGL is the browser's **IndexedDB**, so they survive page reloads.

The lookup order is **memory → disk → download** (via `UnityWebRequest`), storing into both tiers on a miss. Concurrent requests for the same URL are **de-duplicated in flight** (a `Dictionary<url, Task<Sprite>>`), so an asset never downloads twice.

**WebGL caching limitations:**

- `persistentDataPath` is not a real filesystem — it is IndexedDB, flushed asynchronously, so a copy can be lost if the tab closes the instant after a download.
- IndexedDB is **per-browser and per-origin** (not shared across browsers or devices), has a **storage quota**, and can be **evicted** by the browser under pressure or cleared by the user.
- WebGL is **single-threaded** — there are no background threads, so all downloads run on the main thread. This is exactly why "download once, reuse" matters for smoothness.
- A limited memory heap means too many large textures can exhaust memory; a production build would add an LRU eviction policy on the memory tier.

> **Mental model.** The cache is a *fridge* (fast, limited, per-browser, evicts old items); the cloud + CDN is the *warehouse* (has everything, always restockable). A cache miss simply re-downloads — no data is lost.

## 4. How data is saved & loaded

**The "recipe" model.** A saved book stores **references and numbers, never image bytes**. Each placed object records which asset it is and where it sits, as **normalized 0–1 coordinates** — so a book created at one resolution replays correctly at any other.

```json
// one placed sticker
{ "assetId": "obj_camel", "x": 0.62, "y": 0.55, "scale": 0.8, "rotation": 0, "z": 1 }

// a book = title + timestamps + pages (each page = background + placed objects)
BookModel -> PageModel[] -> PlacedObjectModel[]
```

**Firebase over REST (no SDK).** The Firebase Unity SDK **does not support WebGL**, so every data operation uses the **Realtime Database REST API** through `UnityWebRequest`: `GET` on `/path.json` to read, `PUT` to write. Async is achieved by wrapping the callback-based request in a `TaskCompletionSource`, giving clean `async/await` with no coroutines and no blocking the frame.

| Action | Request |
|---|---|
| Read the picker catalog | `GET /assetCatalog.json` |
| List a child's books (shelf) | `GET /books/{kidId}.json` |
| Load one book | `GET /books/{kidId}/{bookId}.json` |
| **Save** a book | `PUT /books/{kidId}/{bookId}.json` |

Books are stored under a per-child path (`/books/{kidId}/…`) — a single `demo` id in this build, but the structure is deliberately ready to scale (see §6). JSON is parsed with **Newtonsoft**, because the Realtime Database returns objects keyed by id, which Unity's built-in `JsonUtility` cannot handle. On save, the shelf card uses the first page's background as its cover (a true page snapshot is reserved; keeping the cover inline avoids paid Firebase Storage).

## 5. Key decisions & challenges

**REST, not the SDK.** The Firebase Unity SDK is unavailable in WebGL, so the entire data layer is REST over `UnityWebRequest` — the WebGL-safe path, and the decision that shapes everything else.

**References, not images.** Books store asset ids + normalized transforms; art is hosted once and cached. A direct benefit: **art can be added or swapped with zero code changes**. The desert & sea theme was dropped in by editing the catalog and hosting the files — no rebuild.

**Resolution-independent placement.** A dragged sticker's pixel position is meaningless across screens. The editor maps the pointer → stage-local space (`RectTransformUtility.ScreenPointToLocalPointInRectangle`) → **normalized 0–1**, and stores that. A book made on a phone replays pixel-perfect on a desktop.

**Async in a single-threaded runtime.** Unity's web requests are callback-based; wrapping each in a `TaskCompletionSource` gives clean `async/await` in WebGL's single thread, without coroutines or frame stalls.

**Hosting the compressed build (the hardest bug).** Unity's gzip build (`.wasm.gz` / `.data.gz` / `.framework.js.gz`) fought Firebase Hosting: Firebase **strips `Content-Encoding` from any `.js` filename** and caches encodings inconsistently across its CDN, so the WebAssembly failed to load with an *"expected magic word"* error. The fix: **ship the payload uncompressed and let Firebase gzip it natively** over the wire — reliable and cache-consistent. This is automated in `firebase/deploy-hosting.sh`.

**Free-tier by design.** Realtime Database + Hosting on the free Spark plan; covers stored inline (no Storage), no Cloud Functions, no credit card.

## 6. Performance & scalability

**Performance awareness (WebGL):**

- **On-demand loading** — nothing downloads until it is actually shown.
- **Two-tier caching** — every asset downloads at most once per browser; in-flight de-dup prevents duplicate fetches.
- **Responsive by construction** — a CanvasScaler with a 1920×1080 reference scales cleanly to any screen; positions are normalized.
- **Single-thread-friendly I/O** — non-blocking `async/await` over web requests keeps the frame responsive.

**Scaling to many children & large libraries.** Data splits into two kinds that scale very differently: **shared** (the catalog + images — one copy for everyone, CDN-served, does not grow with users) and **per-child** (each child loads only their own `/books/{kidId}` slice, never everyone's).

- **8,000 children:** the shared art is one cached/CDN copy for all; each child pulls only their small slice; recipes are tiny text.
- **50,000 images:** the cloud + CDN is the source of truth; IndexedDB holds only the recently-used subset per browser (evicted items simply re-download). A production build adds an LRU cap so it never exceeds the quota.
- **Growth path:** fetch only the child's node; paginate the shelf; add a derived **search index / search service** for discovery; rely on CDN + browser cache for images.

## 7. Use of AI tools

I built BookLab with **Claude Code** (Claude Opus 4.8, in VS Code) as a pair-programmer. It genuinely sped the work up — while I set the direction, made the decisions, and reviewed everything, so I understand every part and can extend it.

- **Planning & architecture** — we talked through the brief and shaped the structure together: a layered MVC + event-driven design, mirrored from a REST-in-WebGL pattern I trust.
- **Implementation** — it wrote much of the code alongside me (the data models, the REST client, save/load, the two-tier cache, the screens), which I reviewed and adjusted as we went.
- **Debugging the hard parts** — a real help on the tricky WebGL + Firebase issues: the SDK not supporting WebGL, and getting the compressed build to boot on Hosting.
- **Ops from the CLI** — Firebase project setup, seeding the database, and Hosting deploys, kept fast.

The build was quicker for it — and the direction, the decisions, and the understanding are mine.

## 8. Tech stack, running & deploying

| Item | Detail |
|---|---|
| Engine | Unity **6000.4.3f1** (URP), Input System, Newtonsoft JSON |
| Backend | Firebase **Realtime Database** + **Hosting** — project `adeeb-booklab-07111926` (Spark / free) |
| Run in editor | Open the project and press **Play** (the app auto-starts via `Bootstrap`) |
| Build | Unity menu **BookLab → Build WebGL** (outputs `Build/WebGL`) |
| Deploy | `bash firebase/deploy-hosting.sh` — assembles `public/`, applies the gzip workaround, deploys |
| Live | https://adeeb-booklab-07111926.web.app |
| Repository | https://github.com/alishehroz-ideo/webgl-and-firebase-learning |

**Deliverables:** Git repository · live WebGL build · this handover document · video walkthrough.

*Art credit: sticker items are OpenMoji (CC BY-SA 4.0); backgrounds are generated flat illustrations.*
