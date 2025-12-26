using RayLibAutoChess.Entities;

namespace RayLibAutoChess
{
    public static class UnitFactory
    {
        public static ICharacter CreateTrickster(Team team) => new Trickster(team);
        public static ICharacter CreateTank(Team team) => new Tank(team);
        public static ICharacter CreateMage(Team team) => new Mage(team);
        public static ICharacter CreateHealer(Team team) => new Healer(team);
    }
}
