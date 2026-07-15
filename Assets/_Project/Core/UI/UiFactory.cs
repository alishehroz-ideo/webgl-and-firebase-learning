using UnityEngine;
using UnityEngine.UI;

namespace BookLab.Core.UI
{
    // Tiny helpers to build uGUI elements in code (keeps the screen controllers readable).
    // Uses Unity's built-in font so nothing needs importing.
    public static class UiFactory
    {
        static Font _font;
        static Font Font
        {
            get
            {
                if (_font != null) return _font;
                // WebGL has no OS font fallback, so the built-in font (Liberation Sans — no Arabic)
                // renders Arabic blank in a build. Amiri covers Latin + Arabic presentation forms
                // (which the reshaper outputs). Falls back to the built-in font if not present.
                _font = Resources.Load<Font>("Fonts/Amiri-Regular");
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

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

        public static InputField InputField(string name, Transform parent, string placeholder, int size)
        {
            var rt = Panel(name, parent, new Color(1f, 1f, 1f, 0.92f));
            var input = rt.gameObject.AddComponent<InputField>();
            input.targetGraphic = rt.GetComponent<Image>();

            var text = Label("Text", rt, "", size, new Color(0.1f, 0.1f, 0.1f), TextAnchor.MiddleLeft);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(14, 0); text.rectTransform.offsetMax = new Vector2(-14, 0);
            text.supportRichText = false;

            var ph = Label("Placeholder", rt, placeholder, size, new Color(0.1f, 0.1f, 0.1f, 0.4f), TextAnchor.MiddleLeft);
            ph.horizontalOverflow = HorizontalWrapMode.Overflow;
            ph.rectTransform.anchorMin = Vector2.zero; ph.rectTransform.anchorMax = Vector2.one;
            ph.rectTransform.offsetMin = new Vector2(14, 0); ph.rectTransform.offsetMax = new Vector2(-14, 0);

            input.textComponent = text;
            input.placeholder = ph;
            return input;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }
    }
}
