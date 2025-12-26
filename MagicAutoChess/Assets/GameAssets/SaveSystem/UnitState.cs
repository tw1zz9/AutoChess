using System;
using GameAssets;
using GameAssets.Interfaces;

namespace GameAssets.SaveSystem
{
    /// <summary>
    /// Состояние юнита для сериализации
    /// </summary>
    [Serializable]
    public class UnitState
    {
        public Guid ID;
        public Team Team;
        public string UnitType; // "Tank", "Mage", "Healer", "Trickster"
        public int Level;
        public double Health;
        public int BoardX; // -1 если в инвентаре
        public int BoardY; // -1 если в инвентаре
        public bool IsUltimateActive;

        /// <summary>
        /// Создает состояние юнита из интерфейса ICharacter
        /// </summary>
        public static UnitState CreateFromCharacter(ICharacter character, int boardX, int boardY)
        {
            var state = new UnitState
            {
                ID = character.ID,
                Team = character.Team,
                Level = character.Level,
                Health = character.Health,
                BoardX = boardX,
                BoardY = boardY
            };

            // Определяем тип юнита
            if (character is Entities.Tank tank)
            {
                state.UnitType = "Tank";
                state.IsUltimateActive = tank.IsUltimateActive;
            }
            else if (character is Entities.Mage mage)
            {
                state.UnitType = "Mage";
                state.IsUltimateActive = mage.IsUltimateActive;
            }
            else if (character is Entities.Healer healer)
            {
                state.UnitType = "Healer";
                state.IsUltimateActive = healer.IsUltimateActive;
            }
            else if (character is Entities.Trickster trickster)
            {
                state.UnitType = "Trickster";
                state.IsUltimateActive = trickster.IsUltimateActive;
            }

            return state;
        }
    }
}
