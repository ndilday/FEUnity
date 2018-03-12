using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts.Econ;
using Assets.Scripts.Models;
using Assets.Scripts.State;
using Assets.Scripts.UI;
using System;

namespace Assets.Scripts
{
    public enum Phases
    {
        Deploy,
        TurnStart,
        Survey,
        Diplomacy,
        Economic,
        Move,
        Interrupt,
        Retrograde,
        Repair
    }

    public class GameManager : MonoBehaviour
    {
        private MapData _mapData;
        //private ShipClass[] _shipClasses;
        private VisualNode[,] _visualNodes;
        private List<List<IntVector2>> _provinceList;
        private Faction _activeFaction;
        private FleetAccordion _draggingAccordion;
        private ShipInfo _draggingShipInfo;
        private Texture2D _disabledCursorTexture;
        private IState[] _states;
        private IState _currentState;

        public static GameManager instance = null;

        public Phases CurrentPhase { get; private set; }
        public int CurrentTurn { get; private set; }

        public HexGrid HexGrid;
        public ConstructionDialog ConstructionDialog;
        public ConversionDialog ConversionDialog;
        public EconomicDialog EconomicDialog;
        public HexContentView HexContentView;
        public FleetDisplayUI FleetDisplayArea;
        public Sprite DisabledCursorSprite;

        public Text PhaseText;
        public Button EcoButton;
        public Button ConstructButton;
        public Button ConvertButton;
        public Button ShipButton;
        public Button PlanetButton;
        public Button PhaseButton;

        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
            }
            else if(instance != this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);

            _states = new IState[2];
            _states[0] = new Deploy();
            _states[1] = new Economics();

            FleetDisplayArea.OnShipInfoDragged += ShipInfo_Dragged;
            FleetDisplayArea.OnShipInfoDragEnd += ShipInfo_DragEnd;
            FleetDisplayArea.OnFleetDisplayEmpty += FleetDisplayArea_Empty;
            FleetDisplayArea.OnFleetSelected += FleetDisplayArea_FleetSelected;

            HexGrid.OnHexEntered += HexGrid_HexEntered;
            HexGrid.OnHexExited += HexGrid_HexExited;
            HexGrid.OnHexSelect += PopulateHexContents;

            DataStore.DataStore store = new DataStore.DataStore();
            _mapData = store.GetMapData();
            //_shipClasses = store.GetShipClasses();
            CurrentTurn = 136;
            _activeFaction = _mapData.GetFaction(Factions.Lyran);
            PopulateProvinceNodes();
            PopulateVisualNodes();
            HexGrid.PopulateVisualData(_visualNodes, _provinceList);
            
            _currentState = _states[0];
            PhaseButton.enabled = false;

            /*_disabledCursorTexture = new Texture2D((int)DisabledCursorSprite.textureRect.width, (int)DisabledCursorSprite.textureRect.height);
            var pixels = DisabledCursorSprite.texture.GetPixels((int)DisabledCursorSprite.textureRect.x,
                                                    (int)DisabledCursorSprite.textureRect.y,
                                                    (int)DisabledCursorSprite.textureRect.width,
                                                    (int)DisabledCursorSprite.textureRect.height);
            _disabledCursorTexture.SetPixels(pixels);
            _disabledCursorTexture.Resize(32, 32);
            _disabledCursorTexture.Apply();*/
        }

        private void Start()
        {
            EnterDeployPhase();
        }

        #region Phase Logic
        private void EnterDeployPhase()
        {
            PopulateFleetDisplay();
            PhaseText.text = _activeFaction.Name + " Deployment Phase";
            PhaseButton.enabled = false;

            ConstructButton.enabled = false;
            ConvertButton.enabled = false;
            EcoButton.enabled = false;
            PlanetButton.enabled = false;
            ShipButton.enabled = false;
            
            CurrentPhase = Phases.Deploy;
        }

        private void EnterTurnStartPhase()
        {
            // base/PDU upgrades become operational
            // evaluate supply status
            // recovery from devestation
            // long term capture
            // annexation
        }

        private void EnterSurveyPhase()
        {
            // assign/remove survey ships in off-map area
            // determine survey points generated
            // annex/transfer provinces
            // diplomacy
        }

        private void EnterEconomicsPhase()
        {
            // evaluate supply for repairs
            // handle repairs
            // 

            PhaseText.text = _activeFaction.Name + " Economic Phase";
            PhaseButton.enabled = false;

            // populate construction dialog
            PopulateConstructionContent();

            // populate conversion dialog
            PopulateConversionContent();

            // economic dialog will be populated on the fly

            ConstructButton.enabled = true;
            ConvertButton.enabled = true;
            EcoButton.enabled = true;
            PlanetButton.enabled = false;
            ShipButton.enabled = false;
            
            CurrentPhase = Phases.Economic;
        }
        #endregion

        #region Initialization Code
        private void PopulateProvinceNodes()
        {
            _provinceList = new List<List<IntVector2>>();
            foreach(Province province in _mapData.ProvinceMap)
            {
                if(province != null)
                {
                    _provinceList.Add(province.MapNodes.Select(mn => new IntVector2 { x = mn.Coordinates.x, y = mn.Coordinates.y }).ToList());
                }
            }
        }

        private void PopulateVisualNodes()
        {
            int xDim = _mapData.MapNodes.GetLength(0);
            int yDim = _mapData.MapNodes.GetLength(1);
            _visualNodes = new VisualNode[xDim, yDim];

            for (int i = 0; i < xDim; i++)
            {
                for (int j = 0; j < yDim; j++)
                {
                    if (_mapData.MapNodes[i, j] != null)
                    {
                        _visualNodes[i, j] = GetVisualNode(_mapData.MapNodes[i, j]);
                    }
                }
            }
        }

        private void PopulateConstructionContent()
        {
            var lyrans = _mapData.GetFaction(Factions.Lyran);
            var plan = lyrans.Shipyard.ConstructionPlan.GetBasePlanForTurn(136);
            List<List<ShipClass>> planSubstitutions = new List<List<ShipClass>>(plan.Count);
            for (int i = 0; i < plan.Count; i++)
            {
                ShipClass shipClass = plan[i];
                var substitutions = lyrans.Shipyard.GetPossibleSubstitutions(shipClass)
                    .Where(s => s.NewClass.DateAvailable <= CurrentTurn)
                    .Select(s => s.NewClass)
                    .ToList();
                substitutions.Insert(0, shipClass);
                planSubstitutions.Add(substitutions);
            }

            ConstructionDialog.PopulateDropdowns(planSubstitutions);
        }

        private void PopulateConversionContent()
        {
            var mapNodes = GetNodesWithBases(Factions.Lyran, BaseType.Starbase).Where(mn => mn.Ships != null && mn.Ships.Any()).ToList();
            ConversionDialog.PopulateDropdowns(GetFaction(Factions.Lyran).Shipyard, mapNodes, CurrentTurn);
        }

        private void PopulateFleetDisplay()
        {
            FleetDisplayArea.PopulateFleets(_activeFaction.Fleets);
        }

        private void PopulateHexContents(IntVector2 gridPosition)
        {
            MapNode node = _mapData.GetMapNode(gridPosition.x, gridPosition.y);
            if (node != null && node.Ships != null)
            {
                HexContentView.PopulateShips(node.Ships);
            }
        }
        #endregion

        public Faction GetFaction(Factions faction)
        {
            return _mapData.GetFaction(faction);
        }

        public VisualNode GetVisualNode(MapNode node)
        {
            TopVisual tv = TopVisual.None;
            if (node.Capital != null)
            {
                tv = TopVisual.Capital;
            }
            else if (node.HasMajorPlanet())
            {
                tv = TopVisual.MajorPlanet;
            }
            else if (node.HasMinorPlanet())
            {
                tv = TopVisual.MinorPlanet;
            }
            else if (node.HasStarbase())
            {
                tv = TopVisual.Starbase;
            }
            else if (node.HasBattlestation())
            {
                tv = TopVisual.Battlestation;
            }

            return new VisualNode
            {
                Color = _mapData.GetFactionColor(node.OriginalOwner),
                TopVisual = tv,
                TopVisualColor = _mapData.GetFactionColor(node.OriginalOwner)
            };
        }

        public void HighlightFleetCells(Fleet fleet)
        {
            HexGrid.HighlightCells(fleet.DeploymentHexes.Select(hex => new IntVector2 { x = hex.Coordinates.x, y = hex.Coordinates.y }));
        }

        private bool IsAllowedFleetShipPlacement(Fleet fleet, IntVector2 hex)
        {
            return fleet.DeploymentHexes.Where(dh => dh.Coordinates.x == hex.x && dh.Coordinates.y == hex.y).Any();
        }

        private List<MapNode> GetNodesWithBases(Factions f, BaseType baseType)
        {
            List<MapNode> retList = new List<MapNode>();
            Faction faction = GetFaction(f);
            foreach(Base b in faction.Bases)
            {
                if(b.BaseType == baseType)
                {
                    MapNode node = _mapData.MapNodes[b.Location.x, b.Location.y];
                    if (!retList.Contains(node))
                    {
                        retList.Add(node);
                    }
                }
            }
            if(faction.CapitalMapNode != null)
            {
                MapNode node = _mapData.MapNodes[faction.CapitalMapNode.Coordinates.x, faction.CapitalMapNode.Coordinates.y];
                if (!retList.Contains(node))
                {
                    foreach (var system in faction.CapitalMapNode.Capital.CapitalSystems)
                    {
                        if (system.Bases != null)
                        {
                            if(system.Bases.Any(b => b.BaseType == baseType && b.Faction == faction.Id))
                            {
                                retList.Add(faction.CapitalMapNode);
                                break;
                            }
                        }
                    }
                }
            }

            return retList;
        }

        private List<MapNode> GetNodesWithSupplyPoints(Faction faction)
        {
            List<MapNode> retList = new List<MapNode>();
            foreach (Base b in faction.Bases)
            {
                MapNode node = _mapData.MapNodes[b.Location.x, b.Location.y];
                if (!retList.Contains(node))
                {
                    retList.Add(node);
                }
            }
            // TODO: what's the best way to handle planets?
            if (faction.CapitalMapNode != null)
            {
                MapNode node = _mapData.MapNodes[faction.CapitalMapNode.Coordinates.x, faction.CapitalMapNode.Coordinates.y];
                if (!retList.Contains(node))
                {
                    foreach (var system in faction.CapitalMapNode.Capital.CapitalSystems)
                    {
                        if (system.Bases != null && system.Bases.Any(b => b.Faction == faction.Id))
                        {
                            retList.Add(faction.CapitalMapNode);
                            break;
                        }
                        if(system.Planets != null && system.Planets.Any(p => p.OwningFaction == faction.Id))
                        {
                            retList.Add(faction.CapitalMapNode);
                            break;
                        }
                    }
                }
            }

            return retList;
        }

        private void PopulateFactionSupplyGrids(Faction faction)
        {
            int xLength = _visualNodes.GetLength(0);
            int yLength = _visualNodes.GetLength(1);
            SupplyStates[,] supplyStatesMap = CreateSupplyStatesMap(faction, xLength, yLength);

            // now we want to define the sets of hexes which are maximally six hexes away from the capital
            // if we get to a base or planet, the distance is reset to 0
            int[,] supplyDistanceMap = new int[xLength, yLength];
            for(int i = 0; i < xLength; i++)
            {
                for(int j = 0; j < yLength; j++)
                {
                    supplyDistanceMap[i, j] = 100;
                }
            }

            var supplyPoints = GetNodesWithSupplyPoints(faction);
            List<MapNode> gridNodes = new List<MapNode>();
            SupplyDistanceHelper(supplyDistanceMap, supplyStatesMap, xLength, yLength, faction, supplyPoints, gridNodes, faction.CapitalMapNode.Coordinates, 0);
            List<List<MapNode>> supplyGrids = new List<List<MapNode>>();
            supplyGrids.Add(gridNodes);
            while(supplyPoints.Count > 0)
            {
                List<MapNode> partialGridNodes = new List<MapNode>();
                MapNode startingSupplyPoint = supplyPoints.First();
                SupplyDistanceHelper(supplyDistanceMap, supplyStatesMap, xLength, yLength, faction, supplyPoints, partialGridNodes, startingSupplyPoint.Coordinates, 0);
                supplyGrids.Add(partialGridNodes);
            }
            faction.SupplyGrids = supplyGrids;
        }

        private void SupplyDistanceHelper(int[,] supplyDistanceMap, SupplyStates[,] supplyStatesMap, int xLength, int yLength, Faction faction, 
                                          List<MapNode> supplyPoints, List<MapNode> gridNodes, IntVector2 position, int distance)
        {
            //if this hex has already been set, it's a no-op
            if(supplyDistanceMap[position.x, position.y] > distance && 
               supplyStatesMap[position.x, position.y] != SupplyStates.ContainsEnemies && 
               supplyStatesMap[position.x, position.y] != SupplyStates.EnemyAdjacent)
            {
                // see if we contain a base or planet for the faction in question
                MapNode mapNode = _mapData.GetMapNode(position.x, position.y);
                if(mapNode.Bases.Any(b => b.Faction == faction.Id) || mapNode.Planets.Any(p => p.OwningFaction == faction.Id))
                {
                    // this should be in the supply point list
                    supplyPoints.Remove(mapNode);
                    distance = 0;
                }
                supplyDistanceMap[position.x, position.y] = distance;
                gridNodes.Add(mapNode);
                if(distance < 6)
                {
                    // iterate through adjacent nodes
                    // up
                    if(position.y > 1)
                    {
                        SupplyDistanceHelper(supplyDistanceMap, supplyStatesMap, xLength, yLength, faction, supplyPoints, gridNodes, new IntVector2 { x = position.x, y = position.y - 1 }, distance + 1);

                    }
                    // down
                    if(position.y < yLength - 1)
                    {
                        SupplyDistanceHelper(supplyDistanceMap, supplyStatesMap, xLength, yLength, faction, supplyPoints, gridNodes, new IntVector2 { x = position.x, y = position.y + 1 }, distance + 1);
                    }

                    // if x is even, same y is up, y + 1 is down
                    // if x is odd, same y is down, y - 1 is up
                    if (position.x > 1)
                    {
                        SupplyDistanceHelper(supplyDistanceMap, supplyStatesMap, xLength, yLength, faction, supplyPoints, gridNodes, new IntVector2 { x = position.x - 1, y = position.y }, distance + 1);
                        if (position.x % 2 == 0 && position.y < yLength - 1)
                        {
                            SupplyDistanceHelper(supplyDistanceMap, supplyStatesMap, xLength, yLength, faction, supplyPoints, gridNodes, new IntVector2 { x = position.x - 1, y = position.y + 1 }, distance + 1);
                        }
                        else if (position.x % 2 == 1 && position.y > 1)
                        {
                            SupplyDistanceHelper(supplyDistanceMap, supplyStatesMap, xLength, yLength, faction, supplyPoints, gridNodes, new IntVector2 { x = position.x - 1, y = position.y - 1 }, distance + 1);
                        }
                    }
                    if (position.x < xLength - 1)
                    {
                        SupplyDistanceHelper(supplyDistanceMap, supplyStatesMap, xLength, yLength, faction, supplyPoints, gridNodes, new IntVector2 { x = position.x + 1, y = position.y }, distance + 1);
                        if (position.x % 2 == 0 && position.y < yLength - 1)
                        {
                            SupplyDistanceHelper(supplyDistanceMap, supplyStatesMap, xLength, yLength, faction, supplyPoints, gridNodes, new IntVector2 { x = position.x + 1, y = position.y + 1 }, distance + 1);
                        }
                        else if (position.x % 2 == 1 && position.y > 1)
                        {
                            SupplyDistanceHelper(supplyDistanceMap, supplyStatesMap, xLength, yLength, faction, supplyPoints, gridNodes, new IntVector2 { x = position.x + 1, y = position.y - 1 }, distance + 1);
                        }
                    }
                }
            }
        }

        private SupplyStates[,] CreateSupplyStatesMap(Faction faction, int xLength, int yLength)
        {
            SupplyStates[,] supplyStatesMap = new SupplyStates[xLength, yLength];

            // on the initialization pass, check for enemy ships/bases
            for (int i = 0; i < xLength; i++)
            {
                for (int j = 0; j < yLength; j++)
                {
                    MapNode currentNode = _mapData.GetMapNode(i, j);
                    // TODO: allies should be okay
                    // TODO: base fighters count for blocking, but not unblocking
                    if (currentNode.Ships.Any(s => s.Faction == faction.Id))
                    {
                        supplyStatesMap[i, j] = SupplyStates.ContainsFriendlyShips;
                    }
                    else if (currentNode.Bases.Any(b => b.Faction == faction.Id) ||
                            currentNode.Planets.Any(p => p.OwningFaction == faction.Id))
                    {
                        supplyStatesMap[i, j] = SupplyStates.ContainsFriendlyUnits;
                    }
                    else if (currentNode.Capital != null &&
                            (currentNode.Capital.CapitalSystems.Any(
                                cs => cs.Bases.Any(b => b.Faction == faction.Id) ||
                                cs.Planets.Any(p => p.OwningFaction == faction.Id))
                            )
                           )
                    {
                        supplyStatesMap[i, j] = SupplyStates.ContainsFriendlyUnits;
                    }
                    else if (currentNode.Ships.Any(s => s.Faction != faction.Id) ||
                        currentNode.Bases.Any(b => b.Faction != faction.Id))
                    {
                        supplyStatesMap[i, j] = SupplyStates.ContainsEnemies;
                    }
                    else
                    {
                        supplyStatesMap[i, j] = SupplyStates.Unset;
                    }
                }
            }

            // on a second pass, for any unset nodes,
            // adjacent to enemy nodes are enemy adjacent
            // unless they have a friendly unit in them
            // or are adjacent to friendly ships
            for (int i = 0; i < xLength; i++)
            {
                for (int j = 0; j < yLength; j++)
                {
                    bool adjacentEnemies = false;
                    bool adjacentFriends = false;
                    if (supplyStatesMap[i, j] == SupplyStates.Unset)
                    {
                        if (j > 1)
                        {
                            adjacentEnemies |= supplyStatesMap[i, j - 1] == SupplyStates.ContainsEnemies;
                            adjacentFriends |= supplyStatesMap[i, j - 1] == SupplyStates.ContainsFriendlyShips;
                        }
                        if (j < yLength - 1)
                        {
                            adjacentEnemies |= supplyStatesMap[i, j + 1] == SupplyStates.ContainsEnemies;
                            adjacentFriends |= supplyStatesMap[i, j + 1] == SupplyStates.ContainsFriendlyShips;
                        }
                        // if x is even, same y is up, y + 1 is down
                        // if x is odd, same y is down, y - 1 is up
                        if (i > 1)
                        {
                            adjacentEnemies |= supplyStatesMap[i - 1, j] == SupplyStates.ContainsEnemies;
                            adjacentFriends |= supplyStatesMap[i - 1, j] == SupplyStates.ContainsFriendlyShips;
                            if (i % 2 == 0 && j < yLength - 1)
                            {
                                adjacentEnemies |= supplyStatesMap[i - 1, j + 1] == SupplyStates.ContainsEnemies;
                                adjacentFriends |= supplyStatesMap[i - 1, j + 1] == SupplyStates.ContainsFriendlyShips;
                            }
                            else if (i % 2 == 1 && j > 1)
                            {
                                adjacentEnemies |= supplyStatesMap[i - 1, j - 1] == SupplyStates.ContainsEnemies;
                                adjacentFriends |= supplyStatesMap[i - 1, j - 1] == SupplyStates.ContainsFriendlyShips;
                            }
                        }
                        if (i < xLength - 1)
                        {
                            adjacentEnemies |= supplyStatesMap[i + 1, j] == SupplyStates.ContainsEnemies;
                            adjacentFriends |= supplyStatesMap[i + 1, j] == SupplyStates.ContainsFriendlyShips;
                            if (i % 2 == 0 && j < yLength - 1)
                            {
                                adjacentEnemies |= supplyStatesMap[i + 1, j + 1] == SupplyStates.ContainsEnemies;
                                adjacentFriends |= supplyStatesMap[i + 1, j + 1] == SupplyStates.ContainsFriendlyShips;
                            }
                            else if (i % 2 == 1 && j > 1)
                            {
                                adjacentEnemies |= supplyStatesMap[i + 1, j - 1] == SupplyStates.ContainsEnemies;
                                adjacentFriends |= supplyStatesMap[i + 1, j - 1] == SupplyStates.ContainsFriendlyShips;
                            }
                        }
                    }
                    if (adjacentEnemies && !adjacentFriends)
                    {
                        supplyStatesMap[i, j] = SupplyStates.EnemyAdjacent;
                    }
                    else if (adjacentEnemies)
                    {
                        supplyStatesMap[i, j] = SupplyStates.Protected;
                    }
                    else
                    {
                        supplyStatesMap[i, j] = SupplyStates.Clear;
                    }
                }
            }

            return supplyStatesMap;
        }

        private int CalculateHexDistance(IntVector2 h1, IntVector2 h2)
        {
            int cx1 = h1.x;
            int cz1 = h1.y - (h1.x + (h1.x & 1)) / 2;
            int cy1 = -cx1 - cz1;
            int cx2 = h2.x;
            int cz2 = h2.y - (h2.x + (h2.x & 1)) / 2;
            int cy2 = -cx2 - cz2;
            int dx = Math.Abs(cx1 - cx2);
            int dy = Math.Abs(cy1 - cy2);
            int dz = Math.Abs(cz1 - cz2);
            return Math.Max(dx, Math.Max(dy, dz));
        }

        #region Event Handlers
        private void PhaseButton_Click()
        {
            if(CurrentPhase == Phases.Deploy)
            {
                EnterEconomicsPhase();
            }
        }

        private void ShipInfo_Dragged(PointerEventData eventData, ShipInfo shipInfo, FleetAccordion accordion)
        {
            if (_draggingShipInfo == null)
            {
                _draggingAccordion = accordion;
                _draggingShipInfo = shipInfo;
            }
        }

        private void ShipInfo_DragEnd(PointerEventData eventData, IEnumerable<ShipInfo> selectedShips, FleetAccordion accordion)
        {
            // if we're currently hovering over a hex, the ship is being placed in that hex
            if(HexGrid.HoverHex != null && IsAllowedFleetShipPlacement(accordion.Fleet, HexGrid.HoverHex.GridPosition))
            {
                foreach (ShipInfo shipInfo in selectedShips)
                {
                    // add the ship to the hex hovered on
                    _mapData.GetMapNode(HexGrid.HoverHex.GridPosition.x, HexGrid.HoverHex.GridPosition.y).Ships.Add(shipInfo.Ship);
                    // remove the ShipInfo from the ShipDisplay
                    FleetDisplayArea.RemoveShipInfo(accordion, shipInfo);
                    if (HexGrid.SelectedHex != null && HexGrid.HoverHex.GridPosition == HexGrid.SelectedHex.GridPosition)
                    {
                        HexContentView.AddShip(shipInfo.Ship);

                    }
                }
            }
            _draggingAccordion = null;
            _draggingShipInfo = null;
        }

        private void FleetDisplayArea_Empty()
        {
            FleetDisplayArea.enabled = false;
            PhaseButton.enabled = true;
        }

        private void FleetDisplayArea_FleetSelected(Fleet fleet)
        {
            HighlightFleetCells(fleet);
        }

        private void HexGrid_HexExited(HexSprite exitedSprite)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void HexGrid_HexEntered(HexSprite enteredSprite)
        {
            // if dragging a ship, see if we're entering a hex that ship can be put into or not
            if(_draggingAccordion != null)
            {
                if(!_draggingAccordion.Fleet.DeploymentHexes.Contains(_mapData.GetMapNode(enteredSprite.GridPosition.x, enteredSprite.GridPosition.y)))
                {
                    //Cursor.SetCursor(_disabledCursorTexture, new Vector2(0.5f, 0.5f), CursorMode.Auto);
                }
                else
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
            }
        }
        #endregion
    }
}