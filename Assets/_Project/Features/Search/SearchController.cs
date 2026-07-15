using UnityEngine;
using BookLab.Core.Events;

namespace BookLab.Features.Search
{
    // The Controller / brain. Owns the SearchService, loads the library on start, and turns
    // SearchRequested events into SearchResults (via the service). It never touches the UI directly —
    // it only speaks through the EventBus. This is the "C" in MVC + the event-driven wiring.
    public class SearchController : MonoBehaviour
    {
        readonly SearchService _service = new SearchService();

        void OnEnable()  => EventBus.Subscribe<SearchRequested>(OnSearch);
        void OnDisable() => EventBus.Unsubscribe<SearchRequested>(OnSearch);

        async void Start()
        {
            EventBus.Publish(new SearchResults { Loading = true });          // View shows "Loading…"
            bool ok = await _service.LoadAsync();
            if (!ok) { EventBus.Publish(new SearchResults { Error = true }); return; }

            // Show the whole library initially (empty query = everything).
            EventBus.Publish(new SearchResults { Query = "", Results = _service.Search("") });
        }

        void OnSearch(SearchRequested req)
        {
            if (!_service.IsLoaded) return;
            EventBus.Publish(new SearchResults { Query = req.Query, Results = _service.Search(req.Query) });
        }
    }
}
