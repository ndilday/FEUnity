using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Econ;
using Assets.Scripts.Models;

namespace Assets.Scripts.UI
{
    public class ConversionUI : MonoBehaviour
    {
        public delegate void ConversionSelected(Conversion conversion);
        public event ConversionSelected OnConversionSelected;

        public Dropdown ShipDropdown;
        public Dropdown ClassDropdown;
        public Text LocationText;
        public Conversion SelectedConversion { get; private set; }
        public Ship SelectedShip { get; private set; }

        private Shipyard _shipyard;
        private List<Ship> _ships;
        private List<Conversion> _currentConversions;
        private bool _isMinorOnly;
        private int _turn;
        
        public void PopulateShipList(Shipyard shipyard, string location, List<Ship> ships, bool minorOnly, int turn)
        {
            _shipyard = shipyard;
            _ships = ships;
            List<string> optionList = new List<string>();
            optionList.Add("None");
            optionList.AddRange(ships.Select(s => s.ToString()));
            ShipDropdown.AddOptions(optionList);
            LocationText.text = location;
            _isMinorOnly = minorOnly;
            _turn = turn;
        }

        public void Awake()
        {
            ShipDropdown = ShipDropdown.GetComponent<Dropdown>();
            ClassDropdown = ClassDropdown.GetComponent<Dropdown>();
            ClassDropdown.onValueChanged.AddListener(delegate
            {
                ConversionDropdownChangeHandler(ClassDropdown);
            });
            ShipDropdown.onValueChanged.AddListener(delegate
            {
                ShipDropdownChangeHandler(ShipDropdown);
            });
            ClassDropdown.enabled = false;
        }

        public void ShipDropdownChangeHandler(Dropdown dropdown)
        {
            if(dropdown.value == 0)
            {
                ClassDropdown.ClearOptions();
                ClassDropdown.enabled = false;
                SelectedShip = null;
                SelectedConversion = null;
            }
            else
            {
                SelectedShip = _ships[dropdown.value - 1];
                SelectedConversion = null;
                _currentConversions = _shipyard.GetPossibleConversions(SelectedShip.Class, _isMinorOnly, _turn).ToList();
                ClassDropdown.ClearOptions();
                ClassDropdown.AddOptions(_currentConversions.Select(c => c.NewClass.Designation + " (" + c.NewClass.Factors + "): " + c.Cost.ToString("0.0") + "EP").ToList());
                ClassDropdown.enabled = true;
                if(ClassDropdown.options.Any())
                {
                    ConversionDropdownChangeHandler(ClassDropdown);
                }
            }
        }

        public void ConversionDropdownChangeHandler(Dropdown dropdown)
        {
            SelectedConversion = _currentConversions[dropdown.value];
            if(OnConversionSelected != null)
            {
                OnConversionSelected(SelectedConversion);
            }
        }
    }
}
