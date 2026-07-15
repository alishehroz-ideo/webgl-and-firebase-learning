using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using BookLab.Core.UI;

namespace BookLab.App
{
    // Entry point for Task 2 (Search & Content Discovery).
    // Lives in its OWN scene ("task2") and its own WebGL build, but reuses the shared Core
    // (UiFactory, FirebaseClient, EventBus, ...). Right now it just paints a placeholder screen;
    // the Search UI + parser drop in here next.
    public class Task2Bootstrap : MonoBehaviour
    {
        // Auto-start ONLY in the Task 2 scene, so it never collides with the Task 1 build.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (SceneManager.GetActiveScene().name != "task2") return;
            if (FindFirstObjectByType<Task2Bootstrap>() != null) return;
            new GameObject("SearchApp").AddComponent<Task2Bootstrap>();
        }

        void Start()
        {
            // Ensure an EventSystem (new Input System module) so buttons receive clicks.
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
                es.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            // Full-HD canvas that scales to any screen (same setup as Task 1).
            var canvasGO = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // --- Placeholder screen (replaced by the real Search screen next) ---
            var bg = UiFactory.Panel("Background", canvasGO.transform, new Color(0.11f, 0.13f, 0.18f));
            UiFactory.Stretch(bg);

            var title = UiFactory.Label("Title", bg, "Task 2 · Search & Content Discovery",
                                        64, Color.white, TextAnchor.MiddleCenter);
            title.rectTransform.anchorMin = title.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            title.rectTransform.sizeDelta = new Vector2(1500, 120);
            title.rectTransform.anchoredPosition = new Vector2(0, 40);

            var sub = UiFactory.Label("Subtitle", bg, "skeleton ready — parser & search coming next",
                                      30, new Color(1, 1, 1, 0.6f), TextAnchor.MiddleCenter);
            sub.rectTransform.anchorMin = sub.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            sub.rectTransform.sizeDelta = new Vector2(1500, 60);
            sub.rectTransform.anchoredPosition = new Vector2(0, -60);
        }
    }
}
