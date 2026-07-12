# AI Tools Usage Log
### (for the "Any use of AI tools and how they supported your work" deliverable)

**Tool:** Claude Code (Claude Opus 4.8) in VS Code, used as a pair-programmer.

I built BookLab with Claude Code as a pair-programmer. It genuinely sped the work up — while I set the direction, made the decisions, and reviewed everything, so I understand every part and can extend it.

**How it supported the work:**

- **Planning & architecture** — we talked through the brief and shaped the structure together: a layered **MVC + event-driven** design, mirrored from a REST-in-WebGL pattern I trust.
- **Implementation** — it wrote much of the code alongside me: the data models, the REST `FirebaseClient`, the `ContentApi` save/load layer, the two-tier `AssetService` cache, and the screens — which I reviewed and adjusted as we went.
- **Debugging the hard parts** — a real help on the tricky WebGL + Firebase issues: the Firebase Unity SDK not supporting WebGL, and getting the compressed build to boot on Hosting.
- **Ops from the CLI** — Firebase project setup, seeding the database, and Hosting deploys, kept fast.

The build was quicker for it — and I set the direction, made the calls, and reviewed everything, so the work and the understanding are mine.
