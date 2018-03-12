using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class FleetInfo : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public delegate void FleetInfoDrag(PointerEventData eventData);
        public event FleetInfoDrag OnFleetInfoDrag;
        public delegate void FleetInfoEndDrag(PointerEventData eventData);
        public event FleetInfoEndDrag OnFleetInfoEndDrag;
        public delegate void FleetClickAction();
        public event FleetClickAction OnClick;

        public Text FleetNameText;
        private FleetInfo _dragDupe;

        private RectTransform _dragRect;

        private void Awake()
        {
            //FleetNameText = transform.Find("TopLine").GetComponent<Text>();
        }

        public void PopulateFleetName(string fleetName)
        {
            FleetNameText.text = fleetName;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null) { return; }
            if (_dragDupe == null)
            {
                _dragDupe = Instantiate<FleetInfo>(this);
                _dragDupe.transform.SetParent(transform.root, false);
                _dragRect = (RectTransform)_dragDupe.transform;
                _dragRect.position = eventData.position;
                _dragDupe.PopulateFleetName(this.FleetNameText.text);
                var rectTransform = (RectTransform)_dragDupe.transform;
                rectTransform.sizeDelta = new Vector2(30, rectTransform.sizeDelta.y);
            }
            var currentPosition = _dragRect.position;
            currentPosition.x += eventData.delta.x;
            currentPosition.y += eventData.delta.y;
            _dragRect.position = currentPosition;
            if(OnFleetInfoDrag != null)
            {
                OnFleetInfoDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Destroy(_dragDupe.gameObject);
            _dragDupe = null;
            _dragRect = null;
            if(OnFleetInfoEndDrag != null)
            {
                OnFleetInfoEndDrag(eventData);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(OnClick != null)
            {
                OnClick();
            }
        }
    }
}