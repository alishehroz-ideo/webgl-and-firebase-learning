using UnityEngine;
using UnityEngine.EventSystems;
using BookLab.Models;

namespace BookLab.Features.BookEditor
{
    // Makes a placed sticker draggable within the 1920x1080 stage, keeping its
    // model's NORMALIZED (0..1) position in sync as you move it.
    public class DraggableSticker : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public PlacedObjectModel Model;
        public RectTransform Stage;
        public System.Action<DraggableSticker> OnSelected;

        RectTransform _rt;
        void Awake() { _rt = (RectTransform)transform; }

        public void OnPointerDown(PointerEventData e) => OnSelected?.Invoke(this);

        public void OnDrag(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    Stage, e.position, e.pressEventCamera, out var local))
                return;

            // stage pivot is centered, so local ranges [-w/2, w/2] × [-h/2, h/2] → normalize to 0..1
            float nx = Mathf.Clamp01((local.x + Stage.rect.width  * 0.5f) / Stage.rect.width);
            float ny = Mathf.Clamp01((local.y + Stage.rect.height * 0.5f) / Stage.rect.height);

            _rt.anchorMin = _rt.anchorMax = new Vector2(nx, ny);
            _rt.anchoredPosition = Vector2.zero;

            if (Model != null) { Model.x = nx; Model.y = ny; }
        }
    }
}
