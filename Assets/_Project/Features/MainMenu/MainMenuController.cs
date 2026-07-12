using UnityEngine;
using UnityEngine.UI;
using BookLab.Core.UI;
using BookLab.Core.Assets;
using BookLab.Core.Events;
using BookLab.Models;
using BookLab.Services;
using BookLab.App;

namespace BookLab.Features.MainMenu
{
    // The home "shelf": shows the kid's saved books as cover-cards, with a
    // "Create New" button and tappable cards. Raises nav events (never navigates itself).
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

            var createBtn = UiFactory.Button("CreateNew", bg, "+  Create New", new Color(0.20f, 0.55f, 0.35f), 30);
            var crt = (RectTransform)createBtn.transform;
            crt.anchorMin = new Vector2(1, 1); crt.anchorMax = new Vector2(1, 1); crt.pivot = new Vector2(1, 1);
            crt.anchoredPosition = new Vector2(-40, -35);
            crt.sizeDelta = new Vector2(300, 80);
            createBtn.onClick.AddListener(() => EventBus.Publish(new CreateBookRequest()));

            // Scrollable area for the shelf
            var scrollGO = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            var scrollRt = (RectTransform)scrollGO.transform;
            scrollRt.SetParent(bg, false);
            scrollRt.anchorMin = Vector2.zero; scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(60, 40); scrollRt.offsetMax = new Vector2(-60, -160);
            var scroll = scrollGO.GetComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40;

            // Viewport (masks content) with an invisible raycast catcher so wheel/drag works anywhere
            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewportRt = (RectTransform)viewportGO.transform;
            viewportRt.SetParent(scrollRt, false);
            UiFactory.Stretch(viewportRt);
            viewportGO.GetComponent<Image>().color = new Color(1, 1, 1, 0);   // invisible, still catches input

            // Content: the grid, height driven by a ContentSizeFitter so it grows + scrolls
            var contentGO = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            _grid = (RectTransform)contentGO.transform;
            _grid.SetParent(viewportRt, false);
            _grid.anchorMin = new Vector2(0, 1); _grid.anchorMax = new Vector2(1, 1); _grid.pivot = new Vector2(0.5f, 1);
            _grid.anchoredPosition = Vector2.zero;
            var glg = contentGO.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(300, 380);
            glg.spacing = new Vector2(40, 40);
            glg.padding = new RectOffset(10, 10, 10, 10);
            glg.childAlignment = TextAnchor.UpperCenter;
            var fitter = contentGO.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scroll.viewport = viewportRt;
            scroll.content = _grid;

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

            var btn = card.gameObject.AddComponent<Button>();
            btn.targetGraphic = card.GetComponent<Image>();
            btn.onClick.AddListener(() => EventBus.Publish(new OpenBookRequest { Book = book }));

            var cover = UiFactory.Image("Cover", card);
            var covrt = cover.rectTransform;
            covrt.anchorMin = new Vector2(0, 0.22f); covrt.anchorMax = new Vector2(1, 1);
            covrt.offsetMin = new Vector2(12, 12); covrt.offsetMax = new Vector2(-12, -12);
            cover.color = new Color(0.30f, 0.30f, 0.36f);
            cover.preserveAspect = true;
            cover.raycastTarget = false;

            var titleLbl = UiFactory.Label("Title", card, book.title, 28, Color.white);
            var lrt = titleLbl.rectTransform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = new Vector2(1, 0.22f);
            lrt.offsetMin = new Vector2(8, 4); lrt.offsetMax = new Vector2(-8, -4);
            titleLbl.raycastTarget = false;

            ApplyCover(cover, book, catalog);
        }

        async void ApplyCover(Image cover, BookModel book, AssetCatalog catalog)
        {
            if (book.pages == null || book.pages.Count == 0) return;
            var def = catalog.Find(book.pages[0].backgroundId);
            if (def == null) return;

            var sprite = await AssetService.GetSprite(def.url);
            if (sprite != null && cover != null) { cover.sprite = sprite; cover.color = Color.white; }
        }
    }
}
