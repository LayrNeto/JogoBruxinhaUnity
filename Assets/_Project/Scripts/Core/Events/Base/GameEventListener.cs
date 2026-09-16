using UnityEngine;
using UnityEngine.Events;

namespace JogoBruxinha.Core.Events
{
    public sealed class GameEventListener : MonoBehaviour
    {
        public GameEvent gameEvent; 
        public UnityEvent Response;      

        private void OnEnable()
        {
            if (gameEvent != null) gameEvent.listeners += EventResponse;
        }

        private void OnDisable()
        {
            if (gameEvent != null) gameEvent.listeners -= EventResponse;
        }

        private void EventResponse()
        {
            Response?.Invoke(); 
        }
    }
}