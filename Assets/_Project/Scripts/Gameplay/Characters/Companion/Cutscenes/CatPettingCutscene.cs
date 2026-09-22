using System.Collections;
using JogoBruxinha.Core.Analytics;
using JogoBruxinha.Core.Audio;
using JogoBruxinha.Gameplay.Characters.Player;
using JogoBruxinha.Gameplay.Environment;
using JogoBruxinha.Gameplay.GameFlow;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JogoBruxinha.Gameplay.Characters.Companion
{
    public sealed class CatPettingCutscene : MonoBehaviour
    {
        private static readonly int PettingRightHash = Animator.StringToHash("PettingRight");
        private static readonly int PettingLeftHash = Animator.StringToHash("PettingLeft");
        private static readonly int LoopRightHash = Animator.StringToHash("LoopRight");
        private static readonly int LoopLeftHash = Animator.StringToHash("LoopLeft");

        [Header("Player References")]
        [SerializeField] private Rigidbody2D _playerRb;
        [SerializeField] private SpriteRenderer _playerSR;
        [SerializeField] private PlayerController _playerController;

        [Header("Cat References")]
        [SerializeField] private SpriteRenderer _catSR;
        [SerializeField] private Interactable _catInteractable;
        [SerializeField] private CompanionAnimator _catAnimator;

        [Header("Combined References")]
        [SerializeField] private GameObject _puppetVisuals;
        [SerializeField] private Animator _puppetAnimator;

        [Header("Positions")]
        [SerializeField] private Transform _pointLeft;
        [SerializeField] private Transform _pointRight;
        [SerializeField] private Vector2 _offsetLeft;
        [SerializeField] private Vector2 _offsetRight;

        [Header("Audio")]
        [SerializeField] private SoundDataSO _meowSound;
        [SerializeField] private float _soundInterval = 5f;

        [Header("General Settings")]
        [SerializeField] private float _walkSpeed = 2f;
        [SerializeField] private float _timeout = 0.8f;

        private Coroutine _cutsceneCoroutine;
        private Coroutine _audioLoopCoroutine;

        private void Awake()
        {
            Debug.Assert(_playerRb != null, $"Missing _playerRb reference on {name}", this);
            Debug.Assert(_playerSR != null, $"Missing _playerSR reference on {name}", this);
            Debug.Assert(_playerController != null, $"Missing _playerController reference on {name}", this);
            Debug.Assert(_catSR != null, $"Missing _catSR reference on {name}", this);
            Debug.Assert(_catInteractable != null, $"Missing _catInteractable reference on {name}", this);
            Debug.Assert(_catAnimator != null, $"Missing _catAnimator reference on {name}", this);
            Debug.Assert(_puppetVisuals != null, $"Missing _puppetVisuals reference on {name}", this);
            Debug.Assert(_puppetAnimator != null, $"Missing _puppetAnimator reference on {name}", this);
            Debug.Assert(_pointLeft != null, $"Missing _pointLeft reference on {name}", this);
            Debug.Assert(_pointRight != null, $"Missing _pointRight reference on {name}", this);
        }

        private void Update()
        {
            if (_puppetVisuals == null || !_puppetVisuals.activeSelf) return;

            bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            bool gamepadPressed = Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame);

            if (keyPressed || gamepadPressed)
            {
                StopCutscene();
            }
        }

        private void OnDisable()
        {
            if (_puppetVisuals != null && _puppetVisuals.activeSelf)
            {
                StopCutscene();
            }
        }

        public void StartCutscene()
        {
            if (_cutsceneCoroutine != null)
            {
                StopCoroutine(_cutsceneCoroutine);
            }

            _cutsceneCoroutine = StartCoroutine(CutsceneRoutine());
        }

        private IEnumerator CutsceneRoutine()
        {
            Debug.Log("START PETTING");

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.PushState(GameStateManager.GameState.CUTSCENE);
            }

            _playerRb.linearVelocity = Vector2.zero;

            bool playerOnRight = _playerRb.position.x > transform.position.x;
            Transform targetPoint = playerOnRight ? _pointRight : _pointLeft;

            float timer = 0f;
            const float stopThresholdSqr = 0.05f * 0.05f;

            Vector2 targetPos = targetPoint.position;
            Vector2 diff = targetPos - _playerRb.position;
            _catInteractable.canInteract = false;

            while (diff.sqrMagnitude > stopThresholdSqr)
            {
                if (timer >= _timeout)
                {
                    Debug.LogWarning("Timeout ao tentar alcançar o gato, abortando cutscene.");

                    _playerController.StopAutoWalk(diff.normalized);
                    _playerRb.linearVelocity = Vector2.zero;

                    if (_catInteractable != null)
                    {
                        _catInteractable.canInteract = true;
                    }

                    if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameStateManager.GameState.CUTSCENE)
                    {
                        GameStateManager.Instance.PopState();
                    }

                    _cutsceneCoroutine = null;
                    yield break;
                }

                Vector2 walkDir = diff.normalized;
                _playerController.StartAutoWalk(walkDir);

                Vector2 newPos = Vector2.MoveTowards(_playerRb.position, targetPos, _walkSpeed * Time.fixedDeltaTime);
                _playerRb.MovePosition(newPos);

                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();

                diff = targetPos - _playerRb.position;
            }

            yield return null;

            _playerSR.enabled = false;
            _catSR.enabled = false;
            _puppetVisuals.SetActive(true);

            _puppetVisuals.transform.localPosition = playerOnRight ? _offsetRight : _offsetLeft;

            // Playtest Analytics ==================================
            PlaytestLogger.Instance.RecordCatPetting();
            // Playtest Analytics ==================================

            if (playerOnRight)
            {
                _playerController.StopAutoWalk(new Vector2(-1f, 0f));
                _puppetAnimator.Play(PettingRightHash);
            }
            else
            {
                _playerController.StopAutoWalk(new Vector2(1f, 0f));
                _puppetAnimator.Play(PettingLeftHash);
            }

            if (_audioLoopCoroutine != null)
            {
                StopCoroutine(_audioLoopCoroutine);
            }

            _audioLoopCoroutine = StartCoroutine(CatAudioLoopRoutine());

            yield return new WaitForSeconds(0.8f);

            if (_puppetVisuals.activeInHierarchy)
            {
                _puppetAnimator.Play(playerOnRight ? LoopRightHash : LoopLeftHash);
            }

            _cutsceneCoroutine = null;
        }

        private IEnumerator CatAudioLoopRoutine()
        {
            while (true)
            {
                if (AudioManager.Instance != null && _meowSound != null)
                {
                    AudioManager.Instance.PlaySFX(_meowSound);
                }

                yield return new WaitForSeconds(_soundInterval);
            }
        }

        public void StopCutscene()
        {
            Debug.Log("STOP PETTING");

            if (_cutsceneCoroutine != null)
            {
                StopCoroutine(_cutsceneCoroutine);
                _cutsceneCoroutine = null;
            }

            if (_audioLoopCoroutine != null)
            {
                StopCoroutine(_audioLoopCoroutine);
                _audioLoopCoroutine = null;
            }

            if (_puppetVisuals != null) _puppetVisuals.SetActive(false);
            if (_playerSR != null) _playerSR.enabled = true;
            if (_catSR != null) _catSR.enabled = true;
            if (_catInteractable != null) _catInteractable.canInteract = true;

            if (_catAnimator != null)
            {
                float lookDirection = (_playerRb.position.x > transform.position.x) ? 1f : -1f;
                _catAnimator.ForceDirection(lookDirection);
            }

            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameStateManager.GameState.CUTSCENE)
            {
                GameStateManager.Instance.PopState();
            }
        }
    }
}