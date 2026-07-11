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
    // Create Mode: pick a background, drop/drag stickers, scale + delete a selected
    // sticker, add multiple pages, then Save.
    public class EditorController : MonoBehaviour
    {
        BookModel _book;
        AssetCatalog _catalog;
        RectTransform _root;
        RectTransform _stage;
        Image _background;

        int _pageIndex;
        DraggableSticker _selected;

        GameObject _selToolbar;
        Text _pageLabel;

        PageModel CurrentPage => _book.pages[_pageIndex];

        public async void Show(Transform canvas)
        {
            _book = new BookModel { title = "New Book" };
            _book.pages.Add(new PageModel());

            _root = UiFactory.Panel("Editor", canvas, new Color(0.10f, 0.10f, 0.13f));
            UiFactory.Stretch(_root);

            var stageGO = new GameObject("Stage", typeof(RectTransform));
            _stage = (RectTransform)stageGO.transform;
            _stage.SetParent(_root, false);
            _stage.anchorMin = _stage.anchorMax = new Vector2(0.5f, 0.5f);
            _stage.pivot = new Vector2(0.5f, 0.5f);
            _stage.sizeDelta = new Vector2(1920, 1080);

            _background = UiFactory.Image("Background", _stage);
            UiFactory.Stretch(_background.rectTransform);
            _background.color = new Color(0.18f, 0.18f, 0.22f);
            _background.raycastTarget = false;

            var cancel = UiFactory.Button("Cancel", _root, "← Cancel", new Color(0.40f, 0.30f, 0.30f), 26);
            Place(cancel, new Vector2(0, 1), new Vector2(30, -25), new Vector2(200, 70));
            cancel.onClick.AddListener(() => EventBus.Publish(new GoHomeRequest()));

            var save = UiFactory.Button("Save", _root, "Save ✓", new Color(0.20f, 0.55f, 0.35f), 28);
            Place(save, new Vector2(1, 1), new Vector2(-30, -25), new Vector2(200, 70));
            save.onClick.AddListener(Save);

            BuildSelectionToolbar();
            BuildPageBar();

            _catalog = await ContentApi.GetCatalog();
            BuildBackgroundPicker();
            BuildStickerPicker();
            RenderCurrentPage();
        }

        void Place(Component c, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = (RectTransform)c.transform;
            rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        // ---- selection toolbar (scale / delete) — shown only when a sticker is selected ----
        void BuildSelectionToolbar()
        {
            var bar = UiFactory.Panel("SelToolbar", _root, new Color(0f, 0f, 0f, 0.6f));
            bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 1); bar.pivot = new Vector2(0.5f, 1);
            bar.anchoredPosition = new Vector2(0, -25); bar.sizeDelta = new Vector2(380, 76);
            var hlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10; hlg.padding = new RectOffset(12, 12, 8, 8);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

            var minus = UiFactory.Button("Minus", bar, "−", new Color(0.25f, 0.27f, 0.33f), 34);
            ((RectTransform)minus.transform).sizeDelta = new Vector2(64, 60);
            minus.onClick.AddListener(() => ScaleSelected(0.85f));

            var plus = UiFactory.Button("Plus", bar, "+", new Color(0.25f, 0.27f, 0.33f), 34);
            ((RectTransform)plus.transform).sizeDelta = new Vector2(64, 60);
            plus.onClick.AddListener(() => ScaleSelected(1.18f));

            var del = UiFactory.Button("Delete", bar, "Delete", new Color(0.55f, 0.25f, 0.25f), 24);
            ((RectTransform)del.transform).sizeDelta = new Vector2(150, 60);
            del.onClick.AddListener(DeleteSelected);

            _selToolbar = bar.gameObject;
            _selToolbar.SetActive(false);
        }

        // ---- page bar (nav + add) ----
        void BuildPageBar()
        {
            var bar = new GameObject("PageBar", typeof(RectTransform));
            var brt = (RectTransform)bar.transform; brt.SetParent(_root, false);
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 1); brt.pivot = new Vector2(0.5f, 1);
            brt.anchoredPosition = new Vector2(0, -110); brt.sizeDelta = new Vector2(520, 60);
            var hlg = bar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12; hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

            var prev = UiFactory.Button("PrevPage", brt, "‹", new Color(0.25f, 0.27f, 0.33f), 34);
            ((RectTransform)prev.transform).sizeDelta = new Vector2(60, 56);
            prev.onClick.AddListener(() => GoPage(-1));

            _pageLabel = UiFactory.Label("PageLabel", brt, "Page 1 / 1", 26, Color.white);
            ((RectTransform)_pageLabel.transform).sizeDelta = new Vector2(180, 56);

            var next = UiFactory.Button("NextPage", brt, "›", new Color(0.25f, 0.27f, 0.33f), 34);
            ((RectTransform)next.transform).sizeDelta = new Vector2(60, 56);
            next.onClick.AddListener(() => GoPage(1));

            var add = UiFactory.Button("AddPage", brt, "＋ Page", new Color(0.20f, 0.45f, 0.55f), 24);
            ((RectTransform)add.transform).sizeDelta = new Vector2(160, 56);
            add.onClick.AddListener(AddPage);
        }

        void UpdatePageLabel()
        {
            if (_pageLabel) _pageLabel.text = $"Page {_pageIndex + 1} / {_book.pages.Count}";
        }

        void GoPage(int dir)
        {
            int next = Mathf.Clamp(_pageIndex + dir, 0, _book.pages.Count - 1);
            if (next == _pageIndex) return;
            _pageIndex = next;
            Deselect();
            RenderCurrentPage();
        }

        void AddPage()
        {
            _book.pages.Add(new PageModel());
            _pageIndex = _book.pages.Count - 1;
            Deselect();
            RenderCurrentPage();
        }

        // ---- render the current page (background + its stickers) ----
        void RenderCurrentPage()
        {
            for (int i = _stage.childCount - 1; i >= 0; i--)
            {
                var child = _stage.GetChild(i);
                if (_background == null || child != _background.transform) Destroy(child.gameObject);
            }
            UpdatePageLabel();
            SetBg(CurrentPage.backgroundId);
            foreach (var obj in CurrentPage.objects)
                SpawnSticker(obj);
        }

        // ---- backgrounds (bottom strip) ----
        void BuildBackgroundPicker()
        {
            var strip = UiFactory.Panel("BgPicker", _root, new Color(0f, 0f, 0f, 0.55f));
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
                UiFactory.Stretch(name.rectTransform);
                LoadThumb(thumb, def.url);
                string id = def.id;
                b.onClick.AddListener(() => SelectBackground(id));
            }
        }

        // ---- stickers (left panel) ----
        void BuildStickerPicker()
        {
            var panel = UiFactory.Panel("StickerPicker", _root, new Color(0f, 0f, 0f, 0.55f));
            panel.anchorMin = new Vector2(0, 0); panel.anchorMax = new Vector2(0, 1);
            panel.offsetMin = new Vector2(0, 190);
            panel.offsetMax = new Vector2(160, -180);
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
            CurrentPage.backgroundId = id;
            var def = _catalog.Find(id);
            if (def != null && _book.title == "New Book") _book.title = def.name;   // name after first bg
            SetBg(id);
        }

        async void SetBg(string id)
        {
            if (string.IsNullOrEmpty(id)) { if (_background) { _background.sprite = null; _background.color = new Color(0.18f, 0.18f, 0.22f); } return; }
            var def = _catalog.Find(id);
            if (def == null) return;
            var s = await AssetService.GetSprite(def.url);
            if (s != null && _background != null) { _background.sprite = s; _background.color = Color.white; }
        }

        // ---- sticker placement ----
        void AddSticker(string assetId)
        {
            var model = new PlacedObjectModel { assetId = assetId, x = 0.5f, y = 0.5f, scale = 0.6f, z = CurrentPage.objects.Count };
            CurrentPage.objects.Add(model);
            SpawnSticker(model);
        }

        async void SpawnSticker(PlacedObjectModel model)
        {
            var def = _catalog.Find(model.assetId);
            if (def == null) return;

            var img = UiFactory.Image($"obj_{model.assetId}", _stage);
            img.preserveAspect = true;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(model.x, model.y);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            var drag = img.gameObject.AddComponent<DraggableSticker>();
            drag.Stage = _stage;
            drag.Model = model;
            drag.OnSelected = Select;

            var sprite = await AssetService.GetSprite(def.url);
            if (sprite == null || img == null) return;
            img.sprite = sprite;
            img.SetNativeSize();
            rt.localScale = Vector3.one * (model.scale <= 0 ? 1f : model.scale);
            rt.localRotation = Quaternion.Euler(0, 0, model.rotation);
        }

        // ---- selection + scale + delete ----
        void Select(DraggableSticker d)
        {
            if (_selected == d) return;
            RemoveOutline(_selected);
            _selected = d;
            AddOutline(_selected);
            if (_selToolbar) _selToolbar.SetActive(_selected != null);
        }

        void Deselect()
        {
            RemoveOutline(_selected);
            _selected = null;
            if (_selToolbar) _selToolbar.SetActive(false);
        }

        void AddOutline(DraggableSticker d)
        {
            if (d == null) return;
            var img = d.GetComponent<Image>();
            if (img == null || img.GetComponent<Outline>() != null) return;
            var o = img.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0.30f, 0.80f, 1f, 0.9f);
            o.effectDistance = new Vector2(6, 6);
        }

        void RemoveOutline(DraggableSticker d)
        {
            if (d == null) return;
            var o = d.GetComponent<Outline>();
            if (o) Destroy(o);
        }

        void ScaleSelected(float factor)
        {
            if (_selected == null) return;
            var m = _selected.Model;
            m.scale = Mathf.Clamp(m.scale * factor, 0.15f, 3f);
            ((RectTransform)_selected.transform).localScale = Vector3.one * m.scale;
        }

        void DeleteSelected()
        {
            if (_selected == null) return;
            CurrentPage.objects.Remove(_selected.Model);
            Destroy(_selected.gameObject);
            Deselect();
        }

        async void Save()
        {
            bool ok = await ContentApi.SaveBook(AppConfig.KidId, _book);
            Debug.Log($"[Editor] saved '{_book.title}' ({_book.pages.Count} pages) -> {(ok ? "OK id=" + _book.id : "FAILED")}");
            EventBus.Publish(new GoHomeRequest());
        }
    }
}
