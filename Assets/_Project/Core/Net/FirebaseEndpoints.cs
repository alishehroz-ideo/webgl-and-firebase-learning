namespace BookLab.Core.Net
{
    // One place for every Firebase URL (same idea as your GameBullEndpoints).
    // Realtime Database is one big JSON tree; each path + ".json" is a REST URL.
    // Change the database in ONE spot here and the whole app follows.
    public static class FirebaseEndpoints
    {
        // Realtime Database URL for the assessment project (adeeb-booklab-07111926).
        public const string BaseUrl = "https://adeeb-booklab-07111926-default-rtdb.firebaseio.com";

        // --- Asset catalog (the picker menu of backgrounds + stickers) ---
        public static string Catalog() => $"{BaseUrl}/assetCatalog.json";

        // --- Books (per kid: /books/{kidId}/{bookId}) ---
        public static string Books(string kidId)               => $"{BaseUrl}/books/{kidId}.json";
        public static string Book(string kidId, string bookId) => $"{BaseUrl}/books/{kidId}/{bookId}.json";
    }
}
