---
name: reference-gamebull-rest-template
description: "User's existing clean REST-in-WebGL SDK (GameBull) to mirror for the Adeeb assessment build; also DragonMouse as the Firebase-SDK (Android) contrast"
metadata: 
  node_type: memory
  type: reference
  originSessionId: 48b57109-425f-47cd-a54a-482ff5cdedd8
---

The user already has a clean, working **REST-over-`UnityWebRequest` SDK in a WebGL Unity project** — the ideal template for the Adeeb assessment ([[project-public-git-sync-demo]]).

**Location:** `D:\COUPRA_GAMES\WebGL_Coupra_BasketBall URL\Assets\GameBull\` (outside the configured working dirs; accessible via Bash/Read). It talks to the GameBull game platform (base URL `https://api.g-b.store`), NOT Firebase — but the pattern is what matters.

**Layered pattern to mirror:**
- `GameBullBoot.cs` — reads config from page URL at startup (`Application.absoluteURL` + `[RuntimeInitializeOnLoadMethod]`).
- `GameBullEndpoints.cs` — single source of truth for URLs (base const + typed builders).
- `GameBullClient.cs` — the ONLY networking code: `GetJson`/`PostJson`, Bearer auth, awaitable via `TaskCompletionSource` (works in single-threaded WebGL), and a **WebGL cache-bust** (unique `_ts` query param + no-cache headers) — this IS the "caching limitations in WebGL" answer for Task 1.
- `GameBullApi.cs` — typed front-door methods + Newtonsoft.Json model classes.
- `GameBullLobbyController.cs` — UI + remote image load via `UnityWebRequestTexture.GetTexture` → `DownloadHandlerTexture` → `Sprite.Create`.
- `Plugins/GameBullClipboard.jslib` — proof the user knows the WebGL↔browser-JS `.jslib` bridge.
- Two markdown guides in the project (`GameBull_WebGL_Integration_Guide.md`, and `Unity WebGL Game — Complete Setup Guide.md` in `D:\COUPRA_GAMES\`) = handover-doc templates.

**Design upgrade for Task 1:** GameBull cache-busts to force FRESH data (good for scores). Task 1 assets are immutable, so do the opposite — "fresh for data, cached for assets": book list uses the cache-bust trick; images go through a new two-tier cache (in-memory `Dictionary<id,Sprite>` + persistent bytes in IndexedDB `persistentDataPath`).

**Contrast — DragonMouse** (`d:\DummyProjects\DragonMouse`): uses the full **Firebase Unity SDK v13.8.0** (Auth/Firestore/Functions/Analytics) with `google-services.json`, but it's an **Android** game — that's why the SDK works there. Same SDK will NOT work in WebGL; hence the REST approach. Cloud Functions there needs the paid Blaze plan — NOT needed for the assessment (free Spark plan covers both tasks; avoid Firebase Storage billing by hosting asset images free + storing covers as base64).
