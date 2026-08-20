using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NPCMovement))]
public class NPCBrain : MonoBehaviour
{
    public enum NPCStateEnum { HIDDEN, ENTERING, WAITING, LEAVING }

    [Header("Current State [Read Only]")]
    public PatientDataSO currentData;
    public bool isHealed = false;

    [Header("Dependencies")]
    public SpriteRenderer spriteRenderer;
    public NPCAnimator npcAnimator;
    public Interactable counterInteractable; 
    public SessionDataSO sessionData;

    public IState HiddenState { get; private set; }
    public IState EnteringState { get; private set; }
    public IState WaitingState { get; private set; }
    public IState LeavingState { get; private set; }
    public IState CurrentState { get; private set; }

    public NPCMovement MovementScript { get; private set; }

    private void Awake()
    {
        MovementScript = GetComponent<NPCMovement>();
        
        HiddenState = new NPCHiddenState(this);
        EnteringState = new NPCEnteringState(this);
        WaitingState = new NPCWaitingState(this);
        LeavingState = new NPCLeavingState(this);
    }

    void Start()
    {
        if (counterInteractable) counterInteractable.canInteract = false;    
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

        currentData = data;
        isHealed = false;

        if (currentData.animatorController)
            npcAnimator.SetController(currentData.animatorController);

        MovementScript.SetSpeed(currentData.cursedMovementSpeed);
        RestoreState();
    }

    private void RestoreState()
    {
        if (sessionData.savedNPCState == NPCStateEnum.ENTERING || sessionData.savedNPCState == NPCStateEnum.WAITING)
        {
            MovementScript.TeleportToCounter();
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

        isHealed = wasSuccess;
        ChangeState(LeavingState);
    }

    public void SetVisibility(bool visible)
    {
        if (spriteRenderer) spriteRenderer.enabled = visible;
        if (npcAnimator) npcAnimator.SetActive(visible);
    }
}   