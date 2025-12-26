namespace RayLibAutoChess.SaveSystem
{
    public class UnitState
    {
        public string UnitType { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public Team Team { get; set; }
        public double Health { get; set; }
        public double Armor { get; set; }
        public int Level { get; set; }

        // Specific unit properties
        public double Damage { get; set; }
        public double HealPower { get; set; }
        public double DodgeChance { get; set; }
        public double BuffMultiplier { get; set; }
        public double ShieldAmount { get; set; }
        public double AreaHealPower { get; set; }
        public bool IsUltimateActive { get; set; }

        public UnitState() { }

        public UnitState(ICharacter unit)
        {
            Id = unit.ID;
            Team = unit.Team;
            Health = unit.Health;
            Armor = unit.Armor;
            Level = unit.Level;

            // Determine unit type and save specific properties
            if (unit is Entities.Mage mage)
            {
                UnitType = "Mage";
                Damage = mage.Damage;
                BuffMultiplier = mage.BuffMultiplier;
                IsUltimateActive = mage.IsUltimateActive;
            }
            else if (unit is Entities.Tank tank)
            {
                UnitType = "Tank";
                Damage = tank.Damage;
                ShieldAmount = tank.ShieldAmount;
                IsUltimateActive = tank.IsUltimateActive;
            }
            else if (unit is Entities.Trickster trickster)
            {
                UnitType = "Trickster";
                Damage = trickster.Damage;
                DodgeChance = trickster.DodgeChance;
                IsUltimateActive = trickster.IsUltimateActive;
            }
            else if (unit is Entities.Healer healer)
            {
                UnitType = "Healer";
                HealPower = healer.HealPower;
                AreaHealPower = healer.AreaHealPower;
                IsUltimateActive = healer.IsUltimateActive;
            }
        }

        public ICharacter ToUnit()
        {
            ICharacter unit;
            switch (UnitType)
            {
                case "Mage":
                    unit = new Entities.Mage(Team);
                    break;
                case "Tank":
                    unit = new Entities.Tank(Team);
                    break;
                case "Trickster":
                    unit = new Entities.Trickster(Team);
                    break;
                case "Healer":
                    unit = new Entities.Healer(Team);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown unit type: {UnitType}");
            }

            // Apply saved state
            // Note: In a full implementation, we'd need to expose setters or use reflection
            // For now, we'll create units with default stats and apply levels
            for (int i = 1; i < Level; i++)
            {
                unit.LevelUp();
            }

            return unit;
        }
    }
}
