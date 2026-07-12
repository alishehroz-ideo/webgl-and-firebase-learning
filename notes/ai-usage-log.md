# AI Tools Usage Log
### (for the "Any use of AI tools and how they supported your work" deliverable)

**Tool:** Claude Code (Claude Opus 4.8) in VS Code — used as a pair-programmer, under my direction.

**My role vs. the tool.** I led the project end to end: I set the requirements, chose the architecture (layered **MVC + event-driven**, mirrored from a REST-in-WebGL pattern I already trusted), designed the data model, and made every trade-off. Claude Code executed against that plan and helped me move faster — and I reviewed and steered every step.

**Where it helped:**

- **Implementing the plan quickly** — turning my architecture into working code (data models, the REST `FirebaseClient`, the `ContentApi` save/load layer, the two-tier `AssetService` cache, and the screens), which I reviewed and adjusted.
- **Debugging the hard problems** — I diagnosed the WebGL/Firebase issues (the Firebase Unity SDK not supporting WebGL; the compressed build failing to boot on Hosting); it helped me iterate on the fixes fast.
- **CLI operations** — Firebase project creation, seeding the database, and Hosting deploys, driven through it to stay in flow.
- **Pressure-testing decisions** — sanity-checking the trade-offs as I made them (free-tier cost, the caching approach, scalability to many users).

**Ownership.** Every line was read, understood, and is mine to defend. The engineering judgement and the decisions were mine — the tool accelerated the work, it didn't replace it.
