using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using BookLab.Core.Net;

namespace BookLab.Features.Search
{
    // The Model/service for Task 2. It:
    //   1. loads the messy StoryLibary from Firebase (one GET, via the reused FirebaseClient),
    //   2. finds every *CoverInfo string (by the "key ends with CoverInfo" rule — no hardcoded ids),
    //   3. parses each into a clean ParsedContent (CoverInfoParser),
    //   4. searches that in-memory list by content name OR author.
    //
    // This is the "download all + parse all + search in memory" approach — correct and instant for
    // the stated 1,000+ items. The 10,000+ redesign (precomputed index + server-side paging) is
    // described in the handover.
    public class SearchService
    {
        readonly List<ParsedContent> _all = new List<ParsedContent>();

        public IReadOnlyList<ParsedContent> All => _all;
        public int Count => _all.Count;
        public bool IsLoaded { get; private set; }

        // Fetch + parse the whole library once. Returns true on success.
        public async Task<bool> LoadAsync()
        {
            _all.Clear();
            IsLoaded = false;

            string json = await FirebaseClient.GetJson(FirebaseEndpoints.StoryLibrary());
            if (string.IsNullOrEmpty(json) || json == "null")
            {
                Debug.LogWarning("[SearchService] StoryLibary is empty or unreachable.");
                return false;
            }

            try
            {
                var root = JObject.Parse(json);   // storyId -> story object
                foreach (var story in root.Properties())
                {
                    var storyObj = story.Value as JObject;
                    if (storyObj == null) continue;

                    // CoverInfo lives inside "Story"; fall back to the story root just in case.
                    string cover = FindCoverInfo(storyObj["Story"] as JObject) ?? FindCoverInfo(storyObj);
                    if (cover == null) continue;

                    var parsed = CoverInfoParser.Parse(cover);
                    parsed.Id = story.Name;   // the Firebase key IS the story id
                    _all.Add(parsed);
                }

                IsLoaded = true;
                Debug.Log($"[SearchService] loaded & parsed {_all.Count} items from StoryLibary.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[SearchService] load/parse failed: " + e.Message);
                return false;
            }
        }

        // Search by content name OR author name (case-insensitive substring).
        // An empty query returns everything.
        public List<ParsedContent> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<ParsedContent>(_all);

            string q = query.Trim().ToLowerInvariant();
            var results = new List<ParsedContent>();
            foreach (var item in _all)
            {
                bool nameHit   = item.Name   != null && item.Name.ToLowerInvariant().Contains(q);
                bool authorHit = item.Author != null && item.Author.ToLowerInvariant().Contains(q);
                if (nameHit || authorHit) results.Add(item);
            }
            return results;
        }

        // Find the value of the key that ENDS WITH "CoverInfo" (never a specific id).
        static string FindCoverInfo(JObject obj)
        {
            if (obj == null) return null;
            foreach (var p in obj.Properties())
            {
                if (p.Name.EndsWith("CoverInfo", StringComparison.OrdinalIgnoreCase))
                    return p.Value != null && p.Value.Type == JTokenType.String
                        ? p.Value.Value<string>()
                        : p.Value?.ToString();
            }
            return null;
        }
    }
}
