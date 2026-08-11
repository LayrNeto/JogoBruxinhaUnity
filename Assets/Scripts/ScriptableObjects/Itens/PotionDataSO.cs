using UnityEngine;

[CreateAssetMenu(fileName = "NewPotionItem", menuName = "Scriptable Objects/Items/Potion Item")]
public class PotionDataSO : ItemDataSO 
{
    [Header("Table Visuals")]
    public Sprite tableSprite; 
}