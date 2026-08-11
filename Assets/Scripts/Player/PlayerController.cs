using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public EntityTrackerSO entityTracker;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runMultiplier = 1.5f;

    [Header("Dash Settings")]
    public float dashSpeed = 6f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 1f;
    
    [HideInInspector]
    public bool isAutoWalking = false;

    private Rigidbody2D rb;

    public Vector2 MovementInput { get; private set; }
    public Vector2 LastDirection { get; private set; } = Vector2.down;
    public bool IsDashing { get; private set; }
    public bool IsRunning { get; private set; }

    private bool canDash = true;

    private void OnEnable()
    {
        entityTracker.player = this; 
        GameStateManager.Instance.inputControls.Player.Dash.performed += AttemptDash;
    }

    private void OnDisable()
    {
        if (entityTracker.player == this) entityTracker.player = null;
        if (GameStateManager.Instance != null) GameStateManager.Instance.inputControls.Player.Dash.performed -= AttemptDash;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (IsDashing) return;

        if (!isAutoWalking) ProcessInputs();
        
        if (MovementInput.sqrMagnitude > 0) LastDirection = MovementInput.normalized;
    }

    void FixedUpdate()
    {
        if (IsDashing) return;

        float speed = IsRunning ? moveSpeed * runMultiplier : moveSpeed;
        rb.linearVelocity = MovementInput.normalized * speed;
    }

    private void ProcessInputs()
    {
        MovementInput = GameStateManager.Instance.inputControls.Player.Move.ReadValue<Vector2>();
        IsRunning = GameStateManager.Instance.inputControls.Player.Run.IsPressed();
    }

    private void AttemptDash(InputAction.CallbackContext context)
    {
        if (canDash && MovementInput.sqrMagnitude > 0)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        IsDashing = true;
        canDash = false;

        rb.linearVelocity = MovementInput.normalized * dashSpeed;

        yield return new WaitForSeconds(dashDuration);
        IsDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void ChangeIdleDirection(Vector2 newDir)
    {
        LastDirection = newDir;
    }

    public void StartAutoWalk(Vector2 direction)
    {
        isAutoWalking = true;
        MovementInput = direction;
    }

    public void StopAutoWalk(Vector2 lookDirection)
    {
        isAutoWalking = false;
        MovementInput = Vector2.zero;
        LastDirection = lookDirection;
    }
}