using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Models;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteRenderer))]
    public class HexSprite : MonoBehaviour
    {
        public delegate void ClickAction(HexSprite clickedSprite);
        public event ClickAction OnClicked;
        public delegate void MouseEnterAction(HexSprite enteredSprite);
        public event MouseEnterAction OnEntered;
        public delegate void MouseExitAction(HexSprite exitedSprite);
        public event MouseExitAction OnExited;

        public GameObject LineRendererPrefab;
        public IntVector2 GridPosition;

        Color _baseColor;
        //Color _previousColor;
        public SpriteRenderer Renderer { get; private set; }
        LineRenderer _borderRenderer;

        // Use this for initialization
        void Awake()
        {
            Renderer = GetComponent<SpriteRenderer>();
            DrawBorder();
        }

        private void Start()
        {
            _baseColor = Renderer.color;
        }

        private void DrawBorder()
        {
            List<Vector3> vertexList = new List<Vector3>();
            vertexList.Add(FindVertex1());
            vertexList.Add(FindVertex2());
            vertexList.Add(FindVertex3());
            vertexList.Add(FindVertex4());
            vertexList.Add(FindVertex5());
            vertexList.Add(FindVertex6());
            vertexList.Add(FindVertex1());
            // create new line renderer to draw this border
            _borderRenderer = Instantiate(LineRendererPrefab).GetComponent<LineRenderer>();
            _borderRenderer.transform.parent = Renderer.transform;
            _borderRenderer.positionCount = vertexList.Count;
            _borderRenderer.SetPositions(vertexList.ToArray());
            _borderRenderer.startColor = Color.gray;
            _borderRenderer.endColor = Color.gray;
            _borderRenderer.startWidth = 0.02f;
            _borderRenderer.endWidth = 0.02f;
            _borderRenderer.enabled = false;
        }

        public void ShowBorder(bool showBorder)
        {
            _borderRenderer.enabled = showBorder;
        }

        public void Highlight()
        {
            Renderer.color = new Color(_baseColor.r * 1.25f, _baseColor.g * 1.25f, _baseColor.b * 1.25f);
        }

        public void ResetColor()
        {
            Renderer.color = _baseColor;
        }

        #region Event Handlers
        private void OnMouseEnter()
        {
            if(OnEntered != null)
            {
                OnEntered(this);
            }
        }

        private void OnMouseExit()
        {
            if(OnExited != null)
            {
                OnExited(this);
            }
        }

        private void OnMouseUpAsButton()
        {
            if(OnClicked != null)
            {
                OnClicked(this);
            }
        }
        #endregion

        #region Vertex Logic
        public Vector3 FindVertex1()
        {
            // up half hight, left quarter width
            float x = transform.position.x - (Renderer.bounds.size.x / 4.0f);
            float y = transform.position.y + (Renderer.bounds.size.y / 2.0f);
            return new Vector3(x, y);
        }

        public Vector3 FindVertex2()
        {
            // up half hight, right quarter width
            float x = transform.position.x + (Renderer.bounds.size.x / 4.0f);
            float y = transform.position.y + (Renderer.bounds.size.y / 2.0f);
            return new Vector3(x, y);
        }

        public Vector3 FindVertex3()
        {
            // right half width
            float x = transform.position.x + (Renderer.bounds.size.x / 2.0f);
            return new Vector3(x, transform.position.y);
        }

        public Vector3 FindVertex4()
        {
            // down half hight, right quarter width
            float x = transform.position.x + (Renderer.bounds.size.x / 4.0f);
            float y = transform.position.y - (Renderer.bounds.size.y / 2.0f);
            return new Vector3(x, y);
        }

        public Vector3 FindVertex5()
        {
            // down half hight, left quarter width
            float x = transform.position.x - (Renderer.bounds.size.x / 4.0f);
            float y = transform.position.y - (Renderer.bounds.size.y / 2.0f);
            return new Vector3(x, y);
        }

        public Vector3 FindVertex6()
        {
            // left half width
            float x = transform.position.x - (Renderer.bounds.size.x / 2.0f);
            return new Vector3(x, transform.position.y);
        }
        #endregion
    }
}