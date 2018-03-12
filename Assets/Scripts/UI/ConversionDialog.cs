using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Econ;
using Assets.Scripts.Models;

namespace Assets.Scripts.UI
{
    public class ConversionDialog : MonoBehaviour
    {
        public GameObject ConversionUIPrefab;
        public GameObject ContentPane;
        public Text EPTotal;
        private List<ConversionUI> _conversionEntries;
        private Shipyard _shipyard;

        public void PopulateDropdowns(Shipyard shipyard, List<MapNode> mapNodes, int turn)
        {
            _shipyard = shipyard;
            _conversionEntries = new List<ConversionUI>();
            foreach (Transform child in ContentPane.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            float totalCost = 0;
            foreach (MapNode mapNode in mapNodes)
            {
                // create a ConversionUI
                ConversionUI conversionUI = Instantiate(ConversionUIPrefab).GetComponent<ConversionUI>();
                conversionUI.transform.SetParent(ContentPane.transform, false);
                conversionUI.PopulateShipList(_shipyard, mapNode.Coordinates.x.ToString("00") + mapNode.Coordinates.y.ToString("00"), mapNode.Ships, mapNode.Capital == null, turn);
                _conversionEntries.Add(conversionUI);
                conversionUI.OnConversionSelected += ConversionUI_ConversionSelected;
                // since the unity dropdown doesn't connect ot objects, we need to map the dropdow index to the real object
                // or we can do designation searches after the fact
            }
            EPTotal.text = "Total Cost: " + totalCost.ToString("0.0") + "EPs";
        }

        public List<int> GetSelectedValues()
        {
            return null;
        }

        public void ConversionButton_OnClick()
        {
            this.gameObject.SetActive(true);
        }

        public void ConfirmButton_OnClick()
        {
            this.gameObject.SetActive(false);
        }

        public void ConversionUI_ConversionSelected(Conversion conversion)
        {
            float total = 0;
            for (int i = 0; i < _conversionEntries.Count; i++)
            {
                Conversion curCon = _conversionEntries[i].SelectedConversion;
                if(curCon != null)
                {
                    total += curCon.Cost;
                }
            }
            EPTotal.text = "Total Cost: " + total.ToString("0.0") + "EPs";
        }
    }
}