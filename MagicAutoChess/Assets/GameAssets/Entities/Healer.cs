using GameAssets.Interfaces;

public class Healer : IHealer, IUtilitable, IInformational
{
    private readonly int _maximumObtainableLevel = 3;

    public string Name { get; private set; }
    public int HealPower { get; set; }
    public int Health { get; set; }
    public int Armor { get; set; }
    public int Level { get; private set; }

    public Healer()
    {
        Name = "Angel";
        HealPower = 100;
        Health = 400;
        Armor = 5;
        Level = 1;
    }

    void Heal(ICharacter unit)
    {
        unit.Health += HealPower;
    }

    public bool IsAlive()
    {
        if (Health > 0) return true;
        else
        {
            Name = "(Dead) " + Name;
            return false;
        }
    }

    public void LevelUp()
    {
        if (Level == _maximumObtainableLevel) return;

        var _multiplicator = 1.8;
        var _enhancedMultiplicator = 2;

        HealPower *= _multiplicator;
        Health *= _enhancedMultiplicator;
        Armor *= _enhancedMultiplicator;

        Level++;
    }
}
