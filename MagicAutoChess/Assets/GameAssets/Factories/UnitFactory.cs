using GameAssets.Entities;
using GameAssets.Interfaces;

namespace GameAssets.Factories
{
    public static class UnitFactory
    {
        public static ICharacter CreateMage(Team team) => new Mage(team);
        public static ICharacter CreateTank(Team team) => new Tank(team);
        public static ICharacter CreateHealer(Team team) => new Healer(team);
        public static ICharacter CreateTrickster(Team team) => new Trickster(team);
    }
}
