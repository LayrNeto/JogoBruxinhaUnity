using UnityEngine;

namespace JogoBruxinha.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "NewPotionItem", menuName = "Scriptable Objects/Items/Potion Item")]
    public sealed class PotionDataSO : ItemDataSO 
    {
        [Header("Table Visuals")]
        public Sprite tableSprite; 
    }
}