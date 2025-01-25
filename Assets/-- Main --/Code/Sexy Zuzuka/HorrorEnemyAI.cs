using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HorrorEnemyAI : MonoBehaviour
{
    public static HorrorEnemyAI instance; // Singleton for easy access

    [Header("General Settings")]
    public Transform player;
    public NavMeshAgent agent;
    public LayerMask playerLayer, obstacleLayer;

    [Header("Speeds")]
    public float wanderSpeed = 2f;
    public float investigateSpeed = 4f;
    public float chaseSpeed = 6f;

    [Header("Field of View")]
    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 90f;

    [Header("Timers")]
    public float animationDelay = 1.5f; // Time before chasing player
    public float attackCooldown = 2f; // Cooldown between attacks

    [Header("Waypoints")]
    public Transform[] wanderPoints; // Random waypoints inside the house

    [Header("Animations")]
    public Animator animator; // Assign your Animator component here
    public string idleAnimation = "Idle";
    public string walkAnimation = "Walk";
    public string runAnimation = "Run";
    public string attackAnimation = "Attack";

    [Header("Attack Settings")]
    public float attackDistance = 2f; // Distance to stop before attacking
    public System.Action onPlayerCaught;

    private PlayerHealth playerHealth;  // Reference to the player's health
    private PlayerFirstPersonMovement playerMovement; // Reference to the player's movement script
    private PlayerFirstPersonCamera playerCamera; // Reference to the player's camera script
    private bool isPlayerDead = false;  // Flag to track if the player is dead

    private Transform targetObject; // Target object to investigate
    private int currentWanderIndex;
    private bool isChasing = false;
    private bool isInvestigating = false;
    private bool isPlayingAnimation = false;
    private bool isAttacking = false; // Prevent spamming attack animations
    private float lastAttackTime;

    private void Awake()
    {
        // Initialize Singleton
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Subscribe to the player's death event
        playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.onDeath.AddListener(OnPlayerDeath);
        }

        playerMovement = player.GetComponent<PlayerFirstPersonMovement>();
        playerCamera = player.GetComponent<PlayerFirstPersonCamera>();

        // Set the agent speed to wandering speed initially
        agent.speed = wanderSpeed;
        GoToNextWanderPoint();
        PlayAnimation(walkAnimation); // Start with walk animation
    }

    private void Update()
    {
        if (isPlayerDead) return; // Stop all actions if the player is dead

        if (isPlayingAnimation) return;

        // Field of View Detection
        if (PlayerInFOV() && !isChasing)
        {
            StartCoroutine(StartChase());
            return;
        }

        // Behavior based on the current state
        if (isChasing)
        {
            ChasePlayer();
        }
        else if (isInvestigating)
        {
            InvestigateObject();
        }
        else
        {
            Wander();
        }
    }

    private bool PlayerInFOV()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer < viewAngle / 2f)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Check if the player is within line of sight
            if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer))
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator StartChase()
    {
        isChasing = true;
        isPlayingAnimation = true;
        agent.isStopped = true;

        // Play spot player animation before chasing
        if (animator != null)
        {
            animator.SetTrigger("SpotPlayer");
        }

        yield return new WaitForSeconds(animationDelay);

        if (PlayerInFOV()) // Ensure the player is still in FOV
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            PlayAnimation(runAnimation); // Switch to run animation
        }
        else
        {
            // If player is no longer in FOV, return to wandering
            isChasing = false;
            GoToNextWanderPoint();
        }

        isPlayingAnimation = false;
    }

    private void ChasePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackDistance)
        {
            // Move toward the player and play run animation
            agent.isStopped = false;
            agent.SetDestination(player.position);
            PlayAnimation(runAnimation);
        }
        else
        {
            // Stop and attack if close enough
            agent.isStopped = true;
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        if (isAttacking || Time.time < lastAttackTime + attackCooldown) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger(attackAnimation);
        }

        // Damage the player if within range
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            // Apply damage to the player
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(20f); // Apply 20 damage, you can adjust as needed
            }
        }

        // Notify game that the player is caught
        onPlayerCaught?.Invoke();

        StartCoroutine(EndAttack());
    }

    private IEnumerator EndAttack()
    {
        // Wait for the attack animation to finish
        yield return new WaitForSeconds(1f); // Adjust based on your animation duration
        isAttacking = false;
    }

    private void OnPlayerDeath()
    {
        // Stop the AI from chasing or attacking when the player dies
        isPlayerDead = true;
        agent.isStopped = true;
        PlayAnimation(idleAnimation); // Ensure the AI is idle

        // Freeze the player's input when dead
        playerMovement.FreezeMovement();
        playerCamera.FreezeLook();
    }

    public void InvestigateObject(Transform target)
    {
        if (isChasing || isPlayingAnimation || isPlayerDead) return; // Don't interrupt chasing or animations

        isInvestigating = true;
        targetObject = target;
        agent.speed = investigateSpeed;
        agent.SetDestination(targetObject.position);

        // Play walk animation while investigating
        PlayAnimation(walkAnimation);
    }

    private void InvestigateObject()
    {
        if (targetObject == null || Vector3.Distance(transform.position, targetObject.position) < 1f)
        {
            isInvestigating = false;
            GoToNextWanderPoint();
        }
    }

    private void Wander()
    {
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            GoToNextWanderPoint();
        }

        // Play walk animation while wandering
        PlayAnimation(walkAnimation);
    }

    private void GoToNextWanderPoint()
    {
        if (wanderPoints.Length == 0) return;

        agent.speed = wanderSpeed;
        isInvestigating = false;
        isChasing = false;

        currentWanderIndex = (currentWanderIndex + 1) % wanderPoints.Length;
        agent.SetDestination(wanderPoints[currentWanderIndex].position);

        // Play walk animation when transitioning to wander state
        PlayAnimation(walkAnimation);
    }

    private void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the FOV in the Scene View
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 forward = transform.forward * viewRadius;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewRadius;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewRadius;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);
    }
}
