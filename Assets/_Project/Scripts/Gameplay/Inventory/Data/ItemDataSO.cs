using UnityEngine;
using UnityEngine.Localization;

namespace JogoBruxinha.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "NewGenericItem", menuName = "Scriptable Objects/Items/Generic Item")]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Base Identification")]
        public string id;
        public LocalizedString displayName;

        [Header("Base Visuals")]
        public Sprite inventoryIcon;

        public string GetLocalizedName()
        {
            if (displayName == null || displayName.IsEmpty)
                return id; // Fallback seguro caso esqueça de associar a tabela

            return displayName.GetLocalizedString();
        }
    }
}