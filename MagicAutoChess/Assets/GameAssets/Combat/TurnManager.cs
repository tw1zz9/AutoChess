using System;
using System.Collections.Generic;
using System.Linq;
using GameAssets.Events;
using GameAssets.Interfaces;
using GameAssets;
using GameAssets.Entities;
using UnityEngine;

namespace GameAssets.Combat
{
    public class TurnManager //класс отвечающий за логику хода
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
            {
                
                Debug.Log("TurnManager.SetReady called - use GameManager.ResolveBattle instead");
            }
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

        /// <summary>
        /// Выполнить бой с предоставленными юнитами
        /// </summary>
        public void ResolveBattle(IEnumerable<ICharacter> blueUnits, IEnumerable<ICharacter> redUnits)
        {
            // В Auto Chess бой длится несколько "раундов" атаки
            const int COMBAT_ROUNDS = 3; // 3 раунда атаки за бой

            var allUnits = blueUnits.Concat(redUnits);

            // Активируем ультимейты один раз в начале боя
            ApplyActiveUltimateEffects(allUnits);

            for (int combatRound = 0; combatRound < COMBAT_ROUNDS; combatRound++)
            {
                Debug.Log($"Combat Round {combatRound + 1}/{COMBAT_ROUNDS}");

                // Активируем однораундовые эффекты ультимейтов в начале каждого раунда
                ApplyOneTurnUltimateEffects(allUnits);

                // Сбрасываем однораундовые эффекты от предыдущего раунда
                foreach (var effect in _oneTurnEffects)
                    EventManager.OnBeforeAttack -= effect;
                _oneTurnEffects.Clear();

                // Автоматически выбираем цели для атакующих без цели
                AutoSelectTargets(blueUnits, redUnits);

                // Все атакующие выполняют атаку
                foreach (var attacker in _attackers)
                    attacker.PerformAttack();

                // Все хилеры лечат
                foreach (var healer in _healers)
                {
                    healer.Heal();
                    // Применяем массовое лечение для ультимейтов (если активировано)
                    if (healer is Entities.Healer healerEntity && healerEntity.IsUltimateActive)
                    {
                        var allAllies = GetAlliesForHealer(healer, allUnits);
                        (healer as IHealer).HealAll(allAllies);
                    }
                }

                // Проверяем, остались ли живые юниты у обеих команд
                var currentBlueUnits = blueUnits.Where(u => u.IsAlive());
                var currentRedUnits = redUnits.Where(u => u.IsAlive());

                if (!currentBlueUnits.Any() || !currentRedUnits.Any())
                {
                    Debug.Log($"Combat ended early in round {combatRound + 1}");
                    break;
                }
            }

            // Сбрасываем эффекты одного хода (последний раз)
            foreach (var effect in _oneTurnEffects)
                EventManager.OnBeforeAttack -= effect;

            _oneTurnEffects.Clear();
            _attackers.Clear();
            _healers.Clear();

            // Сбрасываем временные эффекты и ультимейты
            ResetTemporaryEffects(allUnits);
            ResetUltimateStates(allUnits);

            _player1Ready = false;
            _player2Ready = false;
        }

        private void AutoSelectTargets(IEnumerable<ICharacter> blueUnits, IEnumerable<ICharacter> redUnits)
        {
            // Получаем всех живых юнитов
            var liveBlueUnits = blueUnits.Where(u => u.IsAlive()).ToList();
            var liveRedUnits = redUnits.Where(u => u.IsAlive()).ToList();

            // Для каждого атакующего без цели выбираем случайную цель из вражеской команды
            foreach (var attacker in _attackers)
            {
                var character = (ICharacter)attacker;
                if (character is IDamager damager)
                {
                    // Проверяем, есть ли выбранная цель и жива ли она
                    var selectedTarget = GetSelectedTarget(damager);
                    if (selectedTarget == null || !selectedTarget.IsAlive())
                    {
                        // Выбираем случайную цель из вражеской команды
                        var enemyUnits = character.Team == Team.Blue ? liveRedUnits : liveBlueUnits;
                        if (enemyUnits.Any())
                        {
                            var randomTarget = enemyUnits[UnityEngine.Random.Range(0, enemyUnits.Count)];
                            // SelectTarget определен в конкретных классах
                            if (damager is Entities.Tank tank) tank.SelectTarget(randomTarget);
                            else if (damager is Entities.Mage mage) mage.SelectTarget(randomTarget);
                            else if (damager is Entities.Trickster trickster) trickster.SelectTarget(randomTarget);
                        }
                    }
                }
            }

            // Для хилеров выбираем союзников
            foreach (var healer in _healers)
            {
                var character = (ICharacter)healer;
                var selectedTarget = GetSelectedTarget(healer);
                if (selectedTarget == null || !selectedTarget.IsAlive())
                {
                    // Выбираем случайного союзника
                    var allyUnits = character.Team == Team.Blue ? liveBlueUnits : liveRedUnits;
                    var damagedAllies = allyUnits.Where(u => u.Health < u.Health * 0.8).ToList(); // Раненые союзники
                    if (damagedAllies.Any())
                    {
                        var randomTarget = damagedAllies[UnityEngine.Random.Range(0, damagedAllies.Count)];
                        healer.SelectTarget(randomTarget);
                    }
                    else if (allyUnits.Any())
                    {
                        var randomTarget = allyUnits[UnityEngine.Random.Range(0, allyUnits.Count)];
                        healer.SelectTarget(randomTarget);
                    }
                }
            }
        }

        private IEnumerable<ICharacter> GetAlliesForHealer(IHealer healer, IEnumerable<ICharacter> allUnits)
        {
            // Получаем всех союзников хилера из предоставленного списка
            var healerCharacter = (ICharacter)healer;
            return allUnits.Where(u => u.Team == healerCharacter.Team && u.IsAlive());
        }

        public void ResetTemporaryEffects(IEnumerable<ICharacter> allUnits)
        {
            // Сбрасываем временные эффекты ультимейтов
            foreach (var unit in allUnits)
            {
                if (unit is Entities.Trickster trickster)
                {
                    trickster.ResetDodgeChance();
                }
            }
        }

        private void ApplyActiveUltimateEffects(IEnumerable<ICharacter> allUnits)
        {
            // Применяем постоянные эффекты ультимейтов (один раз за бой)
            foreach (var unit in allUnits)
            {
                if (unit is Entities.Trickster trickster && trickster.IsUltimateActive)
                {
                    // Trickster ультимейт - постоянный эффект на весь бой
                    trickster.UseUltimate(); // Устанавливает dodge chance = 1.0
                }
                // Healer ультимейт применяется в фазе лечения
            }
        }

        private void ApplyOneTurnUltimateEffects(IEnumerable<ICharacter> allUnits)
        {
            // Применяем однораундовые эффекты ультимейтов (каждый раунд)
            foreach (var unit in allUnits)
            {
                if (unit is Entities.Tank tank && tank.IsUltimateActive)
                {
                    tank.ActivateTaunt(this);
                }
                else if (unit is Entities.Mage mage && mage.IsUltimateActive)
                {
                    mage.ActivateDamageBuff(this);
                }
            }
        }

        private void ResetUltimateStates(IEnumerable<ICharacter> allUnits)
        {
            // Сбрасываем флаги активации ультимейтов после боя
            foreach (var unit in allUnits)
            {
                if (unit is Entities.Tank tank) tank.ResetUltimateState();
                else if (unit is Entities.Mage mage) mage.ResetUltimateState();
                else if (unit is Entities.Healer healer) healer.ResetUltimateState();
                else if (unit is Entities.Trickster trickster) trickster.ResetUltimateState();
            }
        }

        private ICharacter GetSelectedTarget(IDamager damager)
        {
            if (damager is Entities.Tank tank) return tank.GetSelectedTarget();
            if (damager is Entities.Mage mage) return mage.GetSelectedTarget();
            if (damager is Entities.Trickster trickster) return trickster.GetSelectedTarget();
            return null;
        }

        private ICharacter GetSelectedTarget(IHealer healer)
        {
            if (healer is Entities.Healer h) return h.GetSelectedTarget();
            return null;
        }
    }
}