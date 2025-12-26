using System;
using System.Collections.Generic;
using System.Linq;
using RayLibAutoChess.Entities;

namespace RayLibAutoChess
{
    public class TurnManager
    {
        private bool _player1Ready;
        private bool _player2Ready;

        private readonly List<Action<AttackContext>> _oneTurnEffects = new();
        private readonly List<IDamager> _attackers = new();
        private readonly List<IHealer> _healers = new();

        // Эффект таунта танка - все враги атакуют этот танк один ход
        private ICharacter? _tauntingTank;

        public void SetReady(int playerId)
        {
            if (playerId != 1 && playerId != 2)
                throw new ArgumentException("Player ID must be 1 or 2.", nameof(playerId));

            if (playerId == 1) _player1Ready = true;
            if (playerId == 2) _player2Ready = true;

            if (_player1Ready && _player2Ready)
            {
                Console.WriteLine("TurnManager.SetReady called - use GameManager.ResolveBattle instead");
            }
        }

        public void RegisterAttacker(IDamager damager)
        {
            if (damager == null)
                throw new ArgumentNullException(nameof(damager), "Damager cannot be null.");

            if (!_attackers.Contains(damager))
                _attackers.Add(damager);
        }

        public void RegisterHealer(IHealer healer)
        {
            if (healer == null)
                throw new ArgumentNullException(nameof(healer), "Healer cannot be null.");

            if (!_healers.Contains(healer))
                _healers.Add(healer);
        }

        public void RegisterOneTurnEffect(Action<AttackContext> effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect), "Effect cannot be null.");

            EventManager.OnBeforeAttack += effect;
            _oneTurnEffects.Add(effect);
        }

        public void SetTauntingTank(ICharacter tank)
        {
            if (tank == null)
                throw new ArgumentNullException(nameof(tank), "Tank cannot be null.");

            _tauntingTank = tank;
        }

        public void ResolveBattle(IEnumerable<ICharacter> blueUnits, IEnumerable<ICharacter> redUnits)
        {
            if (blueUnits == null)
                throw new ArgumentNullException(nameof(blueUnits), "Blue units collection cannot be null.");

            if (redUnits == null)
                throw new ArgumentNullException(nameof(redUnits), "Red units collection cannot be null.");

            const int COMBAT_ROUNDS = 3;

            var allUnits = blueUnits.Concat(redUnits);

            ApplyActiveUltimateEffects(allUnits);

            for (int combatRound = 0; combatRound < COMBAT_ROUNDS; combatRound++)
            {
                Console.WriteLine($"Combat Round {combatRound + 1}/{COMBAT_ROUNDS}");

                ApplyOneTurnUltimateEffects(allUnits);

                foreach (var effect in _oneTurnEffects)
                    EventManager.OnBeforeAttack -= effect;
                _oneTurnEffects.Clear();

                AutoSelectTargets(blueUnits, redUnits);

                foreach (var attacker in _attackers)
                    attacker.PerformAttack();

                foreach (var healer in _healers)
                {
                    healer.Heal();
                    if (healer is Healer healerEntity && healerEntity.IsUltimateActive)
                    {
                        var allAllies = GetAlliesForHealer(healer, allUnits);
                        // Ультимейт лечит всех союзников на 50% от обычной силы лечения
                        double ultimateHealAmount = healerEntity.HealPower * 0.5;
                        foreach (var ally in allAllies.Where(a => a.IsAlive()))
                        {
                            ally.Heal(ultimateHealAmount);
                        }
                        Console.WriteLine($"{healerEntity.Name} used Divine Light! Healed all allies for {ultimateHealAmount:F0} health each");
                    }
                }

                var currentBlueUnits = blueUnits.Where(u => u.IsAlive());
                var currentRedUnits = redUnits.Where(u => u.IsAlive());

                if (!currentBlueUnits.Any() || !currentRedUnits.Any())
                {
                    Console.WriteLine($"Combat ended early in round {combatRound + 1}");
                    break;
                }
            }

            foreach (var effect in _oneTurnEffects)
                EventManager.OnBeforeAttack -= effect;

            _oneTurnEffects.Clear();
            _attackers.Clear();
            _healers.Clear();

            // Сбрасываем эффект таунта танка
            _tauntingTank = null;

            ResetTemporaryEffects(allUnits);
            ResetUltimateStates(allUnits);

            _player1Ready = false;
            _player2Ready = false;
        }

        private void AutoSelectTargets(IEnumerable<ICharacter> blueUnits, IEnumerable<ICharacter> redUnits)
        {
            var liveBlueUnits = blueUnits.Where(u => u.IsAlive()).ToList();
            var liveRedUnits = redUnits.Where(u => u.IsAlive()).ToList();

            foreach (var attacker in _attackers)
            {
                var character = (ICharacter)attacker;
                if (character is IDamager damager)
                {
                    var selectedTarget = GetSelectedTarget(damager);
                    if (selectedTarget == null || !selectedTarget.IsAlive())
                    {
                        // Если есть таунтящий танк от команды противника, все атакующие выбирают его целью
                        if (_tauntingTank != null && _tauntingTank.IsAlive() && _tauntingTank.Team != character.Team)
                        {
                            character.SelectTarget(_tauntingTank);
                        }
                        else
                        {
                            var enemyUnits = character.Team == Team.Blue ? liveRedUnits : liveBlueUnits;
                            if (enemyUnits.Any())
                            {
                                var randomTarget = enemyUnits[new Random().Next(0, enemyUnits.Count)];
                                character.SelectTarget(randomTarget);
                            }
                        }
                    }
                }
            }

            foreach (var healer in _healers)
            {
                var character = (ICharacter)healer;
                var selectedTarget = GetSelectedTarget(healer);
                if (selectedTarget == null || !selectedTarget.IsAlive())
                {
                    var allyUnits = character.Team == Team.Blue ? liveBlueUnits : liveRedUnits;
                    var damagedAllies = allyUnits.Where(u => u.Health < GetMaxHealth(u)).ToList();
                    if (damagedAllies.Any())
                    {
                        var randomTarget = damagedAllies[new Random().Next(0, damagedAllies.Count)];
                        healer.SelectTarget(randomTarget);
                    }
                    else if (allyUnits.Any())
                    {
                        var randomTarget = allyUnits[new Random().Next(0, allyUnits.Count)];
                        healer.SelectTarget(randomTarget);
                    }
                }
            }
        }

        private double GetMaxHealth(ICharacter character)
        {
            // Простое приближение - в реальной реализации это было бы сложнее
            return character.Health * 1.5;
        }

        private IEnumerable<ICharacter> GetAlliesForHealer(IHealer healer, IEnumerable<ICharacter> allUnits)
        {
            var healerCharacter = (ICharacter)healer;
            return allUnits.Where(u => u.Team == healerCharacter.Team && u.IsAlive());
        }

        public void ResetTemporaryEffects(IEnumerable<ICharacter> allUnits)
        {
            if (allUnits == null)
                throw new ArgumentNullException(nameof(allUnits), "All units collection cannot be null.");

            foreach (var unit in allUnits)
            {
                if (unit is IEvading evading)
                {
                    if (unit is Trickster trickster)
                    {
                        trickster.ResetDodgeChance();
                    }
                }
            }
        }

        private void ApplyActiveUltimateEffects(IEnumerable<ICharacter> allUnits)
        {
            foreach (var unit in allUnits)
            {
                if (unit is IUltimateActivatable activatable)
                {
                    activatable.ApplyActiveUltimateEffect();
                }
            }
        }

        private void ApplyOneTurnUltimateEffects(IEnumerable<ICharacter> allUnits)
        {
            foreach (var unit in allUnits)
            {
                if (unit is IUltimateActivatable activatable)
                {
                    activatable.ApplyOneTurnUltimateEffect(this);
                }
            }
        }

        public void ResetUltimateStates(IEnumerable<ICharacter> allUnits)
        {
            if (allUnits == null)
                throw new ArgumentNullException(nameof(allUnits), "All units collection cannot be null.");

            foreach (var unit in allUnits)
            {
                if (unit is IUltimateResettable resettable)
                {
                    resettable.ResetUltimateState();
                }
            }
        }

        private ICharacter? GetSelectedTarget(IDamager damager)
        {
            if (damager is ITargetSelectable selectable)
            {
                return selectable.GetSelectedTarget();
            }
            return null;
        }

        private ICharacter? GetSelectedTarget(IHealer healer)
        {
            if (healer is ITargetSelectable selectable)
            {
                return selectable.GetSelectedTarget();
            }
            return null;
        }
    }
}
