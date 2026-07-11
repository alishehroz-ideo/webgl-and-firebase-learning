namespace BookLab.Core.Net
{
    // One place for every Firebase URL (same idea as your GameBullEndpoints).
    // Realtime Database is one big JSON tree; each path + ".json" is a REST URL.
    // Change the database in ONE spot here and the whole app follows.
    public static class FirebaseEndpoints
    {
        // TODO: paste your Realtime Database URL here once the Firebase project exists.
        //   e.g.  "https://adeeb-booklab-default-rtdb.firebaseio.com"
        public const string BaseUrl = "https://YOUR-PROJECT-default-rtdb.firebaseio.com";

        // --- Asset catalog (the picker menu of backgrounds + stickers) ---
        public static string Catalog() => $"{BaseUrl}/assetCatalog.json";

        // --- Books (per kid: /books/{kidId}/{bookId}) ---
        public static string Books(string kidId)               => $"{BaseUrl}/books/{kidId}.json";
        public static string Book(string kidId, string bookId) => $"{BaseUrl}/books/{kidId}/{bookId}.json";
    }
}
