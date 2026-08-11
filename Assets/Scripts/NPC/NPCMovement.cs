using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCMovement : MonoBehaviour
{
    [Header("Pathing References")]
    public Transform doorPoint;
    public Transform counterPoint;

    [Header("[READ ONLY]")]
    public float currentSpeed = 2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }

    public void MoveToCounter(Action onArrived)
    {
        StartCoroutine(MoveRoutine(counterPoint.position, onArrived));
    }

    public void MoveToDoor(Action onArrived)
    {
        StartCoroutine(MoveRoutine(doorPoint.position, onArrived));
    }

    public void TeleportToCounter()
    {
        rb.position = counterPoint.position;
    }

    private IEnumerator MoveRoutine(Vector2 targetPos, Action onArrived)
    {
        float distance = Vector2.Distance(rb.position, targetPos);

        while (distance > 0.05f)
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, currentSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            yield return new WaitForFixedUpdate();
            
            distance = Vector2.Distance(rb.position, targetPos);
        }
        rb.MovePosition(targetPos);

        onArrived?.Invoke();
    }
    
        public Vector2 GetMovementDirection(Vector3 targetPosition)
    {
        return (targetPosition - transform.position).normalized;
    }
}