using System;

namespace BookLab.Models
{
    // One sticker / furniture item placed on a page.
    // Position + size are NORMALIZED (0..1), never pixels, so a saved book
    // replays correctly on any screen size.
    [Serializable]
    public class PlacedObjectModel
    {
        public string assetId;   // which sticker  (points into assetCatalog/objects)
        public float  x;         // 0..1 across the page  (0 = left,   1 = right)
        public float  y;         // 0..1 up the page      (0 = bottom, 1 = top)
        public float  scale = 1f;
        public float  rotation;  // degrees
        public int    z;         // draw order  (higher = drawn on top)
    }
}
