using System;
using GameAssets.Combat;

namespace GameAssets.Events
{
    public static class EventManager //класс для того, чтобы подпиисываться на активки персонажей. (одноврмененный вызов еффектов).
    {
        public static event Action<AttackContext> OnBeforeAttack;

        public static void InvokeBeforeAttack(AttackContext context)
        {
            OnBeforeAttack?.Invoke(context);
        }
    }
}
