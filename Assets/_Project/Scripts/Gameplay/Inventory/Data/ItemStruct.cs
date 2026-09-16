using System;

namespace JogoBruxinha.Gameplay.Inventory
{
    [Serializable]
    public struct ItemStruct
    {
        public ItemDataSO item;
        public int amount;

        public ItemStruct(ItemDataSO item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }
    }
}