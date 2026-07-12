# BookLab — Task 1 Video Walkthrough Script

**Workflow:** screen-record the **visual** for each segment (either the live app, or the animated deck), then generate the **voice** from the `SAY` text in Speechma, and assemble in CapCut. Target ~6–7 min.

- **Deck** (screen-record this): the animated architecture page — `video/architecture-deck.html` (published artifact link).
- **App** (screen-record this): the live game at `adeeb-booklab-07111926.web.app`.

Covers all six required points: ① overview · ② architecture · ③ asset loading & caching · ④ data saved/loaded · ⑤ key decisions & challenges · ⑥ AI tools.

---

### 1 · Intro  (~20s)
🎬 **SHOW:** Deck — cover slide.
🎙 **SAY:**
> "This is BookLab — a WebGL book-maker for kids, built in Unity and running entirely in the browser on Firebase. Children pick a scene, drag and scale stickers across pages, and save a storybook to the cloud that shows up on their shelf to read back. Let me walk you through how it works and how it's built."

### 2 · Overview + live demo  (~1:30)
🎬 **SHOW:** Switch to the **live app**. Do the full loop: shelf → **Create New** → pick a desert background → drag a camel and a palm on, scale one with `+` → **+ Page**, pick a sea background, add a boat → type a title → **Save** → back on the shelf, open the new book in **Playback** and flip the pages.
🎙 **SAY:**
> "The whole app is one simple loop across three screens. This is the shelf — every saved book is a cover card. I'll create a new one. In the editor I pick a background, then drag stickers straight onto the page and scale them. I can add more pages, each with its own scene. I give it a name and save — and it's instantly on my shelf as a new cover. Tapping a cover opens it in read-only playback, flipping through the pages exactly as they were placed. It targets full-HD and scales to any screen, and it all runs on Firebase's free plan."

### 3 · Architecture  (~1:30)
🎬 **SHOW:** Deck — **Architecture** slide (let the layers animate in; hit ↻ replay if needed).
🎙 **SAY:**
> "The architecture is layered, using MVC for structure and an event bus for flow. At the top, a bootstrap builds the canvas and hands off to AppRoot, which owns navigation. The three feature screens are each split into a View and a Controller — that's the MVC. Below them, a single ContentApi is the front door for all save and load. It sits on the Core layer, where FirebaseClient is the only code that touches the internet, alongside the event bus and the asset cache. And at the bottom are the models — the pure data that describes a book. The key idea: each layer only talks downward, and the screens never reference each other. They just announce events — 'open this book' — and AppRoot swaps the screen. That keeps everything decoupled and easy to extend."

### 4 · How data is saved & loaded  (~1:00)
🎬 **SHOW:** Deck — **Data** slide (the save/load beam animating; show the JSON recipe).
🎙 **SAY:**
> "Here's the important trick for saving. A book stores the recipe, not the pictures — just which asset, and where it sits, as normalized coordinates from zero to one, so it replays correctly on any screen size. Saving flows from the editor, to ContentApi, to FirebaseClient, which writes it as JSON to the Realtime Database under that child's own path. Loading is the same path in reverse. And notice — we talk to Firebase entirely over its REST API using UnityWebRequest, because the Firebase Unity SDK doesn't work in WebGL at all. That single decision shapes the whole data layer."

### 5 · Asset loading & caching  (~1:00)
🎬 **SHOW:** Deck — **Caching** slide.
🎙 **SAY:**
> "Assets — the backgrounds and stickers — live in the cloud and load on demand, only when they're first shown. Then they're cached in two tiers so they never download twice. First, memory — instant, but gone when the tab closes. Second, the browser's IndexedDB, which survives reloads. Only if both miss do we download once from Firebase Hosting and store it in both. Think of the cache as a fridge and the cloud as the warehouse — the fridge holds what you're using now, and if something gets evicted, we just restock it. I'm honest about the WebGL limits too: IndexedDB is per-browser, has a quota, and can be cleared; there are no background threads; and big textures hit a memory ceiling — which is exactly why downloading once matters so much."

### 6 · Key decisions & challenges  (~0:45)
🎬 **SHOW:** Deck — **Decisions** slide.
🎙 **SAY:**
> "A few decisions defined the project. Using REST instead of the SDK, so it runs in WebGL. Storing references instead of images, which keeps saves tiny and lets me swap the art with no code changes. And the hardest challenge — getting the compressed WebGL build to actually load on Firebase Hosting. Firebase quietly strips the encoding header from JavaScript files and caches inconsistently, so the game failed with a WebAssembly error. The fix was to ship the payload uncompressed and let Firebase compress it natively — reliable, and cache-safe."

### 7 · Use of AI tools  (~0:30)
🎬 **SHOW:** Deck — **AI tools** slide.
🎙 **SAY:**
> "I used Claude Code as a pair-programmer throughout — to break down the brief, shape the architecture, write the engine and UI, drive the Firebase setup from the command line, and debug that hosting issue. Every change was reviewed and understood, and the decisions were mine. It accelerated the work; it didn't replace the judgement."

### 8 · Wrap  (~0:15)
🎬 **SHOW:** Deck — **Wrap** slide (live link) → optionally cut to the live app one more time.
🎙 **SAY:**
> "It's live and playable at the link on screen — Unity WebGL, Firebase, REST, and two-tier caching, all on the free tier. Thanks for watching."

---

## Recording tips
- Record the deck **section by section** (scroll to each, let it animate, use ↻ replay to time it to your voice). Or scroll slowly through the whole thing.
- Record the app demo in a clean browser window (hide bookmarks bar); do a dry run first so the drags look smooth.
- In CapCut: lay the Speechma voice on the timeline first, then trim each visual clip to match. Add gentle zoom-ins on the diagrams as you talk about a box.
