using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

using Assets.Scripts.Econ;
using Assets.Scripts.Models;

namespace Assets.Scripts.DataStore
{
    class DataStore
    {
        private IDbConnection _dbConn;
        private MapNode[,] _mapNodes;
        private int[,] _provinceHexMap;
        private Faction[] _factions;
        private Province[] _provinces;
        private ShipClass[] _shipClasses;
        private Ship[] _ships;
        private Fleet[] _fleets;
        private Dictionary<Factions, List<Conversion>> _conversionMap;
        private Dictionary<Factions, List<Substitution>> _substitutionMap;
        private Dictionary<Factions, OffMap> _offMapAreaMap;
        private Dictionary<Factions, List<MapNode>> _factionMapMap;

        public DataStore()
        {
            string conn = "URI=file:" + Application.dataPath + "/GameData/Setup.db3"; //Path to database.
            _dbConn = (IDbConnection)new SqliteConnection(conn);
            _dbConn.Open(); //Open connection to the database.
            PopulateShipClasses();
            PopulateFactions();
            PopulateConversions();
            PopulateSubstitutions();
            PopulateFactionConstructionPlans();
            PopulateMapData();
            PopulateProvinces();
            PopulateBases();
            PopulatePlanets();
            PopulateCapitals();
            PopulateOffMapAreas();
            PopulateShipData();
            _dbConn.Close();

            foreach(Faction faction in _factions)
            {
                if (_factionMapMap.ContainsKey(faction.Id) && _offMapAreaMap.ContainsKey(faction.Id))
                {
                    faction.AddMapNodes(_factionMapMap[faction.Id], _offMapAreaMap[faction.Id]);
                }
            }
        }

        public ShipClass[] GetShipClasses()
        {
            return _shipClasses;
        }

        public MapData GetMapData()
        {
            return new MapData(_mapNodes, _provinceHexMap, _factions, _provinces);
        }

        private void PopulateShipClasses()
        {
            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT Id, FactionId, Designation, Offense, Defense, CrippledOffense, CrippledDefense, Command, DateAvailable, Salvage, Cost, FighterFactors, CrippledFighterFactors, HFFactors, CrippledHFFactors, DroneFactors, IsEscort, IsScout, IsTender, IsFast, IsTug, IsCommando, IsMauler FROM ShipClass";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            List<ShipClass> shipClasses = new List<ShipClass>();
            int id, offense, defense, cOffense, cDefense, command, dateAvailable, df;
            float salvage, cost;
            float ff, cff, hff, chff;
            ShipRoles roles;
            Factions faction;
            string designation;
            int maxId = 0;
            while (reader.Read())
            {
                roles = ShipRoles.None;
                id = reader.GetInt32(0);
                faction = (Factions)(reader.GetInt32(1));
                designation = reader.GetString(2);
                offense = reader.GetInt32(3);
                defense = reader.GetInt32(4);
                cOffense = reader.GetInt32(5);
                cDefense = reader.GetInt32(6);
                command = reader.GetInt32(7);
                dateAvailable = reader.GetInt32(8);
                salvage = reader.GetFloat(9);
                cost = reader.GetFloat(10);
                
                ff = reader.IsDBNull(11) ? 0 : reader.GetFloat(11);
                cff = reader.IsDBNull(12) ? 0 : reader.GetFloat(12);
                hff = reader.IsDBNull(13) ? 0 : reader.GetFloat(13);
                chff = reader.IsDBNull(14) ? 0 : reader.GetFloat(14);
                df = reader.IsDBNull(15) ? 0 : reader.GetInt32(15);

                if (!reader.IsDBNull(16))
                {
                    roles |= ShipRoles.Escort;
                }
                if(!reader.IsDBNull(17))
                {
                    roles |= ShipRoles.Scout;
                }
                if(!reader.IsDBNull(18))
                {
                    roles |= ShipRoles.PFTender;
                }
                if (!reader.IsDBNull(19))
                {
                    roles |= ShipRoles.Fast;
                }
                if (!reader.IsDBNull(20))
                {
                    roles |= ShipRoles.Tug;
                }
                if (!reader.IsDBNull(21))
                {
                    roles |= ShipRoles.Commando;
                }
                if (!reader.IsDBNull(22))
                {
                    roles |= ShipRoles.Mauler;
                }
                if (id > maxId)
                {
                    maxId = id;
                }
                shipClasses.Add(new ShipClass(id, faction, designation, offense, defense, cOffense, cDefense, command, dateAvailable, salvage, cost, ff, cff, hff, chff, df, roles));
            }
            _shipClasses = new ShipClass[maxId + 1];
            foreach (ShipClass shipClass in shipClasses)
            {
                _shipClasses[shipClass.Id] = shipClass;
            }
            reader.Close();
            dbcmd.Dispose();
        }

        private void PopulateFactions()
        {
            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT ID, Name, Red, Green, Blue FROM Faction";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            List<Faction> factions = new List<Faction>();
            int id;
            string name;
            float red;
            float green;
            float blue;
            int maxId = 0;
            while (reader.Read())
            {
                id = reader.GetInt32(0);
                name = reader.GetString(1);
                red = reader.GetFloat(2);
                green = reader.GetFloat(3);
                blue = reader.GetFloat(4);
                if (id > maxId)
                {
                    maxId = id;
                }
                factions.Add(new Faction((Factions)(id), new Color(red, green, blue), name));
            }
            _factions = new Faction[maxId + 1];
            foreach (Faction faction in factions)
            {
                _factions[(int)faction.Id] = faction;
            }
            reader.Close();
            dbcmd.Dispose();
        }

        private void PopulateConversions()
        {
            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT Id, FactionId, NewShipId, OriginalShipId, Cost, IsMinor FROM Conversion";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();

            _conversionMap = new Dictionary<Factions, List<Conversion>>();
            int newClassId, originalClassId;
            Factions faction;
            float cost;
            bool isMinor;

            while (reader.Read())
            {
                //id = reader.GetInt32(0);
                faction = (Factions)(reader.GetInt32(1));
                newClassId = reader.GetInt32(2);
                originalClassId = reader.GetInt32(3);
                cost = reader.GetFloat(4);
                isMinor = !reader.IsDBNull(5);

                if (!_conversionMap.ContainsKey(faction))
                {
                    _conversionMap[faction] = new List<Conversion>();
                }
                _conversionMap[faction].Add(new Conversion(_shipClasses[originalClassId], _shipClasses[newClassId], cost, isMinor));
            }
            reader.Close();
            dbcmd.Dispose();
        }

        private void PopulateSubstitutions()
        {
            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT Id, FactionId, ShipClassId, BaseShipClassId FROM Substitution";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();

            _substitutionMap = new Dictionary<Factions, List<Substitution>>();
            int newClassId, originalClassId;
            Factions faction;

            while (reader.Read())
            {
                //id = reader.GetInt32(0);
                faction = (Factions)(reader.GetInt32(1));
                newClassId = reader.GetInt32(2);
                originalClassId = reader.GetInt32(3);

                if (!_substitutionMap.ContainsKey(faction))
                {
                    _substitutionMap[faction] = new List<Substitution>();
                }
                _substitutionMap[faction].Add(new Substitution(_shipClasses[originalClassId], _shipClasses[newClassId]));
            }
            reader.Close();
            dbcmd.Dispose();
        }

        private void PopulateFactionConstructionPlans()
        {
            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT ConstructionScheduleID, ShipClassId FROM ConstructionScheduleShip";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();

            Dictionary<Factions, SortedList<int, List<ShipClass>>> factionSpringPlanMap = new Dictionary<Factions, SortedList<int, List<ShipClass>>>();
            Dictionary<Factions, SortedList<int, List<ShipClass>>> factionFallPlanMap = new Dictionary<Factions, SortedList<int, List<ShipClass>>>();
            Dictionary<int, List<ShipClass>> scheduleShipMap = new Dictionary<int, List<ShipClass>>();
            int id, classId;
            Factions faction;
            bool isSpring;
            int startTurn;

            while (reader.Read())
            {
                id = reader.GetInt32(0);
                classId = reader.GetInt32(1);
                if (!scheduleShipMap.ContainsKey(id))
                {
                    scheduleShipMap[id] = new List<ShipClass>();
                }
                scheduleShipMap[id].Add(_shipClasses[classId]);
            }
            reader.Close();
            dbcmd.Dispose();


            dbcmd = _dbConn.CreateCommand();
            sqlQuery = "SELECT ID, FactionId, IsSpring, StartTurn FROM ConstructionSchedule";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();

            while (reader.Read())
            {
                id = reader.GetInt32(0);
                faction = (Factions)(reader.GetInt32(1));
                isSpring = !reader.IsDBNull(2);
                startTurn = reader.GetInt32(3);
                if (isSpring)
                {
                    if (!factionSpringPlanMap.ContainsKey(faction))
                    {
                        factionSpringPlanMap[faction] = new SortedList<int, List<ShipClass>>();
                    }
                    factionSpringPlanMap[faction].Add(startTurn, scheduleShipMap[id]);
                }
                else
                {
                    if (!factionFallPlanMap.ContainsKey(faction))
                    {
                        factionFallPlanMap[faction] = new SortedList<int, List<ShipClass>>();
                    }
                    factionFallPlanMap[faction].Add(startTurn, scheduleShipMap[id]);
                }
            }
            reader.Close();
            dbcmd.Dispose();

            foreach(KeyValuePair<Factions, SortedList<int, List<ShipClass>>> factionPlan in factionSpringPlanMap)
            {
                _factions[(int)factionPlan.Key].Shipyard = 
                    new Shipyard(new ConstructionPlan(factionPlan.Value, factionFallPlanMap[factionPlan.Key]), 
                    _conversionMap[factionPlan.Key], _substitutionMap[factionPlan.Key]);
            }
        }

        private void PopulateMapData()
        {
            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT x, y, OriginalFactionId FROM Hex";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            List<MapNode> mapDataList = new List<MapNode>();
            int maxX = 0;
            int maxY = 0;
            while (reader.Read())
            {
                int x = reader.GetInt32(0);
                int y = reader.GetInt32(1);
                Factions originalFaction = (Factions)(reader.GetInt32(2));
                if(x > maxX)
                {
                    maxX = x;
                }
                if(y > maxY)
                {
                    maxY = y;
                }
                mapDataList.Add(new MapNode
                {
                    OriginalOwner = originalFaction,
                    Coordinates = new IntVector2 { x = x, y = y }
                });
            }
            _mapNodes = new MapNode[maxX + 1, maxY + 1];
            foreach(MapNode mapNode in mapDataList)
            {
                _mapNodes[mapNode.Coordinates.x, mapNode.Coordinates.y] = mapNode;
            }
            _factionMapMap = mapDataList.GroupBy(n => n.OriginalOwner).ToDictionary(g => g.Key, g => g.ToList());
            reader.Close();
            dbcmd.Dispose();
        }

        private void PopulateProvinces()
        {
            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT ProvinceId, HexId " + "FROM Province";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            _provinceHexMap = new int[_mapNodes.GetLength(0), _mapNodes.GetLength(1)];
            int maxProvinceId = 0;
            while (reader.Read())
            {
                int provinceId = reader.GetInt32(0);
                int hexId = reader.GetInt32(1);
                int x = hexId / 100;
                int y = hexId % 100;
                _provinceHexMap[x,y] = provinceId;
                if(provinceId > maxProvinceId)
                {
                    maxProvinceId = provinceId;
                }
            }
            var provinces = new List<MapNode>[maxProvinceId + 1];
            for(int i = 1; i < _provinceHexMap.GetLength(0); i++)
            {
                for(int j = 1; j < _provinceHexMap.GetLength(1); j++)
                {

                    int provinceId = _provinceHexMap[i, j];
                    if(provinceId > 0)
                    {
                        if(provinces[provinceId] == null)
                        {
                            provinces[provinceId] = new List<MapNode>();
                        }
                        provinces[provinceId].Add(_mapNodes[i, j]);
                    }
                }
            }
            _provinces = new Province[maxProvinceId + 1];
            for(int i = 1; i < maxProvinceId; i++)
            {
                _provinces[i] = new Province(provinces[i]);
            }

            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
        }

        private void PopulateBases()
        {
            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT HexId, FactionId, BaseType FROM Base WHERE SystemId IS NULL";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            List<Faction> factions = new List<Faction>();
            int hexId;
            Factions faction;
            BaseType baseType;
            while (reader.Read())
            {
                hexId = reader.GetInt32(0);
                int x = hexId / 100;
                int y = hexId % 100;
                faction = (Factions)(reader.GetInt32(1));
                baseType = (BaseType)(reader.GetInt32(2));

                if(_mapNodes[x,y].Bases == null)
                {
                    _mapNodes[x, y].Bases = new List<Base>();
                }
                Base newBase = new Base(baseType, faction, x, y);
                _mapNodes[x, y].Bases.Add(newBase);
                _factions[(int)faction].Bases.Add(newBase);
            }
            reader.Close();
            dbcmd.Dispose();
        }

        private void PopulatePlanets()
        {
            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT HexId, PlanetSize, OwningFactionId, CaptureTurn, DevestationTurn, OriginalFactionId FROM Planet WHERE SystemId IS NULL";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int hexId, captureTurn, devestationTurn;
            Factions faction, originalFaction;
            PlanetType planetType;
            while (reader.Read())
            {
                hexId = reader.GetInt32(0);
                int x = hexId / 100;
                int y = hexId % 100;
                planetType = (PlanetType)(reader.GetInt32(1));
                faction = (Factions)(reader.GetInt32(2));
                captureTurn = reader.GetInt32(3);
                devestationTurn = reader.GetInt32(4);
                originalFaction = (Factions)(reader.GetInt32(5));

                if (_mapNodes[x, y].Planets == null)
                {
                    _mapNodes[x, y].Planets = new List<Planet>();
                }
                _mapNodes[x, y].Planets.Add(new Planet(planetType, faction, originalFaction, captureTurn, devestationTurn));
            }
            reader.Close();
            dbcmd.Dispose();
        }

        private void PopulateCapitals()
        {
            Dictionary<int, Capital> capitalMap = new Dictionary<int, Capital>();
            Dictionary<int, List<Models.System>> sysCapMap = new Dictionary<int, List<Models.System>>();
            Dictionary<int, Models.System> systemMap = new Dictionary<int, Models.System>();
            Dictionary<int, List<Planet>> planetSystemMap = new Dictionary<int, List<Planet>>();
            Dictionary<int, List<Base>> baseSystemMap = new Dictionary<int, List<Base>>();
            int id, hexId, capitalId, captureTurn, devestationTurn;
            Factions faction, originalFaction;
            PlanetType planetType;
            string name;

            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT Id, HexId FROM Capital";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
                hexId = reader.GetInt32(1);
                int x = hexId / 100;
                int y = hexId % 100;
                capitalMap[id] = new Capital();
                _mapNodes[x, y].Capital = capitalMap[id];
            }
            reader.Close();
            dbcmd.Dispose();

            dbcmd = _dbConn.CreateCommand();
            sqlQuery = "SELECT Id, CapitalId, Name FROM System";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
                capitalId = reader.GetInt32(1);
                name = reader.GetString(2);
                Models.System sys = new Models.System
                {
                    Name = name,
                };
                if(!sysCapMap.ContainsKey(capitalId))
                {
                    sysCapMap[capitalId] = new List<Models.System>();
                }
                sysCapMap[capitalId].Add(sys);
                systemMap[id] = sys;
            }
            reader.Close();
            dbcmd.Dispose();

            foreach(KeyValuePair<int, Capital> capital in capitalMap)
            {
                capital.Value.CapitalSystems = sysCapMap[capital.Key].ToArray();
            }

            dbcmd = _dbConn.CreateCommand();
            sqlQuery = "SELECT Id, PlanetSize, OwningFactionId, CaptureTurn, DevestationTurn, OriginalFactionId, SystemId FROM Planet WHERE SystemId IS NOT NULL ORDER BY Id, SystemId";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                planetType = (PlanetType)(reader.GetInt32(1));
                faction = (Factions)(reader.GetInt32(2));
                captureTurn = reader.GetInt32(3);
                devestationTurn = reader.GetInt32(4);
                originalFaction = (Factions)(reader.GetInt32(5));
                int systemId = reader.GetInt32(6);
                if(!planetSystemMap.ContainsKey(systemId))
                {
                    planetSystemMap[systemId] = new List<Planet>();
                }
                planetSystemMap[systemId].Add(new Planet(planetType, faction, originalFaction, captureTurn, devestationTurn));
            }
            reader.Close();
            dbcmd.Dispose();

            foreach(KeyValuePair<int, List<Planet>> planetKeyValue in planetSystemMap)
            {
                systemMap[planetKeyValue.Key].Planets = planetKeyValue.Value.ToArray();
            }

            dbcmd = _dbConn.CreateCommand();
            sqlQuery = "SELECT Id, FactionId, BaseType, SystemId FROM Base WHERE SystemId IS NOT NULL ORDER BY Id, SystemId";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                faction = (Factions)(reader.GetInt32(1));
                BaseType baseType = (BaseType)(reader.GetInt32(2));
                int systemId = reader.GetInt32(3);
                if (!baseSystemMap.ContainsKey(systemId))
                {
                    baseSystemMap[systemId] = new List<Base>();
                }
                baseSystemMap[systemId].Add(new Base(baseType, faction, systemId / 100, systemId % 100));
            }
            reader.Close();
            dbcmd.Dispose();

            foreach (KeyValuePair<int, List<Base>> baseKeyValue in baseSystemMap)
            {
                systemMap[baseKeyValue.Key].Bases = baseKeyValue.Value.ToArray();
            }
        }

        private void PopulateOffMapAreas()
        {
            _offMapAreaMap = new Dictionary<Factions, OffMap>();
            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT FactionId, Provinces, MinorPlanets, MajorPlanets, Starbases FROM OffMap";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                Factions faction = (Factions)(reader.GetInt32(0));
                int provinces = reader.GetInt32(1);
                int minor = reader.GetInt32(2);
                int major = reader.GetInt32(3);
                int sbs = reader.GetInt32(4);
                _offMapAreaMap[faction] = new OffMap(provinces, minor, major, sbs);
            }
            reader.Close();
            dbcmd.Dispose();
        }

        private void PopulateShipData()
        {
            // get ship data
            IDbCommand dbcmd = _dbConn.CreateCommand();
            string sqlQuery = "SELECT Id, FactionId, ShipClassId, HullNumber, Name, IsCrippled FROM Ship";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            List<Ship> shipList = new List<Ship>();
            //Dictionary<Factions, List<Ship>> factionShipMap = new Dictionary<Factions, List<Ship>>();
            int id, shipClassId, hexId;
            string hullNum, name;
            Factions faction;
            bool isCrippled;
            int maxId = 0;
            while (reader.Read())
            {
                id = reader.GetInt32(0);
                if(id > maxId)
                {
                    maxId = id;
                }
                faction = (Factions)(reader.GetInt32(1));
                shipClassId = reader.GetInt32(2);
                hullNum = reader.IsDBNull(3) ? null : reader.GetString(3);
                name = reader.IsDBNull(4) ? null : reader.GetString(4);
                isCrippled = !reader.IsDBNull(5);
                Ship newShip = new Ship(id, _shipClasses[shipClassId], _shipClasses[shipClassId].Faction, hullNum, name, isCrippled);
                _factions[(int)faction].AddShip(newShip);
                shipList.Add(newShip);
            }
            _ships = new Ship[maxId + 1];
            foreach(Ship ship in shipList)
            {
                _ships[ship.Id] = ship;
            }
            reader.Close();
            dbcmd.Dispose();

            // get fleet hex data
            dbcmd = _dbConn.CreateCommand();
            sqlQuery = "SELECT FleetId, HexId FROM FleetSetupHex";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            Dictionary<int, List<MapNode>> fleetHexMap = new Dictionary<int, List<MapNode>>();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
                hexId = reader.GetInt32(1);
                if(!fleetHexMap.ContainsKey(id))
                {
                    fleetHexMap[id] = new List<MapNode>();
                }
                int x = hexId / 100;
                int y = hexId % 100;
                fleetHexMap[id].Add(_mapNodes[x, y]);
            }
            reader.Close();
            dbcmd.Dispose();

            // get fleet data
            dbcmd = _dbConn.CreateCommand();
            sqlQuery = "SELECT Id, FactionId, Name FROM Fleet";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            List<Fleet> fleetList = new List<Fleet>();
            maxId = 0;
            while (reader.Read())
            {
                id = reader.GetInt32(0);
                if (id > maxId)
                {
                    maxId = id;
                }
                faction = (Factions)(reader.GetInt32(1));
                name = reader.GetString(2);
                Fleet fleet = new Fleet(id, name, fleetHexMap.ContainsKey(id) ? fleetHexMap[id] : null);
                _factions[(int)faction].AddFleet(fleet);
                fleetList.Add(fleet);
            }
            _fleets = new Fleet[maxId + 1];
            foreach(Fleet fleet in fleetList)
            {
                _fleets[fleet.Id] = fleet;
            }
            reader.Close();
            dbcmd.Dispose();

            // get ship fleet data
            dbcmd = _dbConn.CreateCommand();
            sqlQuery = "SELECT ShipId, FleetId FROM ShipFleet";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
                Fleet fleet = _fleets[reader.GetInt32(1)];
                fleet.Ships.Add(_ships[id]);
            }
            reader.Close();
            dbcmd.Dispose();
            // get ship hex data

        }
    }
}
