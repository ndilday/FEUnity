using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Models
{
    public class Fleet
    {
        public int Id { get; private set; }
        public List<Ship> Ships { get; private set; }
        public string Name { get; private set; }
        public List<MapNode> DeploymentHexes { get; private set; }
        public MapNode MapNode { get; set; }
        public Fleet(int id, string name, List<MapNode> deploymentHexes)
        {
            Id = id;
            Name = name;
            Ships = new List<Ship>();
            DeploymentHexes = deploymentHexes;
        }
    }
}
