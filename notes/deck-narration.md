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

## Slide 5 — Asset loading & caching  (ladder: Memory · Browser storage · Download; then fridge · warehouse)
🎙 **SAY:**
> "Now, loading images — the backgrounds and stickers. They all live in the cloud and load only when they're first needed, and then they're cached so they never download twice.
>
> **[↓ Memory]** The first place is memory — instant to reuse, but gone the moment the tab closes.
>
> **[↓ Browser storage]** The second is the browser's own storage — so images survive even a page reload.
>
> **[↓ Download]** Only if both of those miss do we download the image once from Firebase, and then save it into both places.
>
> **[↓ fridge]** The easy way to picture it: the cache is like your fridge — it holds what you're using right now, it's small, and old items get pushed out.
>
> **[↓ warehouse]** And the cloud is the warehouse — it has everything and can always restock. So if something's evicted from the fridge, we just fetch it again — nothing is ever lost.
>
> I'm honest about the browser's limits too: that storage is per-browser and can be cleared, there are no background threads, and very large images hit a memory ceiling — which is exactly why downloading each one only once matters so much."

## Slide 6 — Key decisions & challenges  (4 cards — one spotlight step each)
🎙 **SAY:**
> "The interesting engineering was really in four places.
>
> **[↓ card 1]** First — Firebase has a ready-made toolkit, but it simply doesn't run inside a browser game. So I talk to Firebase directly over the web, and I wrapped each request so it waits smoothly in the background without ever freezing the screen.
>
> **[↓ card 2]** Second, placement — where a sticker sits. A pixel position is meaningless when screens are different sizes, so I store every position as a percentage of the page. That's why a book made on a phone lines up perfectly on a desktop.
>
> **[↓ card 3]** Third, caching with no hard drive — a browser doesn't have real file storage. So I keep images in memory and mirror them into the browser's own storage; and if the same image is asked for twice at once, both requests share a single download.
>
> **[↓ card 4]** And fourth — the hardest one — just getting it to start online. The host kept mangling the compressed game files, so it wouldn't boot. The fix was to ship the files uncompressed and let the host compress them itself. That one took real digging."

## Slide 7 — Any use of AI tools  (4 points — one spotlight step each)
🎙 **SAY:**
> "On AI — I built this with Claude Code as a pair-programmer, and I'm upfront about that. It genuinely sped things up, while I set the direction and reviewed everything.
>
> **[↓ Planning]** We started by talking through the brief together and shaping the structure — the layered design you saw earlier.
>
> **[↓ Implementation]** It then wrote a lot of the code alongside me — the models, the Firebase client, save and load, the cache, the screens — which I reviewed and adjusted as we went.
>
> **[↓ Debugging]** It was a real help on the tricky parts — the WebGL and Firebase issues that took the most digging.
>
> **[↓ Setup & deploys]** And it kept the setup and deploys fast, straight from the command line.
>
> The result was quicker to build — but the direction, the decisions, and the understanding are mine. I can walk through and extend any part of it."
