using UnityEngine;
using UnityEngine.UI;
using BookLab.Core.UI;
using BookLab.Core.Assets;
using BookLab.Core.Events;
using BookLab.Models;
using BookLab.Services;
using BookLab.App;

namespace BookLab.Features.BookEditor
{
    // Create Mode — pick a background, drop stickers, drag them, then Save.
    // (Scale, delete, and multiple pages come in the next slice.)
    public class EditorController : MonoBehaviour
    {
        BookModel _book;
        AssetCatalog _catalog;
        RectTransform _stage;
        Image _background;
        DraggableSticker _selected;

        PageModel Page => _book.pages[0];

        public async void Show(Transform canvas)
        {
            _book = new BookModel { title = "New Book" };
            _book.pages.Add(new PageModel());

            var root = UiFactory.Panel("Editor", canvas, new Color(0.10f, 0.10f, 0.13f));
            UiFactory.Stretch(root);

            var stageGO = new GameObject("Stage", typeof(RectTransform));
            _stage = (RectTransform)stageGO.transform;
            _stage.SetParent(root, false);
            _stage.anchorMin = _stage.anchorMax = new Vector2(0.5f, 0.5f);
            _stage.pivot = new Vector2(0.5f, 0.5f);
            _stage.sizeDelta = new Vector2(1920, 1080);

            _background = UiFactory.Image("Background", _stage);
            UiFactory.Stretch(_background.rectTransform);
            _background.color = new Color(0.18f, 0.18f, 0.22f);
            _background.raycastTarget = false;

            var cancel = UiFactory.Button("Cancel", root, "← Cancel", new Color(0.40f, 0.30f, 0.30f), 26);
            var xrt = (RectTransform)cancel.transform;
            xrt.anchorMin = xrt.anchorMax = new Vector2(0, 1); xrt.pivot = new Vector2(0, 1);
            xrt.anchoredPosition = new Vector2(30, -25); xrt.sizeDelta = new Vector2(200, 70);
            cancel.onClick.AddListener(() => EventBus.Publish(new GoHomeRequest()));

            var save = UiFactory.Button("Save", root, "Save ✓", new Color(0.20f, 0.55f, 0.35f), 28);
            var srt = (RectTransform)save.transform;
            srt.anchorMin = srt.anchorMax = new Vector2(1, 1); srt.pivot = new Vector2(1, 1);
            srt.anchoredPosition = new Vector2(-30, -25); srt.sizeDelta = new Vector2(200, 70);
            save.onClick.AddListener(Save);

            var prompt = UiFactory.Label("Prompt", root, "Pick a background ↓   •   tap a sticker ←   •   drag to move", 24, new Color(0.8f, 0.85f, 1f));
            var prt = prompt.rectTransform;
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 1); prt.pivot = new Vector2(0.5f, 1);
            prt.anchoredPosition = new Vector2(0, -30); prt.sizeDelta = new Vector2(1000, 50);

            _catalog = await ContentApi.GetCatalog();
            BuildBackgroundPicker(root);
            BuildStickerPicker(root);
        }

        // ---- Backgrounds (bottom strip) ----
        void BuildBackgroundPicker(RectTransform root)
        {
            var strip = UiFactory.Panel("BgPicker", root, new Color(0f, 0f, 0f, 0.55f));
            strip.anchorMin = new Vector2(0, 0); strip.anchorMax = new Vector2(1, 0); strip.pivot = new Vector2(0.5f, 0);
            strip.anchoredPosition = Vector2.zero; strip.sizeDelta = new Vector2(0, 180);

            var hlg = strip.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20; hlg.padding = new RectOffset(200, 24, 20, 20);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

            foreach (var kv in _catalog.backgrounds)
            {
                var def = kv.Value;
                var b = UiFactory.Button($"bg_{def.id}", strip, "", new Color(0.25f, 0.25f, 0.30f));
                ((RectTransform)b.transform).sizeDelta = new Vector2(200, 130);
                var thumb = UiFactory.Image("thumb", b.transform);
                UiFactory.Stretch(thumb.rectTransform); thumb.raycastTarget = false; thumb.preserveAspect = true;
                thumb.color = new Color(0.4f, 0.4f, 0.45f);
                var name = UiFactory.Label("name", b.transform, def.name, 20, Color.white, TextAnchor.LowerCenter);
                UiFactory.Stretch(name.rectTransform); name.raycastTarget = false;
                LoadThumb(thumb, def.url);
                string id = def.id;
                b.onClick.AddListener(() => SelectBackground(id));
            }
        }

        // ---- Stickers (left panel) ----
        void BuildStickerPicker(RectTransform root)
        {
            var panel = UiFactory.Panel("StickerPicker", root, new Color(0f, 0f, 0f, 0.55f));
            panel.anchorMin = new Vector2(0, 0); panel.anchorMax = new Vector2(0, 1);
            panel.offsetMin = new Vector2(0, 190);    // sits above the background strip
            panel.offsetMax = new Vector2(160, -90);  // sits below the top bar

            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14; vlg.padding = new RectOffset(16, 16, 16, 16);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = false; vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false; vlg.childForceExpandHeight = false;

            foreach (var kv in _catalog.objects)
            {
                var def = kv.Value;
                var b = UiFactory.Button($"st_{def.id}", panel, "", new Color(0.25f, 0.25f, 0.30f));
                ((RectTransform)b.transform).sizeDelta = new Vector2(120, 120);
                var thumb = UiFactory.Image("thumb", b.transform);
                UiFactory.Stretch(thumb.rectTransform); thumb.raycastTarget = false; thumb.preserveAspect = true;
                thumb.color = new Color(0.4f, 0.4f, 0.45f);
                LoadThumb(thumb, def.url);
                string id = def.id;
                b.onClick.AddListener(() => AddSticker(id));
            }
        }

        async void LoadThumb(Image img, string url)
        {
            var s = await AssetService.GetSprite(url);
            if (s != null && img != null) { img.sprite = s; img.color = Color.white; }
        }

        void SelectBackground(string id)
        {
            Page.backgroundId = id;
            var def = _catalog.Find(id);
            if (def != null) _book.title = def.name;   // name the book after its background (demo)
            SetBg(id);
        }

        async void SetBg(string id)
        {
            var def = _catalog.Find(id);
            if (def == null) return;
            var s = await AssetService.GetSprite(def.url);
            if (s != null && _background != null) { _background.sprite = s; _background.color = Color.white; }
        }

        // ---- Sticker placement ----
        void AddSticker(string assetId)
        {
            var model = new PlacedObjectModel { assetId = assetId, x = 0.5f, y = 0.5f, scale = 0.6f, z = Page.objects.Count };
            Page.objects.Add(model);
            SpawnSticker(model);
        }

        async void SpawnSticker(PlacedObjectModel model)
        {
            var def = _catalog.Find(model.assetId);
            if (def == null) return;

            var img = UiFactory.Image($"obj_{model.assetId}", _stage);
            img.preserveAspect = true;   // raycastTarget stays true so it can be dragged
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(model.x, model.y);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            var drag = img.gameObject.AddComponent<DraggableSticker>();
            drag.Stage = _stage;
            drag.Model = model;
            drag.OnSelected = d => _selected = d;

            var sprite = await AssetService.GetSprite(def.url);
            if (sprite == null || img == null) return;
            img.sprite = sprite;
            img.SetNativeSize();
            rt.localScale = Vector3.one * (model.scale <= 0 ? 1f : model.scale);
            rt.localRotation = Quaternion.Euler(0, 0, model.rotation);
        }

        async void Save()
        {
            bool ok = await ContentApi.SaveBook(AppConfig.KidId, _book);
            Debug.Log($"[Editor] saved '{_book.title}' -> {(ok ? "OK id=" + _book.id : "FAILED")}");
            EventBus.Publish(new GoHomeRequest());
        }
    }
}
