using System;
using GameAssets.Combat;

namespace GameAssets.Events
{
    public static class EventManager
    {
        public static event Action<AttackContext> OnBeforeAttack;

        public static void InvokeBeforeAttack(AttackContext context)
        {
            OnBeforeAttack?.Invoke(context);
        }
    }
}
