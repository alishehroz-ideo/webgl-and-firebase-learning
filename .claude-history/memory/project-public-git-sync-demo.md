---
name: project-public-git-sync-demo
description: "Unity demo for the Adeeb Phase 1 Technical Assessment (deadline 16 Jul 2026), git-synced office<->home with Claude chat included, in a PUBLIC repo by user's explicit choice"
metadata: 
  node_type: memory
  type: project
  originSessionId: 48b57109-425f-47cd-a54a-482ff5cdedd8
---

`d:\DummyProjects\webgl and firebase learning` is a Unity 6 (6000.4.3f1, URP) WebGL + Firebase project the user is preparing as a **demo submission for a job's technical assessment**. It is git-synced between office and home.

**The assessment:** Phase 1 Technical Assessment for a **Unity Developer** role at **Adeeb** (adeebkids.com), from CTO **Muzna Alzadjali** (muzna@adeebkids.com). **Deadline: 16 July 2026** (the email typo'd "16 June"; user confirmed 16 July). Two attached task docs (.pptx): "Task 2 — Search & Content Discovery" and "Unity Developer Technical Test". As of 2026-07-11 those attachments were NOT yet downloaded to disk, so their contents are unread. Adeeb invited portfolio/GitHub/live links with the submission (the public repo serves that).

Key setup facts:
- The Claude Code chat transcripts + memory are intentionally committed into the repo under `.claude-history/` so the conversation travels with the code. `.claude/sync-history.ps1` mirrors `~/.claude/projects/<derived>/` <-> `.claude-history/`; `sync.ps1 push|pull` is the manual office<->home workflow. Sync is manual (no hooks).
- For chat `restore` to work, the project must live at the **same absolute path** on both machines.
- The repo is **PUBLIC** — the user confirmed this twice after I flagged that a company demo + full chat log would be world-readable and indexed. Do not re-litigate this; just avoid committing secrets (Firebase service-account keys, tokens). Never have the user paste secrets into chat, since the chat itself is public. See [[reference-github-repo]] and [[feedback-take-action-via-cli]].
