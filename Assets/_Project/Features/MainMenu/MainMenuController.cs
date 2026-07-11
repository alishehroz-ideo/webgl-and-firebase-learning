using UnityEngine;
using UnityEngine.UI;
using BookLab.Core.UI;
using BookLab.Core.Assets;
using BookLab.Models;
using BookLab.Services;
using BookLab.App;

namespace BookLab.Features.MainMenu
{
    // The home "shelf": shows the kid's saved books as cover-cards, with a
    // "Create New" button and tappable cards.
    public class MainMenuController : MonoBehaviour
    {
        RectTransform _grid;
        Text _status;

        public void Show(Transform canvas)
        {
            var bg = UiFactory.Panel("MainMenu", canvas, new Color(0.12f, 0.13f, 0.18f));
            UiFactory.Stretch(bg);

            var title = UiFactory.Label("Title", bg, "My Books", 64, Color.white);
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0, 1); trt.anchorMax = new Vector2(1, 1); trt.pivot = new Vector2(0.5f, 1);
            trt.anchoredPosition = new Vector2(0, -30);
            trt.sizeDelta = new Vector2(0, 110);

            // Create New button (top-right)
            var createBtn = UiFactory.Button("CreateNew", bg, "+  Create New", new Color(0.20f, 0.55f, 0.35f), 30);
            var crt = (RectTransform)createBtn.transform;
            crt.anchorMin = new Vector2(1, 1); crt.anchorMax = new Vector2(1, 1); crt.pivot = new Vector2(1, 1);
            crt.anchoredPosition = new Vector2(-40, -35);
            crt.sizeDelta = new Vector2(300, 80);
            createBtn.onClick.AddListener(OnCreateNew);

            // Status line (bottom) — visible feedback that a click registered
            _status = UiFactory.Label("Status", bg, "", 26, new Color(0.7f, 0.8f, 1f));
            var srt = _status.rectTransform;
            srt.anchorMin = new Vector2(0, 0); srt.anchorMax = new Vector2(1, 0); srt.pivot = new Vector2(0.5f, 0);
            srt.anchoredPosition = new Vector2(0, 20);
            srt.sizeDelta = new Vector2(0, 50);

            // Grid of cards
            var gridGO = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            _grid = (RectTransform)gridGO.transform;
            _grid.SetParent(bg, false);
            _grid.anchorMin = Vector2.zero; _grid.anchorMax = Vector2.one;
            _grid.offsetMin = new Vector2(60, 90); _grid.offsetMax = new Vector2(-60, -160);
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

            // whole card is clickable
            var btn = card.gameObject.AddComponent<Button>();
            btn.targetGraphic = card.GetComponent<Image>();
            btn.onClick.AddListener(() => OnOpenBook(book));

            var cover = UiFactory.Image("Cover", card);
            var covrt = cover.rectTransform;
            covrt.anchorMin = new Vector2(0, 0.22f); covrt.anchorMax = new Vector2(1, 1);
            covrt.offsetMin = new Vector2(12, 12); covrt.offsetMax = new Vector2(-12, -12);
            cover.color = new Color(0.30f, 0.30f, 0.36f);
            cover.preserveAspect = true;
            cover.raycastTarget = false;   // let clicks fall through to the card button

            var titleLbl = UiFactory.Label("Title", card, book.title, 28, Color.white);
            var lrt = titleLbl.rectTransform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = new Vector2(1, 0.22f);
            lrt.offsetMin = new Vector2(8, 4); lrt.offsetMax = new Vector2(-8, -4);
            titleLbl.raycastTarget = false;

            ApplyCover(cover, book, catalog);
        }

        // Uses the first page's background as the cover (real download via AssetService, cached).
        async void ApplyCover(Image cover, BookModel book, AssetCatalog catalog)
        {
            if (book.pages == null || book.pages.Count == 0) return;
            var def = catalog.Find(book.pages[0].backgroundId);
            if (def == null) return;

            var sprite = await AssetService.GetSprite(def.url);
            if (sprite != null && cover != null) { cover.sprite = sprite; cover.color = Color.white; }
        }

        void OnCreateNew()
        {
            Debug.Log("[MainMenu] Create New clicked");
            if (_status) _status.text = "Create New clicked — the Editor screen comes next.";
        }

        void OnOpenBook(BookModel book)
        {
            Debug.Log($"[MainMenu] open book '{book.title}' ({book.id})");
            if (_status) _status.text = $"Opening \"{book.title}\" …  (Playback screen comes next)";
        }
    }
}
