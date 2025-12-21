
// Предполагаемый для использования интерфейс,
// представляющий собой отряд игрока.  
public interface IPlayersHand
{
    void AddUnit(ICharacter unit);
    void RemoveUnit(ICharacter unit);
    void RearrangeUnits();
}

// Стандартный интерфейс, описывающий 
// обычного персонажа без способностей
public interface ICharacter
{
    void LevelUp();
    double Health { get; set; }
    double Armor { get; set; }
    int Level { get; }
    bool IsAlive();
}

// Интерфейс, описывающий персонажа, 
// способного наносить урон другим
public interface IDamager : ICharacter
{
    double Damage { get; set; }
    void Fight(ICharacter target);
}

// Интерфейс, описывающий персонажа, 
// способного восстанавливать здоровье союзникам
public interface IHealer : ICharacter
{
    double HealPower { get; set; }
    void Heal(ICharacter unit);
}

// Интерфейс, который реализуется персонажами,
// у которых есть особенные способности
public interface IUtilitable
{
    void SpecialUtility();
}

// Интерфейс, который создаёт описание 
// персонажа для отображения информации о нём
public interface IInformational
{
    string Description();
}
