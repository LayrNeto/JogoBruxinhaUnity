using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CompanionAnimator : MonoBehaviour
{
    [Header("Parent References")]
    public CompanionBrain brain;
    public CompanionMovement movement;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsSleepingHash = Animator.StringToHash("IsSleeping");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");

    private Animator an;
    private float lastMoveX = 1f; 

    void Awake()
    {
        an = GetComponent<Animator>();
    }

    void Update()
    {
        if (brain == null) return;

        bool isWalking = brain.CurrentState == brain.FollowingState;
        bool isSleeping = brain.CurrentState == brain.SleepingState;

        an.SetBool(IsWalkingHash, isWalking);
        an.SetBool(IsSleepingHash, isSleeping);

        if (isWalking)
        {
            Vector2 dir = movement.CurrentDirection; 

            if (Mathf.Abs(dir.x) > 0.01f)
            {
                float moveX = Mathf.Sign(dir.x); 
                an.SetFloat(MoveXHash, moveX);
                lastMoveX = moveX; 
            }
        }
        else
        {
            an.SetFloat(MoveXHash, lastMoveX); 
        }
    }

    public void ForceDirection(float dirX)
    {
        lastMoveX = Mathf.Sign(dirX);
        an.SetFloat(MoveXHash, lastMoveX);
    }

    public void SleepForTheNight()
    {
        an.SetBool(IsWalkingHash, false);
        an.SetBool(IsSleepingHash, true);
        ForceDirection(-1f); 
    }
}