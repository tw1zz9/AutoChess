using GameAssets.Interfaces;

namespace GameAssets.Field
{
    /// <summary>
    /// Клетка игрового поля
    /// </summary>
    public class Cell : ICellable
    {
        public int X { get; }
        public int Y { get; }
        public Team Team { get; }

        public ICharacter ExistingCharacter { get; private set; }

        public Cell(int x, int y, Team team)
        {
            X = x;
            Y = y;
            Team = team;
        }

        public void SetCharacter(ICharacter character)
        {
            if (character.Team != Team) return; // Нельзя размещать юнитов противника
            ExistingCharacter = character;
        }

        public void RemoveCharacter()
        {
            ExistingCharacter = null;
        }

        public bool IsOccupied()
        {
            return ExistingCharacter != null;
        }

        public override string ToString()
        {
            return $"Cell ({X},{Y}) Team: {Team} Occupied: {IsOccupied()}";
        }
    }
}
