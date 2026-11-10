using UnityEngine;

namespace JogoBruxinha.Core.UI
{
    public sealed class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }

        [Header("Cursor Textures (32x32)")]
        [SerializeField] private Texture2D _cursorDefault;
        [SerializeField] private Texture2D _cursorHover;
        [SerializeField] private Texture2D _cursorPressed;

        [Header("Hotspot Settings")]
        [SerializeField] private Vector2 _hotspot = Vector2.zero;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                ValidateDependencies();
                SetDefaultCursor();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void ValidateDependencies()
        {
            Debug.Assert(_cursorDefault != null, $"Missing _cursorDefault on {name}", this);
            Debug.Assert(_cursorHover != null, $"Missing _cursorHover on {name}", this);
            Debug.Assert(_cursorPressed != null, $"Missing _cursorPressed on {name}", this);
        }

        public void SetDefaultCursor()
        {
            Cursor.SetCursor(_cursorDefault, _hotspot, CursorMode.Auto);
        }

        public void SetHoverCursor()
        {
            Cursor.SetCursor(_cursorHover, _hotspot, CursorMode.Auto);
        }

        public void SetPressedCursor()
        {
            Cursor.SetCursor(_cursorPressed, _hotspot, CursorMode.Auto);
        }
    }
}