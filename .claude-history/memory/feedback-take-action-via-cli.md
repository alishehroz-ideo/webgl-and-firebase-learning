---
name: feedback-take-action-via-cli
description: User prefers Claude to run CLI commands itself rather than hand over step-by-step instructions
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 48b57109-425f-47cd-a54a-482ff5cdedd8
---

When a task needs shell/CLI work, the user wants me to actually run the commands myself, not hand them a numbered list of commands to type ("use cli bro, dnt order me").

**Why:** Being given imperative instruction lists reads as bossy and shifts work back onto them; they came here to have it done.

**How to apply:** Drive tools/CLI directly. Only ask the user to act for the irreducible human-only steps (e.g. an OAuth "Authorize" click), and frame those as "just approve this," not commands. For `gh auth login`, use the web/device flow in the background and relay only the one-time code + URL. See [[project-public-git-sync-demo]].
