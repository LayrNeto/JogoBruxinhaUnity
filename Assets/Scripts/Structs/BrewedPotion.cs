using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BrewedPotion
{
    public List<PlantDataSO> ingredients;
    public bool isSpellUsed;
    public PotionDataSO potionData;

    public BrewedPotion(HashSet<PlantDataSO> hashSetIngredients, bool magic, PotionDataSO potion)
    {
        ingredients = new List<PlantDataSO>(hashSetIngredients);
        isSpellUsed = magic;
        potionData = potion;
    }
}