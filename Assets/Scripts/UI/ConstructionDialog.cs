using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Models;

namespace Assets.Scripts.UI
{
    public class ConstructionDialog : MonoBehaviour
    {
        public GameObject DropdownPrefab;
        public GameObject ContentPane;
        public Text EPTotal;
        private List<Dropdown> _dropdowns;
        private List<List<ShipClass>> _shipClasses;

        public void PopulateDropdowns(List<List<ShipClass>> dropdownShipClasses)
        {
            _shipClasses = dropdownShipClasses;
            _dropdowns = new List<Dropdown>();
            foreach (Transform child in ContentPane.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            float totalCost = 0;
            foreach (List<ShipClass> shipClassList in dropdownShipClasses)
            {
                // create a dropdown
                Dropdown dropdown = Instantiate(DropdownPrefab).GetComponent<Dropdown>();
                dropdown.transform.SetParent(ContentPane.transform, false);
                //var dropdownText = dropdown.GetComponentInChildren<Text>();
                //dropdownText.text = shipClass.Designation + " " + shipClass.Cost;
                var optionList = shipClassList.Select(sc => sc.GetCostReadout()).ToList();
                optionList.Add("None");
                // add substitutions to dropdown
                dropdown.AddOptions(optionList);
                _dropdowns.Add(dropdown);
                totalCost += shipClassList[0].Cost;
                dropdown.onValueChanged.AddListener(delegate
                {
                    DropdownChangeHandler(dropdown);
                });
                // since the unity dropdown doesn't connect ot objects, we need to map the dropdow index to the real object
                // or we can do designation searches after the fact
            }
            EPTotal.text = "Total Cost: " + totalCost.ToString("0.0") + "EPs";
        }

        public List<int> GetSelectedValues()
        {
            return _dropdowns.Select(d => d.value).ToList();
        }

        public void ConstructionButton_OnClick()
        {
            this.gameObject.SetActive(true);
        }

        public void ConfirmButton_OnClick()
        {
            this.gameObject.SetActive(false);
        }

        public void DropdownChangeHandler(Dropdown dropdown)
        {
            float total = 0;
            for (int i = 0; i < _dropdowns.Count; i++)
            {
                int index = _dropdowns[i].value;
                if (index < _shipClasses[i].Count)
                {
                    total += _shipClasses[i][index].Cost;
                }

            }
            EPTotal.text = "Total Cost: " + total.ToString("0.0") + "EPs";
        }

        public List<ShipClass> GetShipsToConstruct()
        {
            List<ShipClass> shipList = new List<ShipClass>();
            for (int i = 0; i < _dropdowns.Count; i++)
            {
                int index = _dropdowns[i].value;
                if (index < _shipClasses[i].Count)
                {
                    shipList.Add(_shipClasses[i][index]);
                }
            }
            return shipList;
        }
    }
}