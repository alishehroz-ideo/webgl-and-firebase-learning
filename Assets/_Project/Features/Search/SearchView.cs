using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BookLab.Core.Events;
using BookLab.Core.UI;

namespace BookLab.Features.Search
{
    // The View: a search box + button + a scrollable list of result cards.
    // It knows nothing about HOW search works — it publishes SearchRequested when the user searches,
    // and renders whatever SearchResults arrive. (Arabic letter-shaping / RTL is applied in Shape(),
    // a stub for now — the reshaper drops in there next.)
    public class SearchView : MonoBehaviour
    {
        InputField _input;
        Text _status;
        RectTransform _content;

        static readonly Color Bg     = new Color(0.11f, 0.13f, 0.18f);
        static readonly Color CardBg = new Color(1f, 1f, 1f, 0.06f);
        static readonly Color Accent = new Color(0.20f, 0.55f, 0.95f);

        void OnEnable()  => EventBus.Subscribe<SearchResults>(OnResults);
        void OnDisable() => EventBus.Unsubscribe<SearchResults>(OnResults);

        public void Build(Transform parent)
        {
            var bg = UiFactory.Panel("SearchBg", parent, Bg);
            UiFactory.Stretch(bg);

            var title = UiFactory.Label("Title", bg, "Search the Content Library", 40, Color.white, TextAnchor.MiddleLeft);
            Box(title.rectTransform, 60, 30, 1200, 56);

            _input = UiFactory.InputField("Input", bg, "Type a content name or author…", 30);
            Box(_input.GetComponent<RectTransform>(), 60, 100, 1420, 72);
            _input.onEndEdit.AddListener(_ => DoSearch());   // Enter (or focus-out) also searches

            var btn = UiFactory.Button("SearchBtn", bg, "Search", Accent, 30);
            Box(btn.GetComponent<RectTransform>(), 1500, 100, 220, 72);
            btn.onClick.AddListener(DoSearch);

            _status = UiFactory.Label("Status", bg, "", 26, new Color(1, 1, 1, 0.6f), TextAnchor.MiddleLeft);
            Box(_status.rectTransform, 60, 186, 1660, 40);

            BuildScroll(bg);
        }

        void DoSearch() => EventBus.Publish(new SearchRequested(_input != null ? _input.text : ""));

        void BuildScroll(RectTransform parent)
        {
            var scrollGO = new GameObject("Results", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            var scrollRT = (RectTransform)scrollGO.transform;
            scrollRT.SetParent(parent, false);
            scrollRT.anchorMin = Vector2.zero; scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(60, 40); scrollRT.offsetMax = new Vector2(-60, -236);
            scrollGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.15f);

            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewportRT = (RectTransform)viewportGO.transform;
            viewportRT.SetParent(scrollRT, false);
            UiFactory.Stretch(viewportRT);
            viewportGO.GetComponent<Image>().color = new Color(0, 0, 0, 0);   // transparent, still raycastable

            var contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            _content = (RectTransform)contentGO.transform;
            _content.SetParent(viewportRT, false);
            _content.anchorMin = new Vector2(0, 1); _content.anchorMax = new Vector2(1, 1); _content.pivot = new Vector2(0.5f, 1);
            _content.sizeDelta = Vector2.zero; _content.anchoredPosition = Vector2.zero;   // fill viewport width EXACTLY (no left overhang)
            var vlg = contentGO.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 12; vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            contentGO.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGO.GetComponent<ScrollRect>();
            scroll.viewport = viewportRT; scroll.content = _content;
            scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30;
        }

        void OnResults(SearchResults r)
        {
            if (r.Loading) { SetStatus("Loading…"); ClearCards(); return; }
            if (r.Error)   { SetStatus("Couldn't load the library — check the connection and try again."); ClearCards(); return; }

            ClearCards();
            var list = r.Results ?? new List<ParsedContent>();
            foreach (var item in list) AddCard(item);

            if (list.Count == 0)
                SetStatus($"No results for “{r.Query}”");
            else
                SetStatus($"{list.Count} result{(list.Count == 1 ? "" : "s")}" +
                          (string.IsNullOrEmpty(r.Query) ? "" : $" for “{r.Query}”"));
        }

        void AddCard(ParsedContent item)
        {
            var card = UiFactory.Panel("Card", _content, CardBg);
            card.gameObject.AddComponent<LayoutElement>().preferredHeight = 118;

            // Nested layout group => guaranteed left/right padding (no manual anchoring to clip).
            var cvlg = card.gameObject.AddComponent<VerticalLayoutGroup>();
            cvlg.padding = new RectOffset(30, 30, 16, 16);
            cvlg.spacing = 4;
            cvlg.childControlWidth = true;  cvlg.childControlHeight = true;
            cvlg.childForceExpandWidth = true;  cvlg.childForceExpandHeight = false;
            cvlg.childAlignment = TextAnchor.MiddleLeft;

            var name = UiFactory.Label("Name", card, Shape(item.HasName ? item.Name : "(untitled)"),
                                       34, Color.white, TextAnchor.LowerLeft);
            name.gameObject.AddComponent<LayoutElement>().preferredHeight = 44;

            string author = item.HasAuthor ? item.Author : "unknown author";
            string date = string.IsNullOrEmpty(item.Date) ? "" : "   ·   " + item.Date;
            var meta = UiFactory.Label("Meta", card, Shape("by " + author) + date,
                                       24, new Color(1, 1, 1, 0.6f), TextAnchor.UpperLeft);
            meta.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;
        }

        void ClearCards()
        {
            if (_content == null) return;
            for (int i = _content.childCount - 1; i >= 0; i--)
                Destroy(_content.GetChild(i).gameObject);
        }

        void SetStatus(string s) { if (_status != null) _status.text = s; }

        // Arabic shaping/RTL hook — returns input unchanged for now; the reshaper lands here next.
        static string Shape(string s) => s;

        // Absolute top-left placement (canvas is 1920x1080 and scales); y grows downward.
        static void Box(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
        }
    }
}
