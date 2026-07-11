using System;
using System.Collections.Generic;

namespace BookLab.Models
{
    // The picker menu exactly as it sits in Firebase: two groups of assets,
    // each keyed by id. Loaded once, kept in memory, looked up instantly.
    [Serializable]
    public class AssetCatalog
    {
        public Dictionary<string, AssetDefinition> backgrounds = new Dictionary<string, AssetDefinition>();
        public Dictionary<string, AssetDefinition> objects     = new Dictionary<string, AssetDefinition>();

        // Instant id -> asset lookup (checks both groups). Returns null if not found.
        // This is the "dictionary teleport" — no scanning, fast at any catalog size.
        public AssetDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (backgrounds != null && backgrounds.TryGetValue(id, out var bg)) return bg;
            if (objects     != null && objects.TryGetValue(id, out var ob))     return ob;
            return null;
        }
    }
}
