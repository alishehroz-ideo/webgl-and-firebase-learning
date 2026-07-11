using UnityEngine;
using BookLab.Models;
using BookLab.Services;

namespace BookLab.App
{
    // TEMPORARY end-to-end test. Drop this on an empty GameObject and press Play,
    // then watch the Console: it reads the catalog + the demo book from real Firebase,
    // then writes a fresh test book back. Delete this file before shipping.
    public class ApiTester : MonoBehaviour
    {
        async void Start()
        {
            Debug.Log("[ApiTester] ---- Firebase round-trip test ----");

            // 1) READ the catalog
            var catalog = await ContentApi.GetCatalog();
            Debug.Log($"[ApiTester] catalog: {catalog.backgrounds.Count} backgrounds, {catalog.objects.Count} objects");
            var sofa = catalog.Find("furn_sofa");
            Debug.Log($"[ApiTester] lookup furn_sofa -> {(sofa != null ? $"{sofa.name} @ {sofa.url}" : "NOT FOUND")}");

            // 2) READ this kid's books
            var books = await ContentApi.ListBooks(AppConfig.KidId);
            Debug.Log($"[ApiTester] '{AppConfig.KidId}' has {books.Count} book(s):");
            foreach (var b in books)
                Debug.Log($"[ApiTester]    - {b.title}  ({b.pages.Count} pages)");

            // 3) WRITE a brand-new book back
            var fresh = new BookModel { title = "Test from Unity " + Random.Range(1000, 9999) };
            fresh.pages.Add(new PageModel { backgroundId = "bg_garden" });
            bool ok = await ContentApi.SaveBook(AppConfig.KidId, fresh);
            Debug.Log($"[ApiTester] SaveBook -> {(ok ? "OK  id=" + fresh.id : "FAILED")}");

            Debug.Log("[ApiTester] ---- done ----");
        }
    }
}
