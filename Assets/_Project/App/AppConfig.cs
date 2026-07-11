namespace BookLab.App
{
    // Small central place for app-wide constants.
    public static class AppConfig
    {
        // The assignment has no login system, so every book is stored under one
        // fixed "kid". Structuring data per-kid keeps it ready to scale to many
        // children later (that id would come from Firebase Auth in a real product) —
        // for the demo we just use this single value.
        public const string KidId = "demo";
    }
}
