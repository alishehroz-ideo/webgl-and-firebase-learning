using UnityEngine;
using UnityEngine.UI;

namespace BookLab.Core.UI
{
    // Legacy UI Text with a DYNAMIC font (our bundled Amiri) can render BLANK when the font's atlas
    // texture rebuilds — which happens as new glyphs (e.g. lots of Arabic) are requested. Unity does
    // not always re-render existing Text after that rebuild, so text that appeared a moment later
    // (like result cards) goes blank. Fix: on every font-texture rebuild, mark all Text dirty so
    // they re-render against the new atlas.
    public class FontRebuildFix : MonoBehaviour
    {
        void OnEnable()  => Font.textureRebuilt += OnRebuilt;
        void OnDisable() => Font.textureRebuilt -= OnRebuilt;

        static void OnRebuilt(Font f)
        {
            var texts = FindObjectsByType<Text>(FindObjectsSortMode.None);
            foreach (var t in texts)
                if (t.font == f) t.SetAllDirty();
        }
    }
}
