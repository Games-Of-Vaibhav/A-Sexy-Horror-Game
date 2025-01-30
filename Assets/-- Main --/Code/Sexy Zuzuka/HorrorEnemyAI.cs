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
    [Header("Chase Settings")]
    public float raycastDelayAfterChaseStart = 2f; // Delay before starting raycast detection after the chase begins
    private float raycastDelayTimer = 0f; // Tracks the delay timer


    [Header("Raycast Settings")]
    public Vector3 aiRayOffset = new Vector3(0, 1.5f, 0); // Offset for AI's raycast origin
    public Vector3 playerRayOffset = new Vector3(0, 1.5f, 0); // Offset for player's raycast target
    public float rayCheckInterval = 0.2f; // How frequently to check for obstacles (in seconds)

    [Header("Audio")]
    public AudioSource screamSound;

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

    private float rayCheckTimer = 0f;

    private void Update()
    {
        if (isPlayerDead) return; // Stop all actions if the player is dead

        if (isPlayingAnimation) return; // Skip detection if playing animation

        // Field of View Detection
        if (!isChasing && PlayerInFOV())
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

        // Disable player detection for a short duration
        float disableDetectionTime = animationDelay + 1f; // Adjust extra time if needed
        StartCoroutine(DisablePlayerDetection(disableDetectionTime));

        // Play the SpotPlayer animation
        if (animator != null)
        {
            animator.SetTrigger("SpotPlayer");
            if (screamSound)
                screamSound.Play();
        }

        yield return new WaitForSeconds(animationDelay); // Wait for the animation to finish

        // Transition to chasing state
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        PlayAnimation(runAnimation); // Switch to run animation
        isPlayingAnimation = false;

        Debug.Log("AI starts chasing the player.");
    }

    private IEnumerator DisablePlayerDetection(float duration)
    {
        bool originalDetectionState = isChasing;
        isChasing = true; // Prevent detection logic from reverting to wandering

        yield return new WaitForSeconds(duration);

        // Restore detection state after the duration
        isChasing = originalDetectionState;
    }

    private void ChasePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Handle raycast delay after the chase starts
        if (raycastDelayTimer > 0f)
        {
            raycastDelayTimer -= Time.deltaTime;
        }
        else
        {
            // Perform obstacle detection with raycast after the delay
            if (!CanSeePlayer())
            {
                isChasing = false;
                GoToNextWanderPoint();
                return;
            }
        }

        if (distanceToPlayer > attackDistance)
        {
            // Move toward the player and play run animation
            agent.isStopped = false;
            agent.SetDestination(player.position);
            PlayAnimation(runAnimation);
        }
        else
        {
            // If AI reaches the player position but the player is no longer there
            if (agent.remainingDistance <= agent.stoppingDistance && !CanSeePlayer())
            {
                Debug.Log("Lost sight of player. Returning to wander state.");
                isChasing = false;
                GoToNextWanderPoint();
                return;
            }

            // Stop and attack if the player is still in range
            agent.isStopped = true;
            AttackPlayer();
        }
    }

    private bool CanSeePlayer()
    {
        // Calculate raycast origins with offsets
        Vector3 aiOrigin = transform.position + aiRayOffset;
        Vector3 playerTarget = player.position + playerRayOffset;

        // Direction of the ray
        Vector3 directionToPlayer = (playerTarget - aiOrigin).normalized;
        float distanceToPlayer = Vector3.Distance(aiOrigin, playerTarget);

        // Perform the raycast
        if (Physics.Raycast(aiOrigin, directionToPlayer, out RaycastHit hit, distanceToPlayer, obstacleLayer))
        {
            // If the ray hits something in the obstacle layer, the AI cannot see the player
            return false;
        }

        // No obstacles detected
        return true;
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

        // Start a coroutine to apply damage after a delay
        StartCoroutine(DelayedDamageToPlayer(2)); // Adjust the delay time as needed
    }

    private IEnumerator DelayedDamageToPlayer(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Check if the player is still within attack range
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(20f); // Apply 20 damage, adjust as needed
            }

            // Notify game that the player is caught
            onPlayerCaught?.Invoke();
        }

        // Finish attack state
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

        // Freeze the player's input when dead
        playerMovement.FreezeMovement();
        playerCamera.FreezeLook();

        // Delay before transitioning to the idle animation
        StartCoroutine(DelayedIdleAfterDeath(1.5f)); // Adjust delay time as needed
    }

    private IEnumerator DelayedIdleAfterDeath(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Play idle animation after the delay
        PlayAnimation(idleAnimation); // Ensure the AI is idle
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
        isAttacking = false;

        // Pick the next waypoint
        currentWanderIndex = (currentWanderIndex + 1) % wanderPoints.Length;
        agent.SetDestination(wanderPoints[currentWanderIndex].position);

        // Play walk animation when transitioning to wander state
        PlayAnimation(walkAnimation);
        Debug.Log("AI is now wandering.");
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 forward = transform.forward * viewRadius;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewRadius;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewRadius;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + forward);
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);

        // Draw raycast between AI and player
        if (player != null)
        {
            Vector3 aiOrigin = transform.position + aiRayOffset;
            Vector3 playerTarget = player.position + playerRayOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(aiOrigin, playerTarget);
        }
    }

}
