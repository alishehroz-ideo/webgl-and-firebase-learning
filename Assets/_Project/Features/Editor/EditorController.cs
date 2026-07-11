using UnityEngine;
using UnityEngine.UI;
using BookLab.Core.UI;
using BookLab.Core.Assets;
using BookLab.Core.Events;
using BookLab.Models;
using BookLab.Services;
using BookLab.App;

namespace BookLab.Features.Editor
{
    // Create Mode — PART 1: pick a background, then Save.
    // (Stickers + drag/scale + multiple pages come in the next slices.)
    public class EditorController : MonoBehaviour
    {
        BookModel _book;
        AssetCatalog _catalog;
        RectTransform _stage;
        Image _background;

        public async void Show(Transform canvas)
        {
            _book = new BookModel { title = "New Book" };
            _book.pages.Add(new PageModel());   // start with one empty page

            var root = UiFactory.Panel("Editor", canvas, new Color(0.10f, 0.10f, 0.13f));
            UiFactory.Stretch(root);

            // The 1920x1080 stage (same as Playback).
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

            // Cancel (top-left) — discard, back to shelf
            var cancel = UiFactory.Button("Cancel", root, "← Cancel", new Color(0.40f, 0.30f, 0.30f), 26);
            var xrt = (RectTransform)cancel.transform;
            xrt.anchorMin = xrt.anchorMax = new Vector2(0, 1); xrt.pivot = new Vector2(0, 1);
            xrt.anchoredPosition = new Vector2(30, -30); xrt.sizeDelta = new Vector2(200, 70);
            cancel.onClick.AddListener(() => EventBus.Publish(new GoHomeRequest()));

            // Save (top-right)
            var save = UiFactory.Button("Save", root, "Save ✓", new Color(0.20f, 0.55f, 0.35f), 28);
            var srt = (RectTransform)save.transform;
            srt.anchorMin = srt.anchorMax = new Vector2(1, 1); srt.pivot = new Vector2(1, 1);
            srt.anchoredPosition = new Vector2(-30, -30); srt.sizeDelta = new Vector2(200, 70);
            save.onClick.AddListener(Save);

            var prompt = UiFactory.Label("Prompt", root, "Pick a background  ↓", 28, new Color(0.8f, 0.85f, 1f));
            var prt = prompt.rectTransform;
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 1); prt.pivot = new Vector2(0.5f, 1);
            prt.anchoredPosition = new Vector2(0, -30); prt.sizeDelta = new Vector2(600, 60);

            _catalog = await ContentApi.GetCatalog();
            BuildBackgroundPicker(root);
        }

        void BuildBackgroundPicker(RectTransform root)
        {
            var strip = UiFactory.Panel("BgPicker", root, new Color(0f, 0f, 0f, 0.55f));
            strip.anchorMin = new Vector2(0, 0); strip.anchorMax = new Vector2(1, 0); strip.pivot = new Vector2(0.5f, 0);
            strip.anchoredPosition = Vector2.zero; strip.sizeDelta = new Vector2(0, 190);

            var hlg = strip.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20; hlg.padding = new RectOffset(24, 24, 24, 24);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;

            foreach (var kv in _catalog.backgrounds)
            {
                var def = kv.Value;
                var b = UiFactory.Button($"bg_{def.id}", strip, "", new Color(0.25f, 0.25f, 0.30f));
                ((RectTransform)b.transform).sizeDelta = new Vector2(220, 140);

                var thumb = UiFactory.Image("thumb", b.transform);
                UiFactory.Stretch(thumb.rectTransform);
                thumb.raycastTarget = false; thumb.preserveAspect = true;
                thumb.color = new Color(0.4f, 0.4f, 0.45f);

                var name = UiFactory.Label("name", b.transform, def.name, 22, Color.white, TextAnchor.LowerCenter);
                UiFactory.Stretch(name.rectTransform); name.raycastTarget = false;

                LoadThumb(thumb, def.url);
                string id = def.id;
                b.onClick.AddListener(() => SelectBackground(id));
            }
        }

        async void LoadThumb(Image img, string url)
        {
            var s = await AssetService.GetSprite(url);
            if (s != null && img != null) { img.sprite = s; img.color = Color.white; }
        }

        void SelectBackground(string id)
        {
            _book.pages[0].backgroundId = id;
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

        async void Save()
        {
            bool ok = await ContentApi.SaveBook(AppConfig.KidId, _book);
            Debug.Log($"[Editor] saved '{_book.title}' -> {(ok ? "OK id=" + _book.id : "FAILED")}");
            EventBus.Publish(new GoHomeRequest());
        }
    }
}
