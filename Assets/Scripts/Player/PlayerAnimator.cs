using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    public PlayerController player;

    private static readonly int RunMultiplierHash = Animator.StringToHash("RunMultiplier");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int LastMoveXHash = Animator.StringToHash("LastMoveX");
    private static readonly int LastMoveYHash = Animator.StringToHash("LastMoveY");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private Animator an;

    void Start()
    {
        an = GetComponent<Animator>();
    }

    void Update()
    {
        if (player.IsDashing) return;

        Vector2 animInput = player.MovementInput;
        Vector2 animLastDir = player.LastDirection;

        if (Mathf.Abs(animInput.x) > 0.01f) animInput.y = 0;
        if (Mathf.Abs(animLastDir.x) > 0.01f) animLastDir.y = 0;

        animInput = animInput.normalized;
        animLastDir = animLastDir.normalized;

        float multiplier = player.IsRunning ? 1.8f : 1f;

        an.SetFloat(RunMultiplierHash, multiplier);
        an.SetFloat(MoveXHash, animInput.x);
        an.SetFloat(MoveYHash, animInput.y);        
        an.SetFloat(LastMoveXHash, animLastDir.x);
        an.SetFloat(LastMoveYHash, animLastDir.y);
        an.SetFloat(SpeedHash, player.MovementInput.sqrMagnitude); 
    }
}