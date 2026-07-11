using System;
using System.Collections.Generic;

namespace BookLab.Models
{
    // One page of a book: a background plus the stickers placed on it.
    [Serializable]
    public class PageModel
    {
        public string backgroundId;                                        // which background (assetCatalog/backgrounds)
        public List<PlacedObjectModel> objects = new List<PlacedObjectModel>();
    }
}
