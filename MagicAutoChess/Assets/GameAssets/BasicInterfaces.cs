namespace GameAssets
{
    /// <summary>
    /// »нтерфейс, который описывает клетку, в которой может находитс€ персонаж.
    /// </summary>
    public interface ICellable
    {
        bool IsOccupied() => ExistingCharacter == null ? false: true;
        ICharacter? ExistingCharacter { get; set; }
    }
    /// <summary>
    /// —тандартный интерфейс, описывающий 
    /// обычного персонажа без способностей
    /// </summary>
    public interface ICharacter
    {
        void LevelUp();
        double Health { get; set; }
        double Armor { get; set; }
        int Level { get; }
        bool IsAlive();
    }


}
