namespace RayLibAutoChess
{
    public class Cell : ICellable
    {
        public int X { get; }
        public int Y { get; }
        public ICharacter? ExistingCharacter { get; private set; }

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void SetCharacter(ICharacter character)
        {
            ExistingCharacter = character;
        }

        public void RemoveCharacter()
        {
            ExistingCharacter = null;
        }
    }
}
