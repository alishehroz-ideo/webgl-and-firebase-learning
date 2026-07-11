---
name: project-public-git-sync-demo
description: "This Unity project is a company demo, git-synced office<->home with the Claude chat history included, in a PUBLIC repo by user's explicit choice"
metadata: 
  node_type: memory
  type: project
  originSessionId: 48b57109-425f-47cd-a54a-482ff5cdedd8
---

`d:\DummyProjects\webgl and firebase learning` is a Unity 6 (6000.4.3f1, URP) WebGL + Firebase project the user is preparing as a **demo to submit to their company**. It is git-synced between office and home.

Key setup facts:
- The Claude Code chat transcripts + memory are intentionally committed into the repo under `.claude-history/` so the conversation travels with the code. `.claude/sync-history.ps1` mirrors `~/.claude/projects/<derived>/` <-> `.claude-history/`; `sync.ps1 push|pull` is the manual office<->home workflow. Sync is manual (no hooks).
- For chat `restore` to work, the project must live at the **same absolute path** on both machines.
- The repo is **PUBLIC** — the user confirmed this twice after I flagged that a company demo + full chat log would be world-readable and indexed. Do not re-litigate this; just avoid committing secrets (Firebase service-account keys, tokens). Never have the user paste secrets into chat, since the chat itself is public. See [[reference-github-repo]] and [[feedback-take-action-via-cli]].
