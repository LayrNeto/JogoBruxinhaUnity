using JogoBruxinha.Core.StateMachines;
using JogoBruxinha.Gameplay.Counter;
using JogoBruxinha.Gameplay.Environment;
using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Characters.NPC
{
    [RequireComponent(typeof(NPCMovement))]
    public sealed class NPCBrain : MonoBehaviour
    {
        public enum NPCStateEnum { HIDDEN, ENTERING, WAITING, LEAVING }

        [Header("Dependencies")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private NPCAnimator _npcAnimator;
        [SerializeField] private Interactable _counterInteractable; 
        [SerializeField] private SessionDataSO _sessionData;

        private PatientDataSO _currentData;
        private bool _isHealed;

        // Propriedades públicas para consumo dos Estados e sistemas externos
        public PatientDataSO CurrentData => _currentData;
        public bool IsHealed => _isHealed;
        public SpriteRenderer SpriteRendererComponent => _spriteRenderer;
        public NPCAnimator NpcAnimator => _npcAnimator;
        public Interactable CounterInteractable => _counterInteractable;
        public SessionDataSO SessionData => _sessionData;

        public IState HiddenState { get; private set; }
        public IState EnteringState { get; private set; }
        public IState WaitingState { get; private set; }
        public IState LeavingState { get; private set; }
        public IState CurrentState { get; private set; }

        public NPCMovement MovementScript { get; private set; }

        private void Awake()
        {
            MovementScript = GetComponent<NPCMovement>();

            Debug.Assert(MovementScript != null, $"Missing NPCMovement component on {name}", this);
            Debug.Assert(_spriteRenderer != null, $"Missing _spriteRenderer reference on {name}", this);
            Debug.Assert(_npcAnimator != null, $"Missing _npcAnimator reference on {name}", this);
            Debug.Assert(_counterInteractable != null, $"Missing _counterInteractable reference on {name}", this);
            Debug.Assert(_sessionData != null, $"Missing _sessionData reference on {name}", this);

            HiddenState = new NPCHiddenState(this);
            EnteringState = new NPCEnteringState(this);
            WaitingState = new NPCWaitingState(this);
            LeavingState = new NPCLeavingState(this);
        }

        private void Start()
        {
            if (_counterInteractable != null)
            {
                _counterInteractable.canInteract = false;
            }

            SetVisibility(false);
        }

        public void ChangeState(IState newState)
        {
            if (newState == null || CurrentState == newState) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void SetupNPC(PatientDataSO data)
        {
            if (!data) return;

            _currentData = data;
            _isHealed = false;

            if (_currentData.animatorController != null && _npcAnimator != null)
            {
                _npcAnimator.SetController(_currentData.animatorController);
            }

            if (MovementScript != null)
            {
                MovementScript.SetSpeed(_currentData.cursedMovementSpeed);
            }

            RestoreState();
        }

        private void RestoreState()
        {
            if (_sessionData == null) return;

            if (_sessionData.savedNPCState == NPCStateEnum.ENTERING || _sessionData.savedNPCState == NPCStateEnum.WAITING)
            {
                if (MovementScript != null)
                {
                    MovementScript.TeleportToCounter();
                }

                ChangeState(WaitingState);
            }
            else
            {
                ChangeState(HiddenState);
            }
        }

        public void SpawnAndEnter()
        {
            if (CurrentState != HiddenState) return;

            ChangeState(EnteringState);
        }

        public void LeaveShop(bool wasSuccess)
        {
            if (CurrentState != WaitingState) return;

            _isHealed = wasSuccess;
            ChangeState(LeavingState);
        }

        public void SetVisibility(bool visible)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = visible;
            }

            if (_npcAnimator != null)
            {
                _npcAnimator.SetActive(visible);
            }
        }
    }
}