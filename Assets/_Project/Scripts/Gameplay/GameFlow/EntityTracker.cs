using JogoBruxinha.Gameplay.Characters.Companion;
using JogoBruxinha.Gameplay.Characters.Player;
using UnityEngine;

namespace JogoBruxinha.Gameplay.GameFlow
{
    [CreateAssetMenu(fileName = "EntityTracker", menuName = "Scriptable Objects/Architecture/Entity Tracker")]
    public sealed class EntityTrackerSO : ScriptableObject
    {
        public PlayerController Player;
        public CompanionBrain Companion;

        private void OnDisable()
        {
            Player = null;
            Companion = null;
        }
    }
}