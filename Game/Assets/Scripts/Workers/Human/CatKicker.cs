using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class HumanBrain : MonoBehaviour
{
    public static HumanBrain Instance;

    [Header("Control Settings")]
    public bool isAIControlled = false;

    [Header("Movement Settings (Manual Player Control)")]
    [SerializeField] private float playerMoveSpeed = 7.0f;
    [SerializeField] private float playerLookSensitivity = 2.0f;

    [Header("Combat & Ability Tuning")]
    [SerializeField] private float attackDamage = 35f;
    [SerializeField] private float kickRange = 2.2f;
    [SerializeField] private float sprayRange = 6.0f;
    [SerializeField] private float sprayDamagePerSecond = 15f;
    
    [Header("AI Logic Configuration")]
    [SerializeField] private float aiScanRadius = 25f;
    [SerializeField] private float aiAttackCooldown = 1.5f;

    [Header("Audio Audio Effects")]
    [SerializeField] private AudioClip waterSpraySound;
    [SerializeField] private AudioClip kickSound;

    // Internal components cache
    private Rigidbody rb;
    private NavMeshAgent navAgent;
    private CatBrainController activeTargetCat;
    private float nextAttackTime;
    private Vector3 manualMoveInput;
    private float cameraPitch = 0.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        rb = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        rb.freezeRotation = true;
        ToggleControlState();
    }

    private void Update()
    {
        if (isAIControlled)
        {
            ExecuteAIStateMachine();
        }
        else
        {
            ExecuteManualPlayerInputProcessing();
        }
    }

    private void FixedUpdate()
    {
        // Physics-based manual player movement calculations
        if (!isAIControlled && manualMoveInput.magnitude > 0.01f)
        {
            Vector3 calculatedMovement = transform.TransformDirection(manualMoveInput) * playerMoveSpeed;
            rb.linearVelocity = new Vector3(calculatedMovement.x, rb.linearVelocity.y, calculatedMovement.z);
        }
    }

    /// <summary>
    /// Configures underlying components dynamically depending on who or what is driving the character.
    /// </summary>
    public void ToggleControlState()
    {
        if (isAIControlled)
        {
            navAgent.enabled = true;
            rb.isKinematic = true; // Let NavMeshAgent take full physics authority
        }
        else
        {
            navAgent.enabled = false;
            rb.isKinematic = false; // Turn authority back over to Rigidbody forces
        }
    }

    // --- 🎮 MANUAL PLAYER ACTIONS (Layer 4 API Hooks) ---

    public void ReceiveMoveInput(Vector2 inputs)
    {
        manualMoveInput = new Vector3(inputs.x, 0, inputs.y).normalized;
    }

    public void ReceiveLookInput(Vector2 lookDelta)
    {
        if (isAIControlled) return;

        // Horizontal rotation (Yaw) turns the actual player body
        transform.Rotate(Vector3.up * lookDelta.x * playerLookSensitivity);

        // Vertical look calculation (Pitch) would sit on your camera component
        cameraPitch -= lookDelta.y * playerLookSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -60f, 60f);
    }

    public void ExecuteSpray()
    {
        // Long-range continuous stream calculation
        if (AudioManagerHub.Instance != null && waterSpraySound != null && Time.frameCount % 15 == 0)
        {
            AudioManagerHub.Instance.PlaySpatialExplosiveSFX(waterSpraySound, transform.position, 0.4f);
        }

        PerformRaycastHitCheck(sprayRange, sprayDamagePerSecond * Time.deltaTime, "Sprayed");
    }

    public void ExecuteKick()
    {
        // High-damage instant blast burst
        if (Time.time < nextAttackTime) return;
        
        nextAttackTime = Time.time + aiAttackCooldown;

        if (AudioManagerHub.Instance != null && kickSound != null)
        {
            AudioManagerHub.Instance.PlaySpatialExplosiveSFX(kickSound, transform.position);
        }

        PerformRaycastHitCheck(kickRange, attackDamage, "Kicked");
    }

    private void PerformRaycastHitCheck(float range, float damage, string actionMessage)
    {
        Ray ray = new Ray(transform.position + Vector3.up * 1f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (hit.collider.TryGetComponent<CatBrainController>(out CatBrainController cat))
            {
                Debug.Log($"Human {actionMessage} {hit.collider.name}!");
                cat.TakeDamage(damage);

                // If kicking, apply an instant physical velocity knockback push to the cat
                if (actionMessage == "Kicked" && hit.collider.TryGetComponent<Rigidbody>(out Rigidbody catRb))
                {
                    Vector3 pushDirection = (hit.collider.transform.position - transform.position).normalized;
                    pushDirection.y = 0.5f; // Pop them slightly up into the air
                    catRb.AddForce(pushDirection * 15f, ForceMode.Impulse);
                }
            }
        }
    }

    // --- 🤖 AI AUTOMATION STATE MACHINE ---

    private void ExecuteAIStateMachine()
    {
        if (activeTargetCat == null || activeTargetCat.isSleeping)
        {
            FindClosestActiveCatPlayer();
            return;
        }

        float targetDistance = Vector3.Distance(transform.position, activeTargetCat.transform.position);
        navAgent.SetDestination(activeTargetCat.transform.position);

        // Attack routine checks
        if (targetDistance <= kickRange && Time.time >= nextAttackTime)
        {
            ExecuteKick();
        }
        else if (targetDistance <= sprayRange)
        {
            ExecuteSpray(); // Sustain water spray stream while closing the distance gap
        }
    }

    private void FindClosestActiveCatPlayer()
    {
        CatBrainController[] catPool = FindObjectsByType<CatBrainController>(FindObjectsSortMode.None);
        float closestDistance = aiScanRadius;
        CatBrainController bestTarget = null;

        foreach (var cat in catPool)
        {
            if (cat.isSleeping) continue;

            float dist = Vector3.Distance(transform.position, cat.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                bestTarget = cat;
            }
        }

        activeTargetCat = bestTarget;
    }

    private void ExecuteManualPlayerInputProcessing()
    {
        // Legacy input fallbacks for quick testing in the editor sandbox
        if (Input.GetButton("Fire1")) ExecuteSpray();
        if (Input.GetButtonDown("Fire2")) ExecuteKick();
    }
}
