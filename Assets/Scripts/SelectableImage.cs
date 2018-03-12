using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Outline))]
    public class SelectableImage : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {
        public static HashSet<SelectableImage> AllSelectables = new HashSet<SelectableImage>();
        public static HashSet<SelectableImage> CurrentlySelected = new HashSet<SelectableImage>();

        private Renderer _renderer;
        private Outline _outline;

        protected virtual void Awake()
        {
            AllSelectables.Add(this);
            _renderer = GetComponent<Renderer>();
            _outline = GetComponent<Outline>();
        }

        protected virtual void OnDestroy()
        {
            if(CurrentlySelected.Contains(this))
            {
                CurrentlySelected.Remove(this);
            }
            AllSelectables.Remove(this);
        }

        public void OnSelect(BaseEventData eventData)
        {
            CurrentlySelected.Clear();
            AddSelection();
        }

        public void AddSelection()
        {
            CurrentlySelected.Add(this);
            _outline.enabled = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            CurrentlySelected.Remove(this);
            _outline.enabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {

            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if(ctrlHeld)
            {
                if(CurrentlySelected.Contains(this))
                {
                    OnDeselect(eventData);
                }
                else
                {
                    AddSelection();
                }
            }
            else if(shiftHeld)
            {
                AddSelection();
            }
            else
            {
                OnSelect(eventData);
            }
        }
    }
}
