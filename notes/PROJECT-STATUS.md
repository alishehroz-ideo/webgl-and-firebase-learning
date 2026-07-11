# Project Status — Task 1 (BookLab)

Snapshot for continuing on another machine. **Clone to the SAME path**
(`d:\DummyProjects\webgl and firebase learning`) so the Claude chat restore + `/resume` work.

## Links
- **Playable (live):** https://adeeb-booklab-07111926.web.app
- **Firebase console:** https://console.firebase.google.com/project/adeeb-booklab-07111926
- **GitHub (public):** https://github.com/alishehroz-ideo/webgl-and-firebase-learning

## Done — Task 1 feature-complete + deployed
- Main Menu shelf · Editor (background + drop/drag/**scale**/**delete** stickers, **multiple pages**, **editable title**) · Playback (page flip).
- Architecture: MVC + event-driven (EventBus/AppRoot), layered like the GameBull SDK.
- Firebase RTDB save/load over **REST** (no SDK — WebGL-safe), two-tier image cache (memory + IndexedDB), images on Firebase Hosting.

## Continue from home
1. Clone to `d:\DummyProjects\webgl and firebase learning`.
2. `.\sync.ps1 pull` → restores the Claude chat; open Claude Code here → `/resume`.
3. Open the Unity project (Unity **6000.4.3f1**); let it restore packages (Newtonsoft).
4. Per-machine auth (one-time each): `gh auth login` (to push) and `firebase login` (to deploy) — same `emberbound1` account for Firebase.

## Remaining
- [ ] Rebuild + redeploy the latest editor changes (incl. the title box): Unity `BookLab > Build WebGL`, then `bash firebase/deploy-hosting.sh`.
- [ ] Confirm the title text box accepts typing in the WebGL build (legacy uGUI InputField + new Input System — swap to TMP if not).
- [ ] Record the 5–10 min video walkthrough.
- [ ] Finish the handover doc (`notes/task1-handover-draft.md` is ~70% there).
- [ ] Task 2 (Search & Content Discovery) — reuses this Core (FirebaseClient / EventBus / UI / pooling).

## Cheatsheet
- Build:  Unity → `BookLab > Build WebGL`
- Deploy: `bash firebase/deploy-hosting.sh`  (handles the gzip workaround)
- Reset demo data: `MSYS_NO_PATHCONV=1 firebase database:set /assetCatalog firebase/seed/assetCatalog.json -f`
- Push code + chat: `.\sync.ps1 push`   ·   Pull at other machine: `.\sync.ps1 pull`
