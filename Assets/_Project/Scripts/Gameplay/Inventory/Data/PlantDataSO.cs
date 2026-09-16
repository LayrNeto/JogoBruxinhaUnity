using JogoBruxinha.Core.Audio;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "PlantDataSO", menuName = "Scriptable Objects/Items/Plant")]
    public sealed class PlantDataSO : ItemDataSO
    {
        [Header("Top Down Visuals")]
        public Sprite[] worldSprites = new Sprite[0];
        public Sprite[] collectedSprites = new Sprite[0];

        [Header("Cauldron Visuals")]
        public Sprite shelfSprite;
        public Sprite dragSprite;
        public Sprite emptyShelfSprite;

        [Header("Growth System")]
        public int daysToGrow;

        [Header("Audio")]
        public SoundDataSO[] collectSound = new SoundDataSO[0];
    }
}