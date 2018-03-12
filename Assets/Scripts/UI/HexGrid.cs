using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

using Assets.Scripts;
using Assets.Scripts.Models;

namespace Assets.Scripts.UI
{
    public class HexGrid : MonoBehaviour, IDragHandler
    {
        public delegate void HexEnterAction(HexSprite enteredSprite);
        public event HexEnterAction OnHexEntered;
        public delegate void HexExitAction(HexSprite exitedSprite);
        public event HexExitAction OnHexExited;

        public int GridWidth = 64;
        public int GridHeight = 21;
        public const float sqrt3 = .866025403784f;

        public GameObject HexCellPrefab;
        public GameObject BattlestationPrefab;
        public GameObject StarbasePrefab;
        public GameObject MinorPlanetPrefab;
        public GameObject MajorPlanetPrefab;
        public GameObject CapitalPrefab;
        public GameObject LineRendererPrefab;

        private HexSprite[,] _cells;
        private VisualNode[,] _visualData;
        private List<List<IntVector2>> _provinces;
        public HexSprite SelectedHex;
        public HexSprite HoverHex;

        private Vector3 _cameraOrigin;
        private float _cameraXMin;
        private float _cameraXMax;
        private float _cameraYMin;
        private float _cameraYMax;

        private IEnumerable<IntVector2> _highlightedCells;

        public delegate void HexSelected(IntVector2 gridPosition);
        public event HexSelected OnHexSelect;

        private void Awake()
        {
            _cells = new HexSprite[GridWidth, GridHeight];
            _cameraOrigin = Camera.main.transform.position;

            //set the position of the grid, which will be the position of the top-left cell
            float cameraHeight = Camera.main.orthographicSize * 2.0f;
            float cameraWidth = cameraHeight * Camera.main.aspect;
            this.transform.position = new Vector2(-cameraWidth * 0.30f + 0.5f, cameraHeight * 0.4f);

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    CreateCell(x, y);
                }
            }

            // we only want to allow the grid to move as far left and up as is necessary for the right-most cells to appear in the viewport
            Vector3 bottomRight = _cells[GridWidth - 1, GridHeight - 1].transform.position;
            _cameraXMin = 0;
            _cameraXMax = bottomRight.x - (cameraWidth / 2.0f) + 1f;
            _cameraYMin = bottomRight.y + (cameraHeight / 2.0f) - 2.5f;
            _cameraYMax = 0;

            DrawProvinceBorders();
        }

        void CreateCell(int x, int y)
        {
            HexSprite cell = _cells[x, y] = Instantiate(HexCellPrefab).GetComponent<HexSprite>();
            cell.GridPosition = new IntVector2 { x = x, y = y };
            cell.OnClicked += OnHexClick;
            cell.OnEntered += OnHexSpriteEntered;
            cell.OnExited += OnHexSpriteExited;
            VisualNode mapNode = null;

            if (x < _visualData.GetLength(0) && y < _visualData.GetLength(1))
            {
                mapNode = _visualData[x, y];
            }
            if (mapNode != null)
            {
                cell.Renderer.color = mapNode.Color;
                switch (mapNode.TopVisual)
                {
                    case TopVisual.Starbase:
                        GameObject starBase = Instantiate(StarbasePrefab);
                        SpriteRenderer sbRenderer = starBase.GetComponent<SpriteRenderer>();
                        starBase.transform.SetParent(cell.transform, false);
                        sbRenderer.color = new Color(mapNode.TopVisualColor.r * 0.5f, mapNode.TopVisualColor.g * 0.5f, mapNode.TopVisualColor.b * 0.5f);
                        break;
                    case TopVisual.Battlestation:
                        GameObject battleStation = Instantiate(BattlestationPrefab);
                        SpriteRenderer bsRenderer = battleStation.GetComponent<SpriteRenderer>();
                        battleStation.transform.SetParent(cell.transform, false);
                        bsRenderer.color = new Color(mapNode.TopVisualColor.r * 0.5f, mapNode.TopVisualColor.g * 0.5f, mapNode.TopVisualColor.b * 0.5f);
                        break;
                    case TopVisual.MajorPlanet:
                        GameObject major = Instantiate(MajorPlanetPrefab);
                        SpriteRenderer majRenderer = major.GetComponent<SpriteRenderer>();
                        major.transform.SetParent(cell.transform, false);
                        majRenderer.color = new Color(mapNode.TopVisualColor.r * 0.5f, mapNode.TopVisualColor.g * 0.5f, mapNode.TopVisualColor.b * 0.5f);
                        break;
                    case TopVisual.MinorPlanet:
                        GameObject minor = Instantiate(MinorPlanetPrefab);
                        SpriteRenderer minRenderer = minor.GetComponent<SpriteRenderer>();
                        minor.transform.SetParent(cell.transform, false);
                        minRenderer.color = new Color(mapNode.TopVisualColor.r * 0.5f, mapNode.TopVisualColor.g * 0.5f, mapNode.TopVisualColor.b * 0.5f);
                        break;
                    case TopVisual.Capital:
                        GameObject cap = Instantiate(CapitalPrefab);
                        SpriteRenderer capRenderer = cap.GetComponent<SpriteRenderer>();
                        cap.transform.SetParent(cell.transform, false);
                        capRenderer.color = new Color(mapNode.TopVisualColor.r * 0.5f, mapNode.TopVisualColor.g * 0.5f, mapNode.TopVisualColor.b * 0.5f);
                        break;
                }
            }

            float height = cell.Renderer.sprite.bounds.size.y * cell.Renderer.transform.localScale.y;
            float width = cell.Renderer.sprite.bounds.size.x * cell.Renderer.transform.localScale.x;

            Vector3 position;
            position.x = x * width * 0.75f;
            position.y = -(y + x * 0.5f - (x + 1) / 2) * height;
            position.z = 0;

            cell.transform.SetParent(transform, false);
            cell.transform.localPosition = position;

            TextMesh label = cell.GetComponentInChildren<TextMesh>();
            label.text = x.ToString("00") + y.ToString("00");
            if (x == 0 || y == 0)
            {
                cell.Renderer.enabled = false;
                label.text = "";
            }
        }

        public void HighlightCells(IEnumerable<IntVector2> coordinates)
        {
            if (_highlightedCells != null)
            {
                foreach (IntVector2 coordinate in _highlightedCells)
                {
                    _cells[coordinate.x, coordinate.y].ResetColor();
                }
            }

            foreach (IntVector2 coordinate in coordinates)
            {
                _cells[coordinate.x, coordinate.y].Highlight();
            }
            _highlightedCells = coordinates;
        }

        #region Border Drawing
        void DrawProvinceBorders()
        {
            foreach (List<IntVector2> province in _provinces)
            {
                // get the map cells corresponding to the province cells
                if (province != null && province.Any())
                {
                    var cellArray = province.OrderBy(c => c.x).ThenBy(c => c.y).ToList();
                    DrawProvinceBorder(cellArray);
                }
            }
        }

        private void DrawCellBorder(int x, int y)
        {
            _cells[x, y].GetComponent<HexSprite>().ShowBorder(true);
        }

        private void DrawProvinceBorder(List<IntVector2> gridCells)
        {
            // the first cell should be the top of the leftmost cells;
            // we know its top line should be drawn
            var topLeftSprite = _cells[gridCells[0].x, gridCells[0].y].GetComponent<HexSprite>();
            IntVector2 previousCell = gridCells[0];
            gridCells.RemoveAt(0);
            List<Vector3> vertexList = new List<Vector3>();
            vertexList.Add(topLeftSprite.FindVertex1());
            vertexList.Add(topLeftSprite.FindVertex2());
            int landingVertex = DrawFromVertex2(previousCell, topLeftSprite, previousCell, gridCells, vertexList);
            switch (landingVertex)
            {
                case 3:
                    vertexList.Add(topLeftSprite.FindVertex4());
                    vertexList.Add(topLeftSprite.FindVertex5());
                    vertexList.Add(topLeftSprite.FindVertex6());
                    break;
                case 4:
                    vertexList.Add(topLeftSprite.FindVertex5());
                    vertexList.Add(topLeftSprite.FindVertex6());
                    break;
                case 5:
                    vertexList.Add(topLeftSprite.FindVertex6());
                    break;
            }
            vertexList.Add(topLeftSprite.FindVertex1());

            // create new line renderer to draw this border
            GameObject lineRendererPrefab = Instantiate(LineRendererPrefab);
            LineRenderer lineRenderer = lineRendererPrefab.GetComponent<LineRenderer>();
            lineRenderer.positionCount = vertexList.Count;
            lineRenderer.SetPositions(vertexList.ToArray());
        }

        private int DrawFromVertex1(IntVector2 topLeftCell, HexSprite currentSprite, IntVector2 currentCell, List<IntVector2> gridCells, List<Vector3> vertexList)
        {
            // see if there is a node directly above us
            IntVector2 nextCell = gridCells.FirstOrDefault(c => c.x == currentCell.x && c.y == currentCell.y - 1);

            if (nextCell != null)
            {
                // we need 5->6 of the next cell
                var nextSprite = _cells[nextCell.x, nextCell.y].GetComponent<HexSprite>();
                vertexList.Add(nextSprite.FindVertex6());
                //gridCells.Remove(nextCell);
                return DrawFromVertex6(topLeftCell, nextSprite, nextCell, gridCells, vertexList);
            }
            else
            {
                // see if the topLeftCell is directly above this cell
                if (topLeftCell.x == currentCell.x && topLeftCell.y == currentCell.y - 1)
                {
                    return 5;
                }
                else
                {
                    // draw 1->2 and continue
                    vertexList.Add(currentSprite.FindVertex2());
                    return DrawFromVertex2(topLeftCell, currentSprite, currentCell, gridCells, vertexList);
                }
            }
        }

        private int DrawFromVertex2(IntVector2 topLeftCell, HexSprite currentSprite, IntVector2 currentCell, List<IntVector2> gridCells, List<Vector3> vertexList)
        {
            IntVector2 nextCell;
            // compare the coordinates of the next 
            // if current x even, the next cell on the same y level is up; if odd, it's down
            // if even and x, y or if odd and x, y-1
            if (currentCell.x % 2 == 0)
            {
                nextCell = gridCells.FirstOrDefault(c => c.x == currentCell.x + 1 && c.y == currentCell.y);
            }
            else
            {
                nextCell = gridCells.FirstOrDefault(c => c.x == currentCell.x + 1 && c.y == currentCell.y - 1);
            }

            if (nextCell != null)
            {
                // we need 6->1 of the next cell
                HexSprite nextSprite = _cells[nextCell.x, nextCell.y].GetComponent<HexSprite>();
                vertexList.Add(nextSprite.FindVertex1());
                return DrawFromVertex1(topLeftCell, nextSprite, nextCell, gridCells, vertexList);
            }
            else
            {
                // vertex 2 should not touch the topLeftHex, so draw 2 -> 3 on this cell and continue
                vertexList.Add(currentSprite.FindVertex3());
                return DrawFromVertex3(topLeftCell, currentSprite, currentCell, gridCells, vertexList);
            }
        }

        private int DrawFromVertex3(IntVector2 topLeftCell, HexSprite currentSprite, IntVector2 currentCell, List<IntVector2> gridCells, List<Vector3> vertexList)
        {
            IntVector2 nextCell;
            // compare the coordinates of the next 
            // if current x even, the next cell on the same y level is up; if odd, it's down
            // if even and x, y or if odd and x, y-1
            if (currentCell.x % 2 == 0)
            {
                nextCell = gridCells.FirstOrDefault(c => c.x == currentCell.x + 1 && c.y == currentCell.y + 1);
            }
            else
            {
                nextCell = gridCells.FirstOrDefault(c => c.x == currentCell.x + 1 && c.y == currentCell.y);
            }

            if (nextCell != null)
            {
                // we need 1->2 of the next cell
                HexSprite nextSprite = _cells[nextCell.x, nextCell.y].GetComponent<HexSprite>();
                vertexList.Add(nextSprite.FindVertex2());
                //gridCells.Remove(nextCell);
                return DrawFromVertex2(topLeftCell, nextSprite, nextCell, gridCells, vertexList);
            }
            else
            {
                // vertex 3 should not touch the topLeftHex, so draw 3 -> 4 on this cell and continue
                vertexList.Add(currentSprite.FindVertex4());
                return DrawFromVertex4(topLeftCell, currentSprite, currentCell, gridCells, vertexList);
            }
        }

        private int DrawFromVertex4(IntVector2 topLeftCell, HexSprite currentSprite, IntVector2 currentCell, List<IntVector2> gridCells, List<Vector3> vertexList)
        {
            // see if there is a node directly below us
            IntVector2 nextCell = gridCells.FirstOrDefault(c => c.x == currentCell.x && c.y == currentCell.y + 1);

            if (nextCell != null)
            {
                // we need 2->3 of the next cell
                HexSprite nextSprite = _cells[nextCell.x, nextCell.y].GetComponent<HexSprite>();
                vertexList.Add(nextSprite.FindVertex3());
                return DrawFromVertex3(topLeftCell, nextSprite, nextCell, gridCells, vertexList);
            }
            else
            {
                //top left cannot be directly below us
                // draw 4->5 and continue
                vertexList.Add(currentSprite.FindVertex5());
                return DrawFromVertex5(topLeftCell, currentSprite, currentCell, gridCells, vertexList);
            }
        }

        private int DrawFromVertex5(IntVector2 topLeftCell, HexSprite currentSprite, IntVector2 currentCell, List<IntVector2> gridCells, List<Vector3> vertexList)
        {
            IntVector2 nextCell;
            // compare the coordinates of the next 
            // if current x even, the next cell on the same y level is up; if odd, it's down
            // if even and x, y or if odd and x, y-1
            if (currentCell.x % 2 == 0)
            {
                nextCell = gridCells.FirstOrDefault(c => c.x == currentCell.x - 1 && c.y == currentCell.y + 1);
            }
            else
            {
                nextCell = gridCells.FirstOrDefault(c => c.x == currentCell.x - 1 && c.y == currentCell.y);
            }

            if (nextCell != null)
            {
                // we need 3->4 of the next cell
                HexSprite nextSprite = _cells[nextCell.x, nextCell.y].GetComponent<HexSprite>();
                vertexList.Add(nextSprite.FindVertex4());
                return DrawFromVertex4(topLeftCell, nextSprite, nextCell, gridCells, vertexList);
            }
            else
            {
                // see if the top left hex is to our bottom left
                if ((currentCell.x % 2 == 0 && topLeftCell.x == currentCell.x - 1 && topLeftCell.y == currentCell.y + 1) ||
                    (currentCell.x % 2 == 1 && topLeftCell.x == currentCell.x - 1 && topLeftCell.y == currentCell.y))
                {
                    return 2;
                }
                else
                {
                    // draw 5->6 on this cell and continue
                    vertexList.Add(currentSprite.FindVertex6());
                    return DrawFromVertex6(topLeftCell, currentSprite, currentCell, gridCells, vertexList);
                }
            }
        }

        private int DrawFromVertex6(IntVector2 topLeftCell, HexSprite currentSprite, IntVector2 currentCell, List<IntVector2> gridCells, List<Vector3> vertexList)
        {
            IntVector2 nextCell;
            // compare the coordinates of the next 
            // if current x even, the next cell on the same y level is up; if odd, it's down
            // if even and x, y or if odd and x, y-1
            if (currentCell.x % 2 == 0)
            {
                nextCell = gridCells.FirstOrDefault(c => c.x == currentCell.x - 1 && c.y == currentCell.y);
            }
            else
            {
                nextCell = gridCells.FirstOrDefault(c => c.x == currentCell.x - 1 && c.y == currentCell.y - 1);
            }

            if (nextCell != null)
            {
                // we need 4->5 of the next cell
                HexSprite nextSprite = _cells[nextCell.x, nextCell.y].GetComponent<HexSprite>();
                vertexList.Add(nextSprite.FindVertex5());
                return DrawFromVertex5(topLeftCell, nextSprite, nextCell, gridCells, vertexList);
            }
            else
            {
                // see if the top left hex is to our top left
                if ((currentCell.x % 2 == 0 && topLeftCell.x == currentCell.x - 1 && topLeftCell.y == currentCell.y) ||
                    (currentCell.x % 2 == 1 && topLeftCell.x == currentCell.x - 1 && topLeftCell.y == currentCell.y - 1))
                {
                    return 4;
                }
                else
                {
                    // draw 6->1 on this cell and continue
                    vertexList.Add(currentSprite.FindVertex1());
                    return DrawFromVertex1(topLeftCell, currentSprite, currentCell, gridCells, vertexList);
                }
            }
        }

        #endregion

        void LateUpdate()
        {
            if (Input.GetMouseButton(1)) // reset camera to original position
            {
                Camera.main.transform.position = _cameraOrigin;
            }
        }

        private void OnHexClick(HexSprite hexSprite)
        {
            if (SelectedHex != null)
            {
                SelectedHex.ShowBorder(false);
            }
            hexSprite.ShowBorder(true);
            SelectedHex = hexSprite;
            if(OnHexSelect != null)
            {
                OnHexSelect(hexSprite.GridPosition);
            }
        }

        private void OnHexSpriteEntered(HexSprite hexSprite)
        {
            HoverHex = hexSprite;
            if(OnHexEntered != null)
            {
                OnHexEntered(hexSprite);
            }
        }

        private void OnHexSpriteExited(HexSprite hexSprite)
        {
            if (HoverHex == hexSprite)
            {
                HoverHex = null;
            }
            if(OnHexExited != null)
            {
                OnHexExited(hexSprite);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            Vector3 newPos = Camera.main.transform.position - new Vector3(eventData.delta.x * 0.05f, eventData.delta.y * 0.05f);
            if (newPos.x > _cameraXMax)
            {
                newPos.x = _cameraXMax;
            }
            else if (newPos.x < _cameraXMin)
            {
                newPos.x = _cameraXMin;
            }
            if (newPos.y > _cameraYMax)
            {
                newPos.y = _cameraYMax;
            }
            else if (newPos.y < _cameraYMin)
            {
                newPos.y = _cameraYMin;
            }
            Camera.main.transform.position = newPos;
        }

        public void PopulateVisualData(VisualNode[,] visualData, List<List<IntVector2>> provinces)
        {
            _visualData = visualData;
            _provinces = provinces;
        }
    }
}