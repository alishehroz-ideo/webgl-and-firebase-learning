# AI Tools Usage Log
### (for the "Any use of AI tools and how they supported your work" deliverable)

**Tool:** Claude Code (Claude Opus 4.8) running inside VS Code.

**How it supported the work:**

- **Requirements breakdown** — read both task PDFs and explained them line-by-line in plain language, so the goals were fully understood before coding.
- **Architecture design** — proposed the layered **MVC + event-driven** structure, deliberately mirrored from an existing, proven REST-in-WebGL project of mine so the pattern was battle-tested.
- **Caught the key WebGL pitfall** — flagged that the Firebase Unity SDK doesn't support WebGL, and steered to the **REST + `UnityWebRequest`** approach instead.
- **Wrote the engine** — data models, `FirebaseClient` / `FirebaseEndpoints`, `ContentApi` (save/load), and the two-tier `AssetService` cache, all with explanatory comments.
- **Explained concepts** — REST, caching, IndexedDB, MVC, event-driven — in plain terms so every decision was mine to understand, not a black box.
- **Cost analysis** — confirmed the whole thing fits Firebase's free plan and avoided paid features (Cloud Functions / Storage billing).
- **Project setup** — initialized the git repo and an office↔home sync workflow.
- **(Ongoing)** — will assist with the UI scaffolding, this handover document, and the scalability writeup.

**Note on responsibility:** all AI-generated code was reviewed and understood before use, and the key decisions (e.g., keeping the per-kid data structure, repo visibility, scope) were made by me. AI accelerated the work; it didn't replace the judgement.
