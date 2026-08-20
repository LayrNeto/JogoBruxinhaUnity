using System.Collections;
using UnityEngine;

public class CatPettingCutscene : MonoBehaviour
{
    [Header("Player References")]
    public Rigidbody2D playerRb;
    public SpriteRenderer playerSR;
    public PlayerController playerController;

    [Header("Cat References")]
    public SpriteRenderer catSR;
    public Interactable catInteractable;
    public CompanionAnimator catAnimator;

    [Header("Combined References")]
    public GameObject puppetVisuals;
    public Animator puppetAnimator;

    [Header("Positions")]
    public Transform pointLeft;
    public Transform pointRight;
    public Vector2 offsetLeft;
    public Vector2 offsetRight;

    [Header("Audio")]
    public SoundDataSO meowSound;
    public float soundInterval = 5f;

    [Header("General Settings")]
    public float walkSpeed = 2f;
    public float timeout = 0.8f;

    private Coroutine audioLoopCoroutine;

    void Update()
    {
        if (!puppetVisuals.activeSelf) return;

        if (UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame)
        {
            StopCutscene();
        } 
    }

    public void StartCutscene()
    {
        StartCoroutine(CutsceneRoutine());
    }

    private IEnumerator CutsceneRoutine()
    {
        Debug.Log("START PETTING");
        GameStateManager.Instance.PushState(GameStateManager.GameState.CUTSCENE);

        playerRb.linearVelocity = Vector2.zero;

        bool playerOnRight = playerRb.position.x > transform.position.x;
        Transform targetPoint = playerOnRight ? pointRight : pointLeft;

        float timer = 0f;
        float distance = Vector2.Distance(playerRb.position, targetPoint.position);

        while (distance > 0.05f)
        {
            if (timer >= timeout)
            {
                GameStateManager.Instance.PopState();
                yield break; 
            }

            Vector2 walkDir = (targetPoint.position - playerRb.transform.position).normalized;
            playerController.StartAutoWalk(walkDir);

            Vector2 newPos = Vector2.MoveTowards(playerRb.position, targetPoint.position, walkSpeed * Time.fixedDeltaTime);
            playerRb.MovePosition(newPos);

            timer += Time.fixedDeltaTime;
            distance = Vector2.Distance(playerRb.position, targetPoint.position);
            
            yield return new WaitForFixedUpdate(); 
        }
        yield return null;

        playerSR.enabled = false;
        catSR.enabled = false;
        puppetVisuals.SetActive(true);
        catInteractable.canInteract = false; 

        puppetVisuals.transform.localPosition = playerOnRight ? offsetRight : offsetLeft;

        if (playerOnRight)
        {
            playerController.StopAutoWalk(new Vector2(-1, 0));
            puppetAnimator.Play("PettingRight");
        }
        else
        {
            playerController.StopAutoWalk(new Vector2(1, 0));
            puppetAnimator.Play("PettingLeft");
        }

        if (audioLoopCoroutine != null) StopCoroutine(audioLoopCoroutine);
        audioLoopCoroutine = StartCoroutine(CatAudioLoopRoutine());

        yield return new WaitForSeconds(0.8f);

        if (puppetVisuals.activeInHierarchy)
        {
            puppetAnimator.Play(playerOnRight ? "LoopRight" : "LoopLeft");
        }
    }

    private IEnumerator CatAudioLoopRoutine()
    {
        while (true)
        {
            AudioManager.Instance.PlaySFX(meowSound);

            yield return new WaitForSeconds(soundInterval);
        }
    }

    public void StopCutscene()
    {
        Debug.Log("STOP PETTING");

        if (audioLoopCoroutine != null)
        {
            StopCoroutine(audioLoopCoroutine);
            audioLoopCoroutine = null;
        }
        
        puppetVisuals.SetActive(false);
        playerSR.enabled = true;
        catSR.enabled = true;
        catInteractable.canInteract = true;

        float lookDirection = (playerRb.position.x > transform.position.x) ? 1f : -1f;
        catAnimator.ForceDirection(lookDirection);
        
        GameStateManager.Instance.PopState();
    }
}