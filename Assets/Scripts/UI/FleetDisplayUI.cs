using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts.Models;

namespace Assets.Scripts.UI
{
    public class FleetDisplayUI : MonoBehaviour
    {
        public delegate void ShipInfoDragged(PointerEventData eventData, ShipInfo shipInfo, FleetAccordion accordion);
        public event ShipInfoDragged OnShipInfoDragged;
        public delegate void ShipInfoDragEnd(PointerEventData eventData, IEnumerable<ShipInfo> shipInfo, FleetAccordion accordion);
        public event ShipInfoDragEnd OnShipInfoDragEnd;
        //public delegate void FleetInfoDragged(PointerEventData eventData, FleetAccordion accordion);
        //public event FleetInfoDragged OnFleetInfoDragged;
        //public delegate void FleetInfoDragEnd(PointerEventData eventData, FleetAccordion accordion);
        //public event FleetInfoDragEnd OnFleetInfoDragEnd;
        public delegate void FleetDisplayEmptyAction();
        public event FleetDisplayEmptyAction OnFleetDisplayEmpty;
        public delegate void FleetSelected(Fleet fleet);
        public event FleetSelected OnFleetSelected;

        public GameObject FleetAccordionPrefab;

        private List<FleetAccordion> _fleetAccordionList;
        private RectTransform _rectTransform;

        private void Start()
        {
            _rectTransform = (RectTransform)gameObject.transform;
        }

        public void PopulateFleets(IEnumerable<Fleet> fleets)
        {
            _fleetAccordionList = new List<FleetAccordion>();
            foreach (Fleet fleet in fleets)
            {
                if (fleet.Ships != null && fleet.Ships.Any())
                {
                    GameObject fleetAccordionObject = Instantiate(FleetAccordionPrefab);
                    FleetAccordion fleetAccordion = fleetAccordionObject.GetComponent<FleetAccordion>();
                    _fleetAccordionList.Add(fleetAccordion);
                    fleetAccordion.OnShipDragged += OnFleetShipInfoDragged;
                    fleetAccordion.OnShipDragEnd += OnFleetShipInfoDragEnd;
                    fleetAccordion.OnShipListEmpty += OnFleetShipListEmpty;
                    fleetAccordion.OnFleetSelected += OnFleetAccordionSelected;
                    fleetAccordion.transform.SetParent(transform, false);
                    fleetAccordion.PopulateFleetData(fleet);
                }
            }
        }

        public void RemoveShipInfo(FleetAccordion fleetAccordion, ShipInfo shipInfo)
        {
            foreach(FleetAccordion accordion in _fleetAccordionList)
            {
                if(accordion == fleetAccordion)
                {
                    accordion.RemoveShipInfo(shipInfo);
                    break;
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }

        #region Event Handlers
        public void OnFleetAccordionSelected(FleetAccordion fleetAccordion)
        {
            foreach(FleetAccordion accordion in _fleetAccordionList)
            {
                accordion.EnableShipList(accordion == fleetAccordion);
            }
            if(OnFleetSelected != null)
            {
                OnFleetSelected(fleetAccordion.Fleet);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
        }

        public void OnFleetShipInfoDragged(PointerEventData eventData, ShipInfo shipInfo, FleetAccordion accordion)
        {
            if(OnShipInfoDragged != null)
            {
                OnShipInfoDragged(eventData, shipInfo, accordion);
            }
        }

        public void OnFleetShipInfoDragEnd(PointerEventData eventData, IEnumerable<ShipInfo> shipInfo, FleetAccordion accordion)
        {
            if(OnShipInfoDragEnd != null)
            {
                OnShipInfoDragEnd(eventData, shipInfo, accordion);
            }
        }

        public void OnFleetShipListEmpty(FleetAccordion accordion)
        {
            // remove the fleet accordion from the list
            _fleetAccordionList.Remove(accordion);
            // if this was the last fleet available, 
            if(gameObject.transform.childCount == 1 && OnFleetDisplayEmpty != null)
            {
                OnFleetDisplayEmpty();
            }
        }
        #endregion
    }
}