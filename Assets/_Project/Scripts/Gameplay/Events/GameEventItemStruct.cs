using JogoBruxinha.Core.Events;
using JogoBruxinha.Gameplay.Inventory;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Events
{
    [CreateAssetMenu(menuName = "Events/ItemStruct Event")]
    public sealed class GameEventItemStruct : BaseGameEvent<ItemStruct> { }
}