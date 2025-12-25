using GameAssets.Interfaces;

namespace GameAssets.Combat
{
    public class AttackContext
    {
        public ICharacter Attacker { get; }
        public ICharacter Target { get; set; }
        public double Damage { get; set; }

        public AttackContext(ICharacter attacker, ICharacter target, double damage)
        {
            Attacker = attacker;
            Target = target;
            Damage = damage;
        }
    }
}
