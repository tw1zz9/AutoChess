namespace RayLibAutoChess
{
    public class AttackContext
    {
        public ICharacter Attacker { get; }
        public ICharacter Target { get; }
        public double Damage { get; set; }

        public AttackContext(ICharacter attacker, ICharacter target, double damage)
        {
            Attacker = attacker ?? throw new ArgumentNullException(nameof(attacker));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Damage = damage;
        }
    }
}
