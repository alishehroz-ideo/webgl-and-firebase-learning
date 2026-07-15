using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using BookLab.Features.Search;
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

            // --- Task 2 Search screen (MVC + event-driven, reusing the Core) ---
            var view = canvasGO.AddComponent<SearchView>();
            view.Build(canvasGO.transform);           // View: box + button + result cards
            canvasGO.AddComponent<SearchController>(); // Controller: loads data, answers searches
            canvasGO.AddComponent<FontRebuildFix>();   // keep dynamic-font text from blanking on atlas rebuild
        }
    }
}
