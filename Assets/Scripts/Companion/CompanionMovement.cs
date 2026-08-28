using UnityEngine;
using UnityEngine.AI; 

public class CompanionMovement : MonoBehaviour
{
    [Header("Movement")]
    public Transform target;
    public float speed = 2f;
    
    [Header("Collision")]
    public float radius = 0.4f;
    public float centerHeight = 0.4f;
    public LayerMask obstaclesLayer; 

    public Vector2 CurrentDirection { get; private set; }

    private NavMeshAgent agent;
    private Color debugCircleCast;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;

        debugCircleCast = Color.green;
    }

    void Update()
    {
        if (!target) return;

        Vector2 myPos = (Vector2)transform.position + new Vector2(0, centerHeight);
        Vector2 targetPos = (Vector2)target.position;

        Vector2 diff = targetPos - myPos;
        float distance = diff.magnitude;
        Vector2 dir = (distance > 0.0001f) ? diff / distance : Vector2.zero;

        RaycastHit2D hit = Physics2D.CircleCast(myPos, radius, dir, distance, obstaclesLayer);

        debugCircleCast = (hit.collider != null) ? Color.red : Color.green;

        if (hit.collider == null)
        {
            agent.isStopped = true;
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            CurrentDirection = dir;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(targetPos);
            CurrentDirection = agent.velocity.normalized;
        }
    }

    public void StartFollowing()
    {
        enabled = true;
    }

    public void StopFollowing()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
        enabled = false;
    }

    public void DisableMovement()
    {
        if (agent != null) agent.enabled = false;
        enabled = false;
    }

    public void TeleportTo(Vector2 newPosition)
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.Warp(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 center = (Vector2)transform.position + new Vector2(0, centerHeight);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, radius);

        if (target != null)
        {
            Vector2 targetCenter = (Vector2)target.position;
            Vector2 direcao = (targetCenter - center).normalized;
            Vector2 perpendicular = new Vector2(-direcao.y, direcao.x) * radius;

            Gizmos.color = debugCircleCast;

            Gizmos.DrawLine(center + perpendicular, targetCenter + perpendicular);
            Gizmos.DrawLine(center - perpendicular, targetCenter - perpendicular);
        }
    }
}