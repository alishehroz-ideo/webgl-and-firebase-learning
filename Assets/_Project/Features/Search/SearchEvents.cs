using System.Collections.Generic;

namespace BookLab.Features.Search
{
    // The two events that wire the Search screen together (event-driven, via the shared EventBus).
    // The View and Controller never call each other directly — they talk through these.

    // Published by the View when the user presses Search.
    public class SearchRequested
    {
        public string Query;
        public SearchRequested(string query) { Query = query; }
    }

    // Published by the Controller once results are ready; the View renders them as cards.
    public class SearchResults
    {
        public string Query;
        public List<ParsedContent> Results;
    }
}
