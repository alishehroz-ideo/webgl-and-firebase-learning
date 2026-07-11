using UnityEngine;
using BookLab.Core.Events;
using BookLab.Features.MainMenu;
using BookLab.Features.Playback;

namespace BookLab.App
{
    // Owns the single canvas and swaps the active screen in response to nav events.
    // Screens never reference each other — they just publish events; AppRoot listens.
    public class AppRoot : MonoBehaviour
    {
        Transform _canvas;
        GameObject _current;

        public void Begin(Transform canvas)
        {
            _canvas = canvas;
            EventBus.Subscribe<OpenBookRequest>(OnOpenBook);
            EventBus.Subscribe<GoHomeRequest>(OnGoHome);
            EventBus.Subscribe<CreateBookRequest>(OnCreate);
            ShowMainMenu();
        }

        void OnDestroy()
        {
            EventBus.Unsubscribe<OpenBookRequest>(OnOpenBook);
            EventBus.Unsubscribe<GoHomeRequest>(OnGoHome);
            EventBus.Unsubscribe<CreateBookRequest>(OnCreate);
        }

        // A full-screen container for one screen.
        RectTransform NewScreen(string name)
        {
            if (_current) Destroy(_current);
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(_canvas, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            _current = go;
            return rt;
        }

        void ShowMainMenu()
        {
            var screen = NewScreen("MainMenuScreen");
            screen.gameObject.AddComponent<MainMenuController>().Show(screen);
        }

        void OnOpenBook(OpenBookRequest req)
        {
            var screen = NewScreen("PlaybackScreen");
            screen.gameObject.AddComponent<PlaybackController>().Show(screen, req.Book);
        }

        void OnGoHome(GoHomeRequest _) => ShowMainMenu();

        void OnCreate(CreateBookRequest _)
        {
            Debug.Log("[AppRoot] Create requested — Editor screen coming in a later slice.");
        }
    }
}
