using System;
using System.Collections.Generic;

namespace GameAssets.Interfaces
{
    public interface ICellable
    {
        ICharacter ExistingCharacter { get; }
        bool IsOccupied() => ExistingCharacter == null ? false: true;
        void SetCharacter(ICharacter character);
        void RemoveCharacter();
    }

    public interface ICharacter
    {
        Guid ID { get; }
        Team Team { get; }
        double Health { get; }
        double Armor { get; }
        int Level { get; }

        void LevelUp();
        bool IsAlive();
        void TakeDamage(double damage);
    }

    public interface IPlayersInventory
    {
        void AddUnits(IEnumerable<ICharacter> unit); 
        void RemoveUnits(IEnumerable<ICharacter> unit); 
        int Gold { get; }
    }
}
