using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace BookLab.App
{
    // Entry point. Add this to an empty GameObject in a scene and press Play.
    // Builds a Full-HD canvas that scales to any screen, then starts the app.
    public class Bootstrap : MonoBehaviour
    {
        // Auto-start ONLY in the Task 1 scene ("SampleScene"). Every script compiles into EVERY
        // build, so without this scene guard this would also fire in the Task 2 build and hijack it.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (SceneManager.GetActiveScene().name != "SampleScene") return;
            if (FindFirstObjectByType<Bootstrap>() != null) return;   // a manual one is already in the scene
            new GameObject("BookLabApp").AddComponent<Bootstrap>();
        }

        void Start()
        {
            // Ensure an EventSystem exists so buttons receive clicks.
            // This project uses the new Input System, so use its UI input module.
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
                es.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            var canvasGO = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;   // <- scales on different screens
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var app = canvasGO.AddComponent<AppRoot>();
            app.Begin(canvasGO.transform);
        }
    }
}
