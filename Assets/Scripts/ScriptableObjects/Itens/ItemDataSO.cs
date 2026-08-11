using UnityEngine;

[CreateAssetMenu(fileName = "NewGenericItem", menuName = "Scriptable Objects/Items/Generic Item")]
public class ItemDataSO : ScriptableObject
{
    [Header("Base Identification")]
    public string id; 
    public string displayName; 

    [Header("Base Visuals")]
    public Sprite inventoryIcon; 
}