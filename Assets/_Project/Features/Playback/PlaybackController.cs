using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BookLab.Core.UI;
using BookLab.Core.Assets;
using BookLab.Core.Events;
using BookLab.Models;
using BookLab.Services;

namespace BookLab.Features.Playback
{
    // Read-only book viewer: renders the current page (background + placed stickers)
    // and flips pages. This is the same rendering the Editor will reuse.
    public class PlaybackController : MonoBehaviour
    {
        BookModel _book;
        AssetCatalog _catalog;
        int _pageIndex;

        RectTransform _stage;
        Image _background;
        Text _pageLabel;

        public async void Show(Transform canvas, BookModel book)
        {
            _book = book;

            var root = UiFactory.Panel("Playback", canvas, new Color(0.08f, 0.08f, 0.10f));
            UiFactory.Stretch(root);

            // The 1920x1080 stage the page is laid out on (scales with the canvas).
            var stageGO = new GameObject("Stage", typeof(RectTransform));
            _stage = (RectTransform)stageGO.transform;
            _stage.SetParent(root, false);
            _stage.anchorMin = _stage.anchorMax = new Vector2(0.5f, 0.5f);
            _stage.pivot = new Vector2(0.5f, 0.5f);
            _stage.sizeDelta = new Vector2(1920, 1080);

            _background = UiFactory.Image("Background", _stage);
            UiFactory.Stretch(_background.rectTransform);
            _background.color = new Color(0.20f, 0.20f, 0.25f);
            _background.raycastTarget = false;

            var back = UiFactory.Button("Back", root, "< Back", new Color(0.30f, 0.30f, 0.36f), 28);
            var brt = (RectTransform)back.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(0, 1); brt.pivot = new Vector2(0, 1);
            brt.anchoredPosition = new Vector2(30, -30); brt.sizeDelta = new Vector2(190, 70);
            back.onClick.AddListener(() => EventBus.Publish(new GoHomeRequest()));

            var prev = UiFactory.Button("Prev", root, "<", new Color(0.25f, 0.27f, 0.33f), 54);
            var prt = (RectTransform)prev.transform;
            prt.anchorMin = prt.anchorMax = new Vector2(0, 0.5f); prt.pivot = new Vector2(0, 0.5f);
            prt.anchoredPosition = new Vector2(20, 0); prt.sizeDelta = new Vector2(90, 150);
            prev.onClick.AddListener(() => Turn(-1));

            var next = UiFactory.Button("Next", root, ">", new Color(0.25f, 0.27f, 0.33f), 54);
            var nrt = (RectTransform)next.transform;
            nrt.anchorMin = nrt.anchorMax = new Vector2(1, 0.5f); nrt.pivot = new Vector2(1, 0.5f);
            nrt.anchoredPosition = new Vector2(-20, 0); nrt.sizeDelta = new Vector2(90, 150);
            next.onClick.AddListener(() => Turn(1));

            _pageLabel = UiFactory.Label("PageLabel", root, "", 30, Color.white);
            var plrt = _pageLabel.rectTransform;
            plrt.anchorMin = plrt.anchorMax = new Vector2(0.5f, 0); plrt.pivot = new Vector2(0.5f, 0);
            plrt.anchoredPosition = new Vector2(0, 25); plrt.sizeDelta = new Vector2(300, 50);

            _catalog = await ContentApi.GetCatalog();
            RenderPage();
        }

        void Turn(int dir)
        {
            if (_book?.pages == null || _book.pages.Count == 0) return;
            _pageIndex = Mathf.Clamp(_pageIndex + dir, 0, _book.pages.Count - 1);
            RenderPage();
        }

        void RenderPage()
        {
            // clear old stickers (keep the background)
            for (int i = _stage.childCount - 1; i >= 0; i--)
            {
                var child = _stage.GetChild(i);
                if (_background == null || child != _background.transform) Destroy(child.gameObject);
            }

            if (_book?.pages == null || _book.pages.Count == 0)
            {
                if (_pageLabel) _pageLabel.text = "(empty book)";
                return;
            }

            var page = _book.pages[_pageIndex];
            if (_pageLabel) _pageLabel.text = $"{_pageIndex + 1} / {_book.pages.Count}";

            SetBackground(page.backgroundId);

            if (page.objects != null)
            {
                var sorted = new List<PlacedObjectModel>(page.objects);
                sorted.Sort((a, b) => a.z.CompareTo(b.z));   // lower z first → higher z drawn on top
                foreach (var obj in sorted)
                    PlaceObject(obj);
            }
        }

        async void SetBackground(string bgId)
        {
            var def = _catalog.Find(bgId);
            if (def == null) return;
            var sprite = await AssetService.GetSprite(def.url);
            if (sprite != null && _background != null) { _background.sprite = sprite; _background.color = Color.white; }
        }

        async void PlaceObject(PlacedObjectModel obj)
        {
            var def = _catalog.Find(obj.assetId);
            if (def == null) return;

            var img = UiFactory.Image($"obj_{obj.assetId}", _stage);
            img.raycastTarget = false;
            img.preserveAspect = true;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(obj.x, obj.y);   // normalized position on the stage
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            var sprite = await AssetService.GetSprite(def.url);
            if (sprite == null || img == null) return;
            img.sprite = sprite;
            img.SetNativeSize();
            rt.localScale = Vector3.one * (obj.scale <= 0 ? 1f : obj.scale);
            rt.localRotation = Quaternion.Euler(0, 0, obj.rotation);
        }
    }
}
