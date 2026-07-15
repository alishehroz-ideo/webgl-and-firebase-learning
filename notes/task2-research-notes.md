# Task 2 — Search & Content Discovery
## Research & Discovery Notes (R&D)

*Working notes from the discovery discussion for the Adeeb Phase 1 assessment — Task 2. Captures the questions worked through and the reasoning behind each decision. This feeds the Task 2 handover document and the video walkthrough.*

**Author:** Ali Shehroz
**Task:** Search & Content Discovery (WebGL · Firebase Realtime Database)

---

## The task in one paragraph

Build a **search page** (input field + Search button) where a user searches by **content name** *or* **author name** over a content library stored in **Firebase Realtime Database**. Results are shown as **cards** (Name · Author · Date). The catch: the data is **real-world messy** and must be used **as-is** — we are **not allowed to clean or restructure it at the source**. Graded on: **parsing robustness, search performance, code architecture, and scalability thinking.** Architecture must be **MVC + event-driven.** Deliverables: Unity project/repo, WebGL build, handover document, and a 5–10 min video. Plus a **required written answer**: *"If the dataset grows to 10,000+ items, how would you redesign the system to improve performance?"*

---

## The core challenge

The useful metadata (name, author, date) is **crammed into one string field** (a field named something like `CoverInfo`). That single string may have:

- **Mixed separators** (`_` and `|`)
- **Inconsistent order** between entries
- **Missing / incomplete** fields
- **Extra / junk** data

Our job: pull **Name / Author / Date** out of that messy string **in code** — robustly, and **without hardcoding** for specific entries.

Example of the raw data shape in Firebase:

```json
"content": {
   "-Nx8a...": { "CoverInfo": "The Hungry Caterpillar_Eric Carle_1969" },
   "-Nx9b...": { "CoverInfo": "Goodnight Moon | Margaret Wise Brown | 1947" },
   "-NxAc...": { "CoverInfo": "1963|Maurice Sendak_Where the Wild Things Are" }
}
```

Each messy string becomes a clean record:

```json
Input:  "The Hungry Caterpillar_Eric Carle_1969"
Output: {
   "name":       "The Hungry Caterpillar",
   "author":     "Eric Carle",
   "date":       "1969",
   "raw":        "The Hungry Caterpillar_Eric Carle_1969",   // keep original, always
   "confidence": "high"
}
```

---

## Questions worked through (Q&A)

### Q1. What does *"each content item contains metadata inside a field similar to `CoverInfo`"* mean?

In Firebase, content is a list of items. Each item has **fields** (labelled boxes). One field — named roughly `CoverInfo` — holds **all** the metadata jammed into a **single text string**, instead of neat separate boxes for name/author/date.

The words *"similar to"* and the `*` are hedges: the real field might be named slightly differently, and the exact format is confirmed only when the real data/sample arrives. **Takeaway: don't hardcode the field name or assume a fixed format.**

### Q2. What is a "parser"?

A **parser** is a function/algorithm that takes **messy raw input**, makes sense of its pieces, and returns **clean structured data**. "To parse" = to break down and interpret. Everyday analogy: reading a scribbled envelope and working out which part is the street, the city, the postcode. That sorting-out step *is* parsing.

### Q3. What is "robustness"?

**Robustness = doesn't break when things go wrong.** A robust parser keeps working (or returns safe nulls) even on messy, broken, or missing input, instead of crashing. The opposite is **brittle** (works only when input is perfect). The grading criterion "data parsing robustness" means: *they will throw broken entries at the code on purpose and check it stays standing.*

### Q4. Why does the naive approach crash? (`author = parts[1]`)

```
parts = raw.split("_")   // cut the string at every "_"
author = parts[1]        // grab the 2nd piece  (code counts from 0)
```

- On `"The Hungry Caterpillar_Eric Carle_1969"` → 3 pieces, `parts[1]` = `"Eric Carle"`. ✅
- On `"Corduroy"` → **no** underscore → only **1** piece. `parts[1]` (the 2nd piece) **doesn't exist** → the program **errors out and stops**. 💥

One bad entry crashes the whole search page. The brittle code *assumed* every entry has name + author + date neatly split by `_`. Our parser never assumes — "no 2nd piece? fine, author = null" — and keeps going.

### Q5. "Shape-based", "rule-based", "specialist" — are these different parsers?

No — it's **one** parser, described with three adjectives:

| Word | What it describes | The contrast (what it's NOT) |
|---|---|---|
| **Rule-based** | *How* it decides — with simple hand-written rules | ...not AI/ML guessing |
| **Shape-based** | *What* the rules look at — the **appearance** of each piece | ...not the **position** of each piece |
| **Specialist** | Nickname for **one** rule (e.g. the date-catcher) | ...not the whole parser |

Full sentence: *a **rule-based** parser whose rules are **shape-based**, and each rule is a **"specialist."*** Analogy: sorting laundry by recognising each item (a sock is a sock) rather than by "the 2nd thing I grab always goes in the shirt pile."

### Q6. Is the data "random"?

No — it's **inconsistent / messy**, not random. Random = no pattern, meaningless noise (unparseable). Messy = the info **is** there and meaningful, just written in different **styles** each time. Analogy: 50 handwritten index cards, all noting *title, author, year*, but each in its own layout. **Because it's not random, it's parseable.**

### Q7. If we can't fix the data in Firebase, how does search even work?

We don't clean the data **in Firebase** — we build a **clean copy in the app's own memory** when it loads. That's allowed (the rule is only "don't modify the source").

Library analogy: we can't rewrite the library's messy card catalogue, but we can read every card once and jot a **neat list in our own notebook**, then search the notebook. Firebase stays messy; the app's "notebook" is tidy.

Flow (simple/demo version):
1. App loads all messy items from Firebase **once**.
2. Parser turns each into a clean `{name, author, date}` record **in memory**.
3. User types `carle` → app scans the **clean in-memory list** (no Firebase trip, no re-parse) → matches "Eric Carle" → shows the card.

Parsing happens **once, upfront**, so search is **instant** for 1,000+ items.

### Q8. At 100,000 entries, does every new user download + parse all of it?

**Yes — in the simple design, every visitor's browser would download all 100,000 messy entries and parse them all**, just to be ready to search. And **yes, that is far too much** — slow, heavy, wasteful.

This is **exactly** the problem the task's scalability question is testing. It's fine at ~1,000 items (what the demo does); it falls apart at 10,000+.

### Q9. How do we fix it? ("clean once on the server, send only the matches")

**Two moves:**

1. **Clean once, keep a separate clean copy on Firebase.** A **run-once script** (free) — or a **Cloud Function** for the automatic version (paid plan) — reads the messy data, runs our parser, and writes clean `{name, author, date}` into a **new, separate** `/searchIndex` node. The original messy data stays untouched.

   ```
   /content       ← ORIGINAL messy data (never touched)
   /searchIndex   ← CLEAN copy the script wrote (new)
   ```

2. **The browser only asks for the matches.** On search, the browser sends a **targeted request** to Firebase — *"give me items whose author starts with 'carle', max 20"* — and Firebase returns just those ~20. The browser **never** downloads the other 99,980.

   ```
   GET /searchIndex.json?orderBy="author"&startAt="carle"&endAt="carle"&limitToFirst=20
   ```

Net effect: the browser only ever touches **small slices** → fast at any size.

### Q10. But doesn't Firebase still scan all 1,000,000 entries on each search?

**No — because of an index.** Key idea: **when data is sorted, you don't check every entry, you jump to it.**

Phone-book analogy: to find "Carle" in a phone book of a million names, you don't read all million — it's alphabetical, so you flip straight to **C → Ca → Car**. If it were in random order, you'd have to check all million.

Firebase keeps the clean copy **sorted by the search field** (the `.indexOn` setting = an **index**). A search **jumps** to the matches instead of scanning everything.

Two separate things, at separate times:

| | When it runs | What it touches |
|---|---|---|
| **Cloud Function** (cleaning) | Only when an item is added/changed | Just **that one** new item |
| **Search** (finding) | When the user hits Search | **Jumps** to matches via the index — not all million |

The only full pass over the data is the **first** build of the clean copy — done **once, offline**, by the script.

### Q11. So we need an indexed field — must it be unique?

We mark a **field** (name / author) as **indexed** — but it must **NOT** be unique.

- **Unique** = no two items share the value (like an ID).
- **Indexed** = keep the field **sorted** for fast lookup; **duplicates are expected and fine.**

For author search we *want* duplicates — "Eric Carle" is on many books; a unique constraint would allow only one book per author. Phone-book analogy: sorted by surname, but a thousand "Smith"s is fine.

In Firebase:
- Item **key/ID** (`-Nx8a...`) → **unique** (auto-generated).
- Searchable **field** (author, name) → **indexed but NOT unique.**

Since we search by name **or** author, we index **both** fields (or keep one combined lowercase `searchText` field and index that).

---

## Parsing approaches compared

| # | Approach | What it is | Good | Bad | Fit |
|---|---|---|---|---|---|
| 1 | **Split & grab by position** | Cut on `_`, take piece #1/#2/#3 | Trivial | Breaks on reorder/missing/mixed separators; crashes | ❌ (the thing they warn against) |
| 2 | **One big regex** | A single fixed `(name)_(author)_(year)` pattern | Compact | Locks in one order/format | ❌ Too rigid |
| 3 | **Shape-based specialists** | Split, then ID each piece by *shape* | Robust, reusable, fast, deterministic, no hardcoding | Needs design + tuning to real data | ✅ **Chosen** |
| 4 | **Formal grammar parser** | Strict grammar rules (a mini-language) | Great for *structured* input | This data has no consistent rules | ❌ Wrong tool |
| 5 | **ML / NER model** | AI tags PERSON / DATE / TITLE | Handles fuzziness | Heavy, non-deterministic, awkward in C#/WebGL | ⚠️ Offline only |
| 6 | **LLM** | Ask an LLM to extract | Very accurate | Slow, costs, insecure in WebGL, non-deterministic | ⚠️ Offline precompute only |

**Chosen: #3 (rule-based, shape-based specialists)** — the only approach that is robust, non-hardcoded, fast/free, deterministic, and extensible all at once. In code: an `IFieldExtractor` interface with `DateExtractor`, `AuthorExtractor`, `NameExtractor` (a strategy/pipeline pattern).

**Where AI legitimately fits (hybrid):** rule-based specialists handle ~95% deterministically; the ugly, low-confidence stragglers can be cleaned **once, offline**, by an LLM — which is also a valid tool to **build the clean `/searchIndex`** for the scalability redesign. This shows the right default engineering tool *and* knowing where AI actually belongs.

---

## The shape-based parser — worked examples

| Raw `CoverInfo` string | → name | → author | → date | What made it tricky |
|---|---|---|---|---|
| `Goodnight Moon \| Margaret Wise Brown \| 1947` | Goodnight Moon | Margaret Wise Brown | 1947 | Different separator, extra spaces |
| `1963_Maurice Sendak_Where the Wild Things Are` | Where the Wild Things Are | Maurice Sendak | 1963 | **Date first** — position parsing would break |
| `Brown Bear\|Bill Martin Jr.\|03/1967` | Brown Bear | Bill Martin Jr. | 1967 | Odd date format + name with `Jr.` |
| `Where the Wild Things Are_Maurice Sendak\|1963\|promo` | Where the Wild Things Are | Maurice Sendak | 1963 | Mixed `_` and `\|` in one entry; junk `promo` dropped |
| `The Snowy Day_Ezra Jack Keats` | The Snowy Day | Ezra Jack Keats | *(null)* | No date; two name-like pieces |
| `Corduroy` | Corduroy | *(null)* | *(null)* | Only a title → nulls, low confidence, **no crash** |
| `""` / missing | *(null)* | *(null)* | *(null)* | Empty/garbage → skipped safely |

**The specialists and the shape each hunts for:**

| Specialist | Shape it recognises |
|---|---|
| 📅 Date | 4-digit year `1969`, or `03/1967`, `2019-05` (date pattern) |
| ✍️ Author | 2–3 Capitalised words, may have initials/suffix (`Eric Carle`, `J. Smith`, `Bill Martin Jr.`) |
| 📖 Name | whatever's left (usually the longest piece; often has an article `The`/`A`) |

---

## Scalability redesign — the required written answer

**Question:** *If the dataset grows to 10,000+ items, how would you redesign this system?*

**Problem with the simple design:** every visitor's browser downloads **all** items and parses **all** of them on the single WebGL thread, every page load — big download, CPU jank (freeze), and memory pressure.

**Redesign — do the heavy work once, off the browser; send each user only what they need:**

| | Who does the tidying? | What the browser downloads |
|---|---|---|
| ❌ Simple | **Every browser, every visit** | **All** items (e.g. 100,000) |
| ✅ Redesign | **Server, once** | **Only** the matching results (e.g. 20) |

1. **Precompute a clean index** — parse the messy data **once** (run-once script, or a Cloud Function that re-runs only on changes) and store clean records in a **separate** `/searchIndex` node. Raw data untouched (still "don't modify the source"). Browser reads clean data → **zero parsing on load.**
2. **Server-side search + pagination** — with an **indexed** field, the browser sends a targeted "starts-with, limit 20" query; Firebase **jumps** to matches (no full scan) and returns only that page.
3. **Or a dedicated search service** (Algolia / Typesense / Elasticsearch) for instant, typo-tolerant, multi-field search at scale.
4. **Cache** results so repeat searches don't re-fetch.

**Honest limitation:** Firebase's built-in query is "starts-with, one field at a time." For match-in-the-middle, typo-tolerance, or name+author together, a dedicated search service is the right tool — same principle: the browser only ever gets the few matches.

---

## Architecture (MVC + event-driven) — reuse from Task 1

Task 2 reuses the Core built for Task 1 (Firebase REST client, `EventBus`, `UiFactory`, MVC screen system, caching). New pieces:

- **Model** — `ContentItem` (raw) + `ParsedContent` (name/author/date) + the parser (specialists) + the search index; a `SearchService` (fetch → parse → cache → query).
- **View** — the search screen (input, button, results list of cards), built with `UiFactory`.
- **Controller** — `SearchController`: handles the search event, queries the service, updates the view.
- **Event-driven** — button click publishes `SearchRequested(query)` on the `EventBus`; controller handles it and publishes `SearchResults(list)`; the view renders. Screens stay decoupled.

---

## Key takeaways / decisions

- **Parse in the app's memory, never at the source** — Firebase stays messy; the app builds a clean copy.
- **Parse by shape, not position** — the core of parsing robustness; survives reorder / mixed separators / missing fields without crashing.
- **Chosen parser:** rule-based, shape-based "specialists" (`IFieldExtractor` pipeline) — robust, deterministic, extensible, no hardcoding.
- **Demo scope:** download + parse + in-memory search (correct and fast for the stated 1,000+).
- **Scalability answer (explained, not built):** clean once into a separate indexed `/searchIndex`, then server-side paginated queries (or a search service) so the browser only ever gets the matches — fast at 10,000+ or a million.
- **Where AI fits:** offline, to build the clean index and clean the low-confidence stragglers — not in the live search hot path.
