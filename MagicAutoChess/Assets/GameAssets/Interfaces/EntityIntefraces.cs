using System;

namespace GameAssets.Interfaces
{
    /// <summary>
    /// Интерфейс, описывающий персонажа, способного наносить урон другим.
    /// </summary>
    public interface IDamager
    {
        double Damage { get; }
        void PerformAttack();
    }

    /// <summary>
    /// Интерфейс персонажа, способного лечить союзников.
    /// Healer выбирает союзника для лечения через SelectTarget, а затем вызывает Heal().
    /// </summary>
    public interface IHealer
    {
        /// <summary>
        /// Сила исцеления
        /// </summary>
        double HealPower { get; }

        /// <summary>
        /// Выбирает союзника для лечения
        /// </summary>
        /// <param name="ally">Союзный персонаж, которого нужно лечить</param>
        void SelectTarget(ICharacter ally);

        /// <summary>
        /// Лечит выбранного союзника (или себя). Нельзя воскресить мертвого.
        /// </summary>
        void Heal();
    }

    /// <summary>
    /// Интерфейс, который описывает персонажа, способного уклониться от атаки с каким-то шансом.
    /// </summary>
    public interface IEvading
    {
        /// <summary>
        /// Поле, хранящее данные о шансе уклонения (0%-100%)
        /// </summary>
        double DodgeChance { get; }
        /// <summary>
        /// Метод уклонения.
        /// </summary>
        bool Dodge();
    }

    /// <summary>
    /// Интерфейс, который создаёт описание персонажа для отображения информации о нём.
    /// </summary>
    public interface IInformational
    {
        string Description();
    }
}