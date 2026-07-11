using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using BookLab.Core.Net;

namespace BookLab.Core.Assets
{
    // Downloads an image ONCE, then reuses it. This is the "critical" caching layer.
    //
    // Two tiers:
    //   1) memory — instant reuse while the app is open
    //   2) disk   — survives page reloads (in WebGL this is the browser's IndexedDB)
    // Plus in-flight de-dupe: if two callers ask for the same image at the same time,
    // they share ONE download instead of fetching it twice.
    public static class AssetService
    {
        // Tier 1: url -> ready Sprite, kept for this session.
        static readonly Dictionary<string, Sprite>       _memory   = new Dictionary<string, Sprite>();

        // url -> the download currently in progress (so duplicates join it, not restart it).
        static readonly Dictionary<string, Task<Sprite>> _inFlight = new Dictionary<string, Task<Sprite>>();

        static string CacheDir => Path.Combine(Application.persistentDataPath, "assetCache");

        // The one call the app uses: give a url, get back a ready Sprite (from cache if possible).
        public static Task<Sprite> GetSprite(string url)
        {
            if (string.IsNullOrEmpty(url))                    return Task.FromResult<Sprite>(null);
            if (_memory.TryGetValue(url, out var cached))     return Task.FromResult(cached);   // tier 1 hit
            if (_inFlight.TryGetValue(url, out var running))  return running;                   // already downloading

            var task = LoadSprite(url);
            _inFlight[url] = task;
            return task;
        }

        static async Task<Sprite> LoadSprite(string url)
        {
            try
            {
                byte[] bytes = ReadFromDisk(url);                 // tier 2: saved on a previous visit?
                if (bytes == null)
                {
                    bytes = await FirebaseClient.GetBytes(url);   // tier 3: download it
                    if (bytes != null) WriteToDisk(url, bytes);   // and keep a copy for next time
                }
                if (bytes == null) return null;

                var sprite = ToSprite(bytes);
                if (sprite != null) _memory[url] = sprite;        // remember it in memory
                return sprite;
            }
            finally
            {
                _inFlight.Remove(url);   // download finished (win or lose) — clear the "in progress" mark
            }
        }

        static Sprite ToSprite(byte[] bytes)
        {
            var tex = new Texture2D(2, 2);
            if (!tex.LoadImage(bytes)) return null;               // decodes the PNG / JPG bytes
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        // ---- disk tier (this is the browser's IndexedDB when running in WebGL) ----

        static byte[] ReadFromDisk(string url)
        {
            try
            {
                string p = PathFor(url);
                return File.Exists(p) ? File.ReadAllBytes(p) : null;
            }
            catch { return null; }   // any disk hiccup just means "not cached"
        }

        static void WriteToDisk(string url, byte[] bytes)
        {
            try
            {
                Directory.CreateDirectory(CacheDir);
                File.WriteAllBytes(PathFor(url), bytes);
            }
            catch { /* caching is best-effort; ignore write failures */ }
        }

        // Filename = a stable hash of the url (FNV-1a): odd characters can't break it,
        // and the same url always maps to the same file across sessions.
        static string PathFor(string url)
        {
            ulong hash = 14695981039346656037UL;
            foreach (char c in url) { hash ^= c; hash *= 1099511628211UL; }
            return Path.Combine(CacheDir, hash.ToString("x") + ".img");
        }
    }
}
