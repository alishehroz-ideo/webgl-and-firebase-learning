using System;
using System.Collections.Generic;

namespace BookLab.Models
{
    // A whole saved book — the "recipe".
    // It is small: just references + numbers, never the actual images.
    [Serializable]
    public class BookModel
    {
        public string id;
        public string title;
        public string createdAt;    // ISO timestamp
        public string updatedAt;    // ISO timestamp
        public string coverBase64;  // tiny snapshot shown on the shelf (keeps us off Firebase Storage)
        public List<PageModel> pages = new List<PageModel>();
    }
}
