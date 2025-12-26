using System.Collections.Generic;
using System.Linq;

namespace RayLibAutoChess.SaveSystem
{
    public class PlayerState
    {
        public int Gold { get; set; }
        public Team Team { get; set; }
        public List<UnitState> Units { get; set; } = new();

        public PlayerState() { }

        public PlayerState(PlayersInventory inventory, Team team)
        {
            Gold = inventory.Gold;
            Team = team;
            Units = inventory.GetAllUnits().Select(u => new UnitState(u)).ToList();
        }
    }
}
