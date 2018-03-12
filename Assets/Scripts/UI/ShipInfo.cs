using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts.Models;

namespace Assets.Scripts.UI
{
    public class ShipInfo : SelectableImage, IDragHandler, IEndDragHandler
    {
        public delegate void ShipInfoDrag(PointerEventData eventData, ShipInfo draggedShipInfo);
        public event ShipInfoDrag OnShipInfoDrag;
        public delegate void ShipInfoEndDrag(PointerEventData eventData, IEnumerable<ShipInfo> draggedShipInfo);
        public event ShipInfoEndDrag OnShipInfoEndDrag;

        public Text TopLine;
        public Text BottomLine;
        private ShipInfo _dragDupe;
        public Ship Ship;

        private RectTransform _dragRect;

        protected override void Awake()
        {
            base.Awake();
            TopLine = transform.Find("TopLine").GetComponent<Text>();
            BottomLine = transform.Find("BottomLine").GetComponent<Text>();
        }

        public void PopulateShipData(Ship ship)
        {
            Ship = ship;
            TopLine.text = ship.Class.Designation + " " + (ship.HullNumber ?? ship.HullNumber) + " " + (ship.Name ?? "");
            BottomLine.text = ship.IsCrippled ? ship.Class.CrippledFactors : ship.Class.Factors;
        }

        public void PopulateShortData(Ship ship)
        {
            Ship = ship;
            TopLine.text = ship.Class.Designation;
            BottomLine.text = ship.IsCrippled ? ship.Class.CrippledFactors : ship.Class.Factors;
        }

        public void OnDrag(PointerEventData eventData)
        {

            if (eventData == null) { return; }
            if (_dragDupe == null)
            {
                _dragDupe = Instantiate<ShipInfo>(this);
                _dragDupe.transform.SetParent(transform.root, false);
                _dragRect = (RectTransform)_dragDupe.transform;
                _dragRect.position = eventData.position;
                _dragDupe.PopulateShortData(this.Ship);
                var rectTransform = (RectTransform)_dragDupe.transform;
                rectTransform.sizeDelta = new Vector2(30, rectTransform.sizeDelta.y);
            }
            var currentPosition = _dragRect.position;
            currentPosition.x += eventData.delta.x;
            currentPosition.y += eventData.delta.y;
            _dragRect.position = currentPosition;
            if(OnShipInfoDrag != null)
            {
                OnShipInfoDrag(eventData, this);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Destroy(_dragDupe.gameObject);
            _dragDupe = null;
            _dragRect = null;
            if(OnShipInfoEndDrag != null)
            {
                if (CurrentlySelected.Any())
                {
                    OnShipInfoEndDrag(eventData, CurrentlySelected.Select(si => si.GetComponent<ShipInfo>()));
                }
                else
                {
                    OnShipInfoEndDrag(eventData, Enumerable.Repeat(this, 1));
                }
            }
        }
    }
}