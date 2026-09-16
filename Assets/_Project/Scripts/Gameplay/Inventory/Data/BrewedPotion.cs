using System;
using System.Collections.Generic;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Inventory
{
    [Serializable]
    public struct BrewedPotion
    {
        public List<PlantDataSO> ingredients;
        public bool isSpellUsed;
        public PotionDataSO potionData;

        public BrewedPotion(HashSet<PlantDataSO> hashSetIngredients, bool isSpellUsed, PotionDataSO potionData)
        {
            ingredients = hashSetIngredients != null ? new List<PlantDataSO>(hashSetIngredients) : new List<PlantDataSO>();
            this.isSpellUsed = isSpellUsed;
            this.potionData = potionData;
        }
    }
}