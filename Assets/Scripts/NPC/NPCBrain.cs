using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NPCMovement))]
public class NPCBrain : MonoBehaviour
{
    public enum NPCState
    {
        HIDDEN,
        ENTERING,
        WAITING,
        LEAVING
    }

    [Header("Current State [Read Only]")]
    public PatientDataSO currentData;
    public NPCState currentState = NPCState.HIDDEN;
    public bool isHealed = false;

    [Header("Dependencies")]
    public SpriteRenderer spriteRenderer;
    public Animator anim;
    public Interactable counterInteractable; 
    public SessionDataSO sessionData;

    private NPCMovement movementScript;

    private void Awake()
    {
        movementScript = GetComponent<NPCMovement>();
        
        SetVisibility(false);
    }

    void Start()
    {
        if (counterInteractable) counterInteractable.canInteract = false;    
    }

    public void SetupNPC(PatientDataSO data)
    {
        currentData = data;
        isHealed = false;

        if (currentData.animatorController)
        {
            anim.runtimeAnimatorController = currentData.animatorController;
        }

        movementScript.SetSpeed(currentData.cursedMovementSpeed);

        anim.SetBool("IsHealed", false);
        anim.SetBool("IsWalking", false);
        anim.SetFloat("MoveY", -1f);

        RestoreState();
    }

    private void RestoreState()
    {
        if (sessionData.savedNPCState == NPCState.ENTERING || sessionData.savedNPCState == NPCState.WAITING)
        {
            currentState = NPCState.WAITING;
            sessionData.savedNPCState = NPCState.WAITING;

            SetVisibility(true);
    
            anim.SetBool("IsHealed", isHealed);
            anim.SetBool("IsWalking", false);
            anim.SetFloat("MoveY", -1f);

            movementScript.TeleportToCounter();
            
            if (counterInteractable) counterInteractable.canInteract = true;
        }
        else 
        {
            currentState = NPCState.HIDDEN;
            sessionData.savedNPCState = NPCState.HIDDEN; 
            
            SetVisibility(false);
    
            anim.SetBool("IsHealed", false);
            anim.SetBool("IsWalking", false);
            anim.SetFloat("MoveY", -1f);
        }
    }

    public void SpawnAndEnter()
    {
        if (currentState != NPCState.HIDDEN) return;

        sessionData.hasNPCSpawnedToday = true;
        sessionData.savedNPCState = NPCState.ENTERING;
        
        currentState = NPCState.ENTERING;
        SetVisibility(true);

        anim.SetFloat("MoveY", -1f);
        anim.SetBool("IsWalking", true);

        movementScript.MoveToCounter(() => 
        {
            OnArrivedAtCounter();
        });
    }

    private void OnArrivedAtCounter()
    {
        sessionData.savedNPCState = NPCState.WAITING;
        currentState = NPCState.WAITING;

        anim.SetBool("IsWalking", false);

        if (counterInteractable ) counterInteractable.canInteract = true;

        Debug.Log($"[{currentData.clientName}] chegou ao balcão e está esperando.");
    }

    public void LeaveShop(bool wasSuccess)
    {
        Debug.Log("NPC Leaving - Iniciando Delay");
        
        if (currentState != NPCState.WAITING) return;
        if (counterInteractable) counterInteractable.canInteract = false;

        isHealed = wasSuccess;
        anim.SetBool("IsHealed", isHealed);
        sessionData.savedNPCState = NPCState.LEAVING;
        
        movementScript.SetSpeed(currentData.healedMovementSpeed);

        StartCoroutine(LeaveDelayRoutine());
    }

    private IEnumerator LeaveDelayRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        currentState = NPCState.LEAVING;
        
        anim.SetFloat("MoveY", 1f);
        anim.SetBool("IsWalking", true);

        movementScript.MoveToDoor(() => 
        {
            OnArrivedAtDoor();
        });
    }

    private void OnArrivedAtDoor()
    {
        currentState = NPCState.HIDDEN;
        sessionData.savedNPCState = NPCState.HIDDEN;
        
        anim.SetBool("IsWalking", false);
        SetVisibility(false);
        
        // Aqui você pode disparar um evento global avisando o GameManager que o dia acabou
        // Ex: EventManager.Trigger(Events.NPC_LEFT);
    }

    private void SetVisibility(bool visible)
    {
        if (spriteRenderer) spriteRenderer.enabled = visible;
    }
}   