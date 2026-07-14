# BookLab — Deck Voice-over (per slide)

The redesigned deck (`video/deck-standalone.html`), recorded slide by slide with the
arrow-key spotlight stepping through each slide's parts. Convert each **SAY** block to
voice (Speechma / ElevenLabs) and lay it under the matching slide recording.

Covers the six required points: overview · architecture · asset loading & caching ·
data saved/loaded · key decisions & challenges · AI tools.

---

## Slide 1 — Cover / title
🎙 **SAY:**
> "Hi — this is my submission for Task 1 of the assessment: BookLab, the interactive content-creation task. It's a storybook maker for kids that runs right in the browser — built in Unity, exported to WebGL, with everything saved on Firebase. Now I'll walk you through what I built and how it works."

## Slide 2 — Overview  (3 screen cards: shelf · editor · playback — step ↓ to spotlight each)
🎙 **SAY:**
> "The whole app is one simple loop across just three screens — make a book, save it, read it back.
>
> **[↓ Pick a book]** First, the shelf — this is it: every saved book shows up as a cover, with a 'Create New' button to start fresh.
>
> **[↓ Make it]** Tapping Create opens the editor, shown here — you pick a background, drag stickers on and scale them, add pages, name it, and save.
>
> **[↓ Read it]** And opening a saved book plays it back, read-only — here's one open, flipping through the pages exactly as they were made.
>
> It targets full HD and scales to any screen, needs no install — just a URL — and runs on Firebase's free plan."

## Slide 3 — Architecture  (real project folder + legend: App · Features · Services · Core · Models)
🎙 **SAY:**
> "Here's the architecture — and you can see it directly in my Unity project. The code is split into layers, each a folder with one clear job.
>
> **[↓ App]** At the top, App — the starter and traffic-cop: it boots the app and decides which screen you're on.
>
> **[↓ Features]** Features holds the three screens — Main Menu, Editor, and Playback. Each is split into what you see and the logic behind it — that's the MVC pattern.
>
> **[↓ Services]** Services is the front desk: one class, ContentApi, that the whole app goes through to save and load — so nothing else needs to know how storage works.
>
> **[↓ Core]** Core is the engine room: the internet line that talks to Firebase, the messenger that passes events between screens, and the image cache.
>
> **[↓ Models]** And Models is the blueprint — plain data describing what a book, a page, and a sticker are, with no pictures inside.
>
> Two rules keep it tidy: every layer only talks to the one below it, and screens never call each other — they just announce an event like 'open this book,' and the traffic-cop switches the screen. That decoupling is what makes it easy to extend."

## Slide 4 — Data  (recipe card + real Firebase book, then the save/load flow: Editor · Front desk · Internet line · Cloud)
🎙 **SAY:**
> "Now, how a book is actually saved and loaded. The key idea: I save the recipe, not the picture.
>
> **[↓ recipe card]** Every sticker a child places is stored as just a few numbers — which sticker it is, and where it sits, as percentages across and up the page. So 'a camel, 62% across, 55% up' becomes this tiny line of data. Because it's percentages, not pixels, the book replays correctly on any screen size.
>
> **[↓ Firebase shot]** And here's that exact book living in the real Firebase database — just text: a title, and each page's stickers with their positions. No images are stored, so it stays tiny and fast.
>
> **[↓ Editor]** Saving flows one way — the editor builds the recipe,
> **[↓ Front desk]** hands it to the front desk, ContentApi,
> **[↓ Internet line]** which passes it to the internet line,
> **[↓ Cloud database]** that writes it into Firebase's Realtime Database, under that child's own path. Loading is the same path in reverse.
>
> One key decision: I talk to Firebase over plain web requests — REST — because the normal Firebase toolkit doesn't run inside a browser game at all. That single choice shaped the whole data layer."
