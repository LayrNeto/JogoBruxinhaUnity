using UnityEngine;

[CreateAssetMenu(fileName = "PlantDataSO", menuName = "Scriptable Objects/Items/Plant")]
public class PlantDataSO : ItemDataSO 
{
    [Header("Top Down Visuals")]
    public Sprite[] worldSprites; 
    public Sprite[] collectedSprites; 

    [Header("Cauldron Visuals")]
    public Sprite shelfSprite;
    public Sprite dragSprite; 
    public Sprite emptyShelfSprite;

    [Header("Growth System")]
    public int daysToGrow; 

    [Header("Audio")]
    public SoundDataSO[] collectSound;
}