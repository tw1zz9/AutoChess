namespace RayLibAutoChess
{
    public static class UnitFactory
    {
        public static ICharacter CreateTrickster(Team team) => new Entities.Trickster(team);
        public static ICharacter CreateTank(Team team) => new Entities.Tank(team);
        public static ICharacter CreateMage(Team team) => new Entities.Mage(team);
        public static ICharacter CreateHealer(Team team) => new Entities.Healer(team);
    }
}
