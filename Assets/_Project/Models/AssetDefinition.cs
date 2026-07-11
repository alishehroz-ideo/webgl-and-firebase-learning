using System;

namespace BookLab.Models
{
    // One available background or sticker in the catalog (the "ingredients menu").
    // 'id' is filled in from its key in the Firebase catalog; 'url' is where the
    // actual image lives (a free host), which AssetService downloads and caches.
    [Serializable]
    public class AssetDefinition
    {
        public string id;
        public string name;   // shown in the picker menu
        public string url;    // where the real image lives
    }
}
