using System;
using System.Transactions;

namespace GameAssets.Interfaces.Effects
{
    public class DamageContext
    {
        public double Amount;
        public bool IsDamageCancelled;
    }

    public class EvasionEffect
    {
        private IEvading _unit;
        private double _chance;
        
        public EvasionEffect(IEvading unit)
        {
            _unit = unit;
            _chance = unit.DodgeChance;
            _unit.OnBeforeDamage += EvasionEffect_OnBeforeDamage;
        }

        private void EvasionEffect_OnBeforeDamage(DamageContext query)
        {
            var random = new Random();
            var probability = random.NextDouble();
            if (probability < _chance) 
            {
                query.IsDamageCancelled = true;
                return;
            }
            query.IsDamageCancelled = false;
        }

        public void Dispose()
        {
            _unit.OnBeforeDamage -= EvasionEffect_OnBeforeDamage;
        }
    }

}