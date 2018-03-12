using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts.Models;

namespace Assets.Scripts.UI
{
    public class FleetShipList : MonoBehaviour
    {
        public delegate void ShipInfoDrag(PointerEventData eventData, ShipInfo shipInfo);
        public event ShipInfoDrag OnShipInfoDragged;
        public delegate void ShipInfoEndDrag(PointerEventData eventData, IEnumerable<ShipInfo> shipInfo);
        public event ShipInfoEndDrag OnShipInfoDragEnd;
        public delegate void ShipDisplayAreaEmpty();
        public event ShipDisplayAreaEmpty OnShipDisplayEmpty;

        public GameObject ShipInfoPrefab;
        public Renderer Renderer { get; private set; }

        private List<ShipInfo> _shipInfoList;
        private RectTransform _rectTransform;

        private void Awake()
        {
            Renderer = gameObject.GetComponent<Renderer>();
        }

        private void Start()
        {
            _rectTransform = (RectTransform)gameObject.transform;
        }

        public void PopulateShips(IEnumerable<Ship> ships)
        {
            _shipInfoList = new List<ShipInfo>();
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach (Ship ship in ships)
            {
                GameObject shipInfo = Instantiate(ShipInfoPrefab);
                shipInfo.transform.SetParent(transform, false);
                ShipInfo info = shipInfo.GetComponent<ShipInfo>();
                _shipInfoList.Add(info);
                info.PopulateShipData(ship);
                info.OnShipInfoEndDrag += OnShipInfoEndDrag;
                info.OnShipInfoDrag += OnShipInfoDrag;
            }
        }

        public void RemoveShipInfo(ShipInfo shipInfo)
        {
            _shipInfoList.Remove(shipInfo);
            foreach (Transform child in transform)
            {
                if (child == shipInfo.transform)
                {
                    GameObject.Destroy(child.gameObject);
                    break;
                }
            }
            // because childCount isn't updated until the end of the frame, 
            // a child count of 1 means we just emptied the list
            if (_shipInfoList.Count == 0 && OnShipDisplayEmpty != null)
            {
                OnShipDisplayEmpty();
            }
            else
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
            }
        }

        private void OnShipInfoDrag(PointerEventData eventData, ShipInfo shipInfo)
        {
            if(OnShipInfoDragged != null)
            {
                OnShipInfoDragged(eventData, shipInfo);
            }
        }

        private void OnShipInfoEndDrag(PointerEventData eventData, IEnumerable<ShipInfo> shipInfo)
        {
            if (OnShipInfoDragEnd != null)
            {
                OnShipInfoDragEnd(eventData, shipInfo);
            }
        }
    }
}