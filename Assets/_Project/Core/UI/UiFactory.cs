using UnityEngine;
using UnityEngine.UI;

namespace BookLab.Core.UI
{
    // Tiny helpers to build uGUI elements in code (keeps the screen controllers readable).
    // Uses Unity's built-in font so nothing needs importing.
    public static class UiFactory
    {
        static Font _font;
        static Font Font => _font != null ? _font : (_font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        public static RectTransform Panel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return rt;
        }

        public static Image Image(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return go.GetComponent<Image>();
        }

        public static Text Label(string name, Transform parent, string text, int size, Color color,
                                 TextAnchor align = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = text; t.font = Font; t.fontSize = size; t.color = color; t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;   // labels never need clicks — never let them block buttons behind them
            return t;
        }

        public static Button Button(string name, Transform parent, string label, Color bg, int fontSize = 28)
        {
            var rt = Panel(name, parent, bg);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = rt.GetComponent<Image>();
            if (!string.IsNullOrEmpty(label))
            {
                var t = Label("Label", rt, label, fontSize, Color.white);
                Stretch(t.rectTransform);
                t.raycastTarget = false;   // let the click reach the button, not the text
            }
            return btn;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }
    }
}
