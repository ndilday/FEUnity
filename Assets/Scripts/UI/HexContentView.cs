using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Models;

namespace Assets.Scripts.UI
{
    public class HexContentView : MonoBehaviour
    {
        public GameObject ShipInfoPrefab;
        private List<Ship> ShipList;

        public void PopulateShips(IEnumerable<Ship> ships)
        {
            ShipList = new List<Ship>();
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach (Ship ship in ships)
            {
                AddShip(ship);
            }
        }

        public void AddShip(Ship ship)
        {
            GameObject shipInfo = Instantiate(ShipInfoPrefab);
            shipInfo.transform.SetParent(transform, false);
            var rectTransform = ((RectTransform)(shipInfo.transform));
            rectTransform.sizeDelta = new Vector2(30, rectTransform.sizeDelta.y);
            ShipInfo info = shipInfo.GetComponent<ShipInfo>();
            info.PopulateShortData(ship);
            ShipList.Add(ship);
        }
    }
}
