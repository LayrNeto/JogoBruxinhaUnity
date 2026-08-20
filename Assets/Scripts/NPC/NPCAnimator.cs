using UnityEngine;

[RequireComponent(typeof(Animator))]
public sealed class NPCAnimator : MonoBehaviour
{
    private Animator anim;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsHealedHash = Animator.StringToHash("IsHealed");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetController(RuntimeAnimatorController controller)
    {
        anim.runtimeAnimatorController = controller;
    }

    public void SetWalking(bool isWalking, float directionY = -1f)
    {
        anim.SetBool(IsWalkingHash, isWalking);
        anim.SetFloat(MoveYHash, directionY);
    }

    public void SetHealed(bool isHealed)
    {
        anim.SetBool(IsHealedHash, isHealed);
    }

    public void SetActive(bool active)
    {
        anim.enabled = active;
    }
}