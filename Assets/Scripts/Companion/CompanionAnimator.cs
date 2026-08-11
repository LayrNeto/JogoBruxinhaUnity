using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CompanionAnimator : MonoBehaviour
{
    [Header("Parent References")]
    public CompanionBrain brain;
    public CompanionMovement movement;

    private Animator an;
    private float lastMoveX = 1f; 

    void Awake()
    {
        an = GetComponent<Animator>();
    }

    void Update()
    {
        bool isWalking = brain.currentState == CompanionBrain.CompanionState.Following;
        bool isSleeping = brain.currentState == CompanionBrain.CompanionState.Sleeping;

        an.SetBool("IsWalking", isWalking);
        an.SetBool("IsSleeping", isSleeping);

        if (isWalking)
        {
            Vector2 dir = movement.CurrentDirection; 

            if (Mathf.Abs(dir.x) > 0.01f)
            {
                float moveX = Mathf.Sign(dir.x); 
                an.SetFloat("MoveX", moveX);
                lastMoveX = moveX; 
            }
        }
        else
        {
            an.SetFloat("MoveX", lastMoveX); 
        }
    }

    public void ForceDirection(float dirX)
    {
        lastMoveX = Mathf.Sign(dirX);
        an.SetFloat("MoveX", lastMoveX);
    }

    public void SleepForTheNight()
    {
        an.SetBool("IsWalking", false);
        an.SetBool("IsSleeping", true);
        ForceDirection(-1f); 
    }
}