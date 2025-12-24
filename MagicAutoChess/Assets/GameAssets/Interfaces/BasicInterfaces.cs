using System;

namespace GameAssets.Interfaces
{
    /// <summary>
    /// »нтерфейс, который описывает клетку, в которой может находитс€ персонаж.
    /// </summary>
    public interface ICellable
    {
        bool IsOccupied() => ExistingCharacter == null ? false: true;
        ICharacter ExistingCharacter { get; set; }
    }
    /// <summary>
    /// —тандартный интерфейс, описывающий 
    /// обычного персонажа без способностей
    /// </summary>
    public interface ICharacter
    {
        Guid ID { get; }
        void LevelUp();
        double Health { get; }
        double Armor { get; }
        int Level { get; }
        bool IsAlive();
        void TakeDamage(double damage);
    }
}
