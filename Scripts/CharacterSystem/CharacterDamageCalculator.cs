using System.Collections.Generic;
using CharacterDefinition;
using UnityEngine;

namespace CharacterSystem
{
    public class CharacterDamageCalculator
    {
        private readonly Dictionary<(CharacterType attacker, CharacterType defender), float> typeAdvantageTable
            = new Dictionary<(CharacterType, CharacterType), float>
            {
                // 약점 관계
                {(CharacterType.Freeman, CharacterType.Engineer), 2.0f},
                {(CharacterType.Engineer, CharacterType.Successor), 2.0f},
                {(CharacterType.Successor, CharacterType.Freeman), 2.0f},

                // 내성 관계
                {(CharacterType.Freeman, CharacterType.Successor), 0.5f},
                {(CharacterType.Engineer, CharacterType.Freeman), 0.5f},
                {(CharacterType.Successor, CharacterType.Engineer), 0.5f},
            };

        
        /// <summary>
        /// Calculates the final damage dealt from the attacker to the target, factoring in the attacker's base attack damage
        /// and the type advantage system. The type advantage system applies a multiplier based on the relationship between
        /// the attacker's and target's character types (e.g., 2.0 for advantage, 0.5 for avoidance, 1.0 for neutral).
        /// </summary>
        /// <param name="attacker">The character dealing the damage. Must not be null.</param>
        /// <param name="target">The character receiving the damage. Must not be null.</param>
        /// <returns>
        /// The final calculated damage as an integer, after applying type multipliers. Returns 0 if either character is null.
        /// </returns>
        public int CalculateDamage(BaseCharacter attacker, BaseCharacter target)
        {
            if (attacker == null || target == null) return 0;

            // 1. 기본 공격력만 사용
            int baseDamage = attacker.characterStat.Atk;

            // 2. 상성 보정
            float typeMultiplier = GetTypeDamageMultiplier(attacker, target);
            int modifiedDamage = Mathf.FloorToInt(baseDamage * typeMultiplier);

            // 4. 최종 피해량 반환 (최소 0)
            int finalDamage = Mathf.Max(0, modifiedDamage);
            return finalDamage;
        }


        private float GetTypeDamageMultiplier(BaseCharacter attacker, BaseCharacter target)
        {
            var atkType = attacker.characterStat.Type;
            var defType = target.characterStat.Type;

            if (typeAdvantageTable.TryGetValue((atkType, defType), out float multiplier))
            {
                return multiplier;
            }

            return 1.0f; // 중립 관계
        }
    }
}