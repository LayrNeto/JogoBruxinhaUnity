using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    public PlayerController player;
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

        an.SetFloat("RunMultiplier", multiplier);
        an.SetFloat("MoveX", animInput.x);
        an.SetFloat("MoveY", animInput.y);        
        an.SetFloat("LastMoveX", animLastDir.x);
        an.SetFloat("LastMoveY", animLastDir.y);
        an.SetFloat("Speed", player.MovementInput.sqrMagnitude); 
    }
}