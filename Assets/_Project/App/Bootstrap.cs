using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using BookLab.Features.MainMenu;

namespace BookLab.App
{
    // Entry point. Add this to an empty GameObject in a scene and press Play.
    // Builds a Full-HD canvas that scales to any screen, then shows the Main Menu.
    public class Bootstrap : MonoBehaviour
    {
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
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;   // <- "scales on different screens"
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var menu = canvasGO.AddComponent<MainMenuController>();
            menu.Show(canvasGO.transform);
        }
    }
}
