using GameAssets.Interfaces.Effects;
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
        /// Поле, хранящее данные о шансе уклонения
        /// </summary>
        double DodgeChance { get; }
        /// <summary>
        /// Действие, которое происходит прямо перед атакой по персонажу.
        /// </summary>
        Action<DamageContext> OnBeforeDamage { get; set; }
    }


    /// <summary>
    /// Интерфейс, который создаёт описание персонажа для отображения информации о нём.
    /// </summary>
    public interface IInformational
    {
        string Description();
    }

}