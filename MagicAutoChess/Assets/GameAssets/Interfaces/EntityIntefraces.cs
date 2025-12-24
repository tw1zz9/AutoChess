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
    public interface IDamager : ICharacter
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
    public interface IHealer : ICharacter
    {
        double HealPower { get; }
        /// <summary>
        /// Этим методом его нельзя воскресить, только увеличить ненулевое здоровье.
        /// </summary>
        /// <param name="unit">Юнит, которому восстанавливается здоровье</param>
        void Heal(ICharacter unit);
    }

    /// <summary>
    /// Интерфейс, который реализуется персонажами, у которых есть особенные способности.
    /// </summary>
    public interface IUtilitable
    {
        void SpecialUtility(int _numberOfTeammaetes);
    }

    /// <summary>
    /// Интерфейс, который создаёт описание персонажа для отображения информации о нём.
    /// </summary>
    public interface IInformational
    {
        string Description();
    }

}