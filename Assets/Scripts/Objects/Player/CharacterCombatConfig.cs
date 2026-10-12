using System;
using UnityEditor;
using UnityEngine;

namespace ESS.Objects.Player
{
    [Serializable]
    public class CharacterCombatConfig
    {
        [Header("Combat Scripts")]
        public MonoScript basicAttack;
        public MonoScript[] skills;
    }
}