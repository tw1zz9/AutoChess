using System;

namespace RayLibAutoChess
{
    public static class EventManager
    {
        public static event Action<AttackContext>? OnBeforeAttack;
        public static event Action<AttackContext>? OnAfterAttack;

        public static void InvokeBeforeAttack(AttackContext context)
        {
            OnBeforeAttack?.Invoke(context);
        }

        public static void InvokeAfterAttack(AttackContext context)
        {
            OnAfterAttack?.Invoke(context);
        }

        public static void Clear()
        {
            OnBeforeAttack = null;
            OnAfterAttack = null;
        }
    }
}
