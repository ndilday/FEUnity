using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class EconomicDialog : MonoBehaviour
    {
        public Transform DescriptionPane;
        public Transform ValuePane;
        public GameObject EconTextPrefab;

        public void PopulateEconomicData(List<string> descriptions, List<string> values)
        {
            for (int i = 0; i < descriptions.Count; i++)
            {
                var text = Instantiate(EconTextPrefab);
                text.transform.parent = DescriptionPane;
                text.GetComponent<Text>().text = descriptions[i];
                text = Instantiate(EconTextPrefab);
                text.transform.parent = ValuePane;
                text.GetComponent<Text>().text = values[i];
            }
        }

        public void EconomicButton_OnClick()
        {
            this.gameObject.SetActive(true);
        }

        public void CloseButton_OnClick()
        {
            this.gameObject.SetActive(false);
        }
    }
}