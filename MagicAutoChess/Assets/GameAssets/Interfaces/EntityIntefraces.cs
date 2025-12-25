using System;

namespace GameAssets.Interfaces
{

    /// <summary>
    /// Предполагаемый для использования интерфейс, представляющий собой отряд игрока. 
    /// </summary>    
    public interface IPlayersHand
    {
        void AddUnit(ICharacter unit);
        void RemoveUnit(ICharacter unit);
        void RearrangeUnits();
    }
    /// <summary>
    /// Интерфейс, описывающий персонажа, способного наносить урон другим.
    /// </summary>
    public interface IDamager
    {
        double Damage { get; }
        /// <summary>
        /// Метод нанесения урона.
        /// </summary>
        /// <param name="target">Юнит, которому снижается здоровье</param>
        void Fight(ICharacter target);
    }

    /// <summary>
    /// Интерфейс, описывающий персонажа, способного восстанавливать здоровье союзникам.
    /// </summary>
    public interface IHealer
    {
        double HealPower { get; }
        /// <summary>
        /// Этим методом его нельзя воскресить, только увеличить ненулевое здоровье.
        /// </summary>
        /// <param name="unit">Юнит, которому восстанавливается здоровье</param>
        void Heal(ICharacter unit);
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

    public interface IDefenging
    {
        /// <summary>
        /// Поле, хранящее данные о проценте перенаправленного урона от союзников.
        /// </summary>
        double ReflectPower { get; }
        /// <summary>
        /// Метод установки защиты конкретному персонажу.
        /// </summary>
        /// <param name="ally">Союзник танка</param>
        void TauntAndProtect(ICharacter ally);
        /// <summary>
        /// Метод, который вызывается в конце хода (когда весь необходимый урон поглощён танком)
        /// </summary>
        /// <param name="ally">Союзник танка</param>
        void StopProtecting(ICharacter ally);
    }


    /// <summary>
    /// Интерфейс, который создаёт описание персонажа для отображения информации о нём.
    /// </summary>
    public interface IInformational
    {
        string Description();
    }

}