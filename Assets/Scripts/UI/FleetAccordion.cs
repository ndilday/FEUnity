using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts.Models;

namespace Assets.Scripts.UI
{
    public class FleetAccordion : MonoBehaviour
    {
        public delegate void FleetSelected(FleetAccordion fleetAccordion);
        public event FleetSelected OnFleetSelected;
        public delegate void FleetDragged(PointerEventData eventData, FleetAccordion accordion);
        public event FleetDragged OnFleetDragged;
        public delegate void ShipDragged(PointerEventData eventData, ShipInfo shipInfo, FleetAccordion accordion);
        public event ShipDragged OnShipDragged;
        public delegate void ShipDragEnd(PointerEventData eventData, IEnumerable<ShipInfo> shipInfo, FleetAccordion accordion);
        public event ShipDragEnd OnShipDragEnd;
        public delegate void FleetDragEnd(PointerEventData eventData, FleetAccordion accordion);
        public event FleetDragEnd OnFleetDragEnd;
        public delegate void FleetListEmpty(FleetAccordion accordion);
        public event FleetListEmpty OnShipListEmpty;

        public FleetInfo FleetInfo;
        public FleetShipList FleetShipList;

        public Fleet Fleet { get; private set; }

        public void Awake()
        {
            FleetInfo.OnClick += OnFleetInfoClicked;
            FleetInfo.OnFleetInfoDrag += OnFleetInfoDragged;
            FleetInfo.OnFleetInfoEndDrag += OnFleetInfoEndDrag;
            FleetShipList.OnShipInfoDragged += OnFleetShipInfoDragged;
            FleetShipList.OnShipInfoDragEnd += OnFleetShipInfoDragEnd;
            FleetShipList.OnShipDisplayEmpty += OnFleetShipListEmpty;
        }

        public void PopulateFleetData(Fleet fleet)
        {
            Fleet = fleet;
            FleetInfo.PopulateFleetName(fleet.Name);
            FleetShipList.PopulateShips(fleet.Ships);
            EnableShipList(false);
        }

        public void EnableShipList(bool enable)
        {
            FleetShipList.gameObject.SetActive(enable);
        }

        public void RemoveShipInfo(ShipInfo shipInfo)
        {
            FleetShipList.RemoveShipInfo(shipInfo);
        }

        #region Event Handlers
        public void OnFleetInfoClicked()
        {
            if(OnFleetSelected != null)
            {
                OnFleetSelected(this);
            }
        }

        public void OnFleetShipInfoDragEnd(PointerEventData eventData, IEnumerable<ShipInfo> shipInfo)
        {
            if(OnShipDragEnd != null)
            {
                OnShipDragEnd(eventData, shipInfo, this);
            }
        }

        public void OnFleetShipListEmpty()
        {
            if(OnShipListEmpty != null)
            {
                OnShipListEmpty(this);
            }
            // destroy self
            Destroy(gameObject);
        }

        public void OnFleetShipInfoDragged(PointerEventData eventData, ShipInfo shipInfo)
        {
            if(OnShipDragged != null)
            {
                OnShipDragged(eventData, shipInfo, this);
            }
        }

        public void OnFleetInfoDragged(PointerEventData eventData)
        {
            if(OnFleetDragged != null)
            {
                OnFleetDragged(eventData, this);
            }
        }

        public void OnFleetInfoEndDrag(PointerEventData eventData)
        {
            if(OnFleetDragEnd != null)
            {
                OnFleetDragEnd(eventData, this);
            }
        }
        #endregion
    }
}