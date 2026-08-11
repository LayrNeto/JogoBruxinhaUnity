[System.Serializable]
public struct ItemStruct
{
    public ItemDataSO item;
    public int amount;

    public ItemStruct(ItemDataSO p, int i)
    {
        item = p;
        amount = i;
    }
}