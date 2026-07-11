using UnityEngine;
using UnityEngine.UI;
using BookLab.Core.UI;
using BookLab.Core.Assets;
using BookLab.Models;
using BookLab.Services;
using BookLab.App;

namespace BookLab.Features.MainMenu
{
    // The home "shelf": shows the kid's saved books as cover-cards.
    // Display-only for now (buttons/navigation come next slice).
    public class MainMenuController : MonoBehaviour
    {
        RectTransform _grid;

        public void Show(Transform canvas)
        {
            var bg = UiFactory.Panel("MainMenu", canvas, new Color(0.12f, 0.13f, 0.18f));
            UiFactory.Stretch(bg);

            var title = UiFactory.Label("Title", bg, "My Books", 64, Color.white);
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0, 1); trt.anchorMax = new Vector2(1, 1); trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = new Vector2(0, -30);
            trt.sizeDelta = new Vector2(0, 110);

            var gridGO = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            _grid = (RectTransform)gridGO.transform;
            _grid.SetParent(bg, false);
            _grid.anchorMin = Vector2.zero; _grid.anchorMax = Vector2.one;
            _grid.offsetMin = new Vector2(60, 60); _grid.offsetMax = new Vector2(-60, -160);
            var glg = gridGO.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(300, 380);
            glg.spacing = new Vector2(40, 40);
            glg.childAlignment = TextAnchor.UpperCenter;

            Refresh();
        }

        async void Refresh()
        {
            foreach (Transform c in _grid) Destroy(c.gameObject);

            var books = await ContentApi.ListBooks(AppConfig.KidId);
            Debug.Log($"[MainMenu] loaded {books.Count} book(s)");

            var catalog = await ContentApi.GetCatalog();
            foreach (var book in books)
                CreateCard(book, catalog);
        }

        void CreateCard(BookModel book, AssetCatalog catalog)
        {
            var card = UiFactory.Panel($"Card_{book.id}", _grid, new Color(0.20f, 0.22f, 0.28f));

            var cover = UiFactory.Image("Cover", card);
            var crt = cover.rectTransform;
            crt.anchorMin = new Vector2(0, 0.22f); crt.anchorMax = new Vector2(1, 1);
            crt.offsetMin = new Vector2(12, 12); crt.offsetMax = new Vector2(-12, -12);
            cover.color = new Color(0.30f, 0.30f, 0.36f);
            cover.preserveAspect = true;

            var titleLbl = UiFactory.Label("Title", card, book.title, 28, Color.white);
            var lrt = titleLbl.rectTransform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = new Vector2(1, 0.22f);
            lrt.offsetMin = new Vector2(8, 4); lrt.offsetMax = new Vector2(-8, -4);

            ApplyCover(cover, book, catalog);
        }

        // Uses the first page's background image as the cover (real download via AssetService,
        // which caches it — so this also proves the caching layer visually).
        async void ApplyCover(Image cover, BookModel book, AssetCatalog catalog)
        {
            if (book.pages == null || book.pages.Count == 0) return;
            var def = catalog.Find(book.pages[0].backgroundId);
            if (def == null) return;

            var sprite = await AssetService.GetSprite(def.url);
            if (sprite != null && cover != null)
            {
                cover.sprite = sprite;
                cover.color = Color.white;
            }
        }
    }
}
