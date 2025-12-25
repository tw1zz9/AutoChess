using System;
using System.Collections.Generic;
using GameAssets.Events;
using GameAssets.Interfaces;

namespace GameAssets.Combat
{
    public class TurnManager
    {
        private bool _player1Ready;
        private bool _player2Ready;

        private readonly List<Action<AttackContext>> _oneTurnEffects = new();
        private readonly List<IDamager> _attackers = new();
        private readonly List<IHealer> _healers = new();

        public void SetReady(int playerId)
        {
            if (playerId == 1) _player1Ready = true;
            if (playerId == 2) _player2Ready = true;

            if (_player1Ready && _player2Ready)
                ResolveBattle();
        }

        public void RegisterAttacker(IDamager damager)
        {
            if (!_attackers.Contains(damager))
                _attackers.Add(damager);
        }

        public void RegisterHealer(IHealer healer)
        {
            if (!_healers.Contains(healer))
                _healers.Add(healer);
        }

        public void RegisterOneTurnEffect(Action<AttackContext> effect)
        {
            EventManager.OnBeforeAttack += effect;
            _oneTurnEffects.Add(effect);
        }

        private void ResolveBattle()
        {
            // 1️⃣ Сначала применяем бафы и атаки
            foreach (var attacker in _attackers)
                attacker.PerformAttack();

            // 2️⃣ Потом лечим союзников
            foreach (var healer in _healers)
                healer.Heal();

            // 3️⃣ Сбрасываем эффекты одного хода
            foreach (var effect in _oneTurnEffects)
                EventManager.OnBeforeAttack -= effect;

            _oneTurnEffects.Clear();
            _attackers.Clear();
            _healers.Clear();

            _player1Ready = false;
            _player2Ready = false;
        }
    }
}
