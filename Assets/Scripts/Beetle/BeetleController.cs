// BeetleController.cs
// Place: Assets/Scripts/Beetle/BeetleController.cs
// Master script that wires all beetle components together.
// Attach this to the root Beetle GameObject.

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BeetleIdentity))]
[RequireComponent(typeof(BeetleStats))]
[RequireComponent(typeof(BeetleLifeCycle))]
[RequireComponent(typeof(BeetleRLAgent))]
[RequireComponent(typeof(BeetleAnimationController))]
[RequireComponent(typeof(CharacterController))]
public class BeetleController : MonoBehaviour
{
    [Header("Is this the camera beetle?")]
    public bool isTracked = false;

    [Header("Spawner Reference")]
    public BeetleSpawner spawner;

    // ── Component References (auto-wired) ─────────────────────────────
    [HideInInspector] public BeetleIdentity    identity;
    [HideInInspector] public BeetleStats       stats;
    [HideInInspector] public BeetleLifeCycle   lifeCycle;
    [HideInInspector] public BeetleRLAgent     rl;
    [HideInInspector] public BeetleAnimationController anim;
    [HideInInspector] public CharacterController cc;

    // ── Movement ──────────────────────────────────────────────────────
    [Header("Movement Speeds")]
    public float walkSpeed   = 1.5f;
    public float runSpeed    = 4f;
    public float climbSpeed  = 1f;
    public float flySpeed    = 6f;

    [Header("Gravity")]
    public float gravity = -9.8f;

    private Vector3 _velocity;
    private bool _isGrounded;

    // ── RL Tick ───────────────────────────────────────────────────────
    private float _rlTickTimer = 0f;
    private const float RL_TICK_INTERVAL = 2f; // seconds between RL decisions

    // ── Nearby sensors ───────────────────────────────────────────────
    [Header("Sensor Radii")]
    public float foodSenseRadius  = 5f;
    public float enemySenseRadius = 8f;

    private Transform _targetFood;
    private Transform _targetEnemy;

    // ── State ─────────────────────────────────────────────────────────
    // private BeetleState _state = BeetleState.Idle; // Fixed CS0414

    void Awake()
    {
        identity = GetComponent<BeetleIdentity>();
        stats    = GetComponent<BeetleStats>();
        lifeCycle = GetComponent<BeetleLifeCycle>();
        rl       = GetComponent<BeetleRLAgent>();
        anim     = GetComponent<BeetleAnimationController>();
        cc       = GetComponent<CharacterController>();
    }

    void Start()
    {
        // Wire death event
        stats.OnDeath += HandleDeath;

        // Wire RL action changes to animation
        rl.OnActionChanged += HandleActionChanged;

        // Wire stage changes
        lifeCycle.OnStageChanged += OnStageChanged;
        lifeCycle.OnAdultExpired += OnAdultExpired;

        // Log beetle spawned
        StartCoroutine(LogSpawnToFirebase());
    }

    void Update()
    {
        if (stats.IsDead) return;

        _isGrounded = cc.isGrounded;

        // Gravity
        if (_isGrounded && _velocity.y < 0) _velocity.y = -2f;
        _velocity.y += gravity * Time.deltaTime;

        // RL tick
        _rlTickTimer += Time.deltaTime;
        if (_rlTickTimer >= RL_TICK_INTERVAL)
        {
            _rlTickTimer = 0f;
            ScanSensors();
            rl.Tick(
                hungry:    stats.Hunger < 30f,
                lowHP:     stats.HP < stats.MaxHP * 0.3f,
                enemyNear: _targetEnemy != null,
                foodNear:  _targetFood  != null,
                atNest:    false // TODO: wire nest proximity
            );
        }

        // Execute current RL action
        ExecuteAction(rl.CurrentAction);

        // Apply gravity
        cc.Move(_velocity * Time.deltaTime);
    }

    // ── Execute RL Decision ──────────────────────────────────────────

    private void ExecuteAction(BeetleRLAgent.BeetleAction action)
    {
        switch (action)
        {
            case BeetleRLAgent.BeetleAction.Walk:
                MoveTowardTarget(_targetFood?.position ?? RandomWanderTarget(), walkSpeed);
                anim.PlayWalk();
                break;

            case BeetleRLAgent.BeetleAction.Run:
                MoveTowardTarget(_targetFood?.position ?? RandomWanderTarget(), runSpeed);
                anim.PlayRun();
                break;

            case BeetleRLAgent.BeetleAction.Eat:
                if (_targetFood != null)
                {
                    MoveTowardTarget(_targetFood.position, walkSpeed);
                    if (Vector3.Distance(transform.position, _targetFood.position) < 1f)
                    {
                        stats.Eat(30f);
                        rl.GiveReward(+10f);
                        Destroy(_targetFood.gameObject);
                        _targetFood = null;
                    }
                }
                break;

            case BeetleRLAgent.BeetleAction.Fight:
                if (_targetEnemy != null)
                {
                    MoveTowardTarget(_targetEnemy.position, runSpeed);
                    if (Vector3.Distance(transform.position, _targetEnemy.position) < 1.5f)
                    {
                        anim.PlayAttack();
                        // Damage enemy via their stats
                        var enemyStats = _targetEnemy.GetComponent<BeetleStats>();
                        enemyStats?.TakeDamage(10f + stats.Strength * 0.1f);
                        stats.ConsumeEP(5f);
                    }
                }
                break;

            case BeetleRLAgent.BeetleAction.Fly:
                anim.SetFlying(true);
                FlyTowardTarget(_targetFood?.position ?? RandomWanderTarget());
                break;

            default:
                anim.PlayIdle();
                break;
        }
    }

    // ── Movement Helpers ─────────────────────────────────────────────

    private void MoveTowardTarget(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0f;
        if (dir.magnitude < 0.5f) return;

        dir.Normalize();
        cc.Move(dir * speed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
    }

    private void FlyTowardTarget(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        _velocity = dir * flySpeed;
        stats.ConsumeEP(1f * Time.deltaTime);
    }

    private Vector3 _wanderTarget;
    private float   _wanderTimer = 0f;
    private Vector3 RandomWanderTarget()
    {
        _wanderTimer -= Time.deltaTime;
        if (_wanderTimer <= 0f)
        {
            _wanderTarget = transform.position + new Vector3(
                Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
            _wanderTimer = Random.Range(3f, 8f);
        }
        return _wanderTarget;
    }

    // ── Sensors ───────────────────────────────────────────────────────

    private void ScanSensors()
    {
        // Food scan
        _targetFood  = FindNearest("Food", foodSenseRadius);
        // Enemy scan (other beetles)
        _targetEnemy = FindNearestBeetle(enemySenseRadius);
    }

    private Transform FindNearest(string tag, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        float best = float.MaxValue;
        Transform found = null;
        foreach (var h in hits)
        {
            if (!h.CompareTag(tag)) continue;
            float d = Vector3.Distance(transform.position, h.transform.position);
            if (d < best) { best = d; found = h.transform; }
        }
        return found;
    }

    private Transform FindNearestBeetle(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        float best = float.MaxValue;
        Transform found = null;
        foreach (var h in hits)
        {
            if (h.gameObject == gameObject) continue;
            var other = h.GetComponent<BeetleController>();
            if (other == null) continue;
            float d = Vector3.Distance(transform.position, h.transform.position);
            if (d < best) { best = d; found = h.transform; }
        }
        return found;
    }

    // ── Event Handlers ────────────────────────────────────────────────

    private void HandleActionChanged(BeetleRLAgent.BeetleAction action)
    {
        if (action == BeetleRLAgent.BeetleAction.Fly)
            anim.PlayFlightStart();
        else
            anim.SetFlying(false);
    }

    private void HandleDeath()
    {
        anim.PlayDeathSequence(
            Random.value > 0.6f ? DeathCategory.Normal
          : Random.value > 0.5f ? DeathCategory.Headless
          :                       DeathCategory.UpsideDown);

        rl.SaveMemory();

        if (isTracked)
            StartCoroutine(RespawnTracked());
        else
            StartCoroutine(RespawnBasic());
    }

    private IEnumerator RespawnTracked()
    {
        yield return new WaitForSeconds(5f);
        anim.PlayCrashExit();
        stats.Revive();
        Debug.Log($"[{identity.BeetleName}] Respawned with memory.");
    }

    private IEnumerator RespawnBasic()
    {
        yield return new WaitForSeconds(5f);
        stats.Revive();
        // Move to random spawn point
        transform.position = spawner != null
            ? spawner.GetRandomSpawnPoint()
            : Vector3.zero;
    }

    private void OnStageChanged(BeetleStage newStage)
    {
        Debug.Log($"[{identity.BeetleName}] Entered stage: {newStage}");
        anim.PlayShake(); // celebrate stage up
    }

    private void OnAdultExpired()
    {
        // Die permanently and rebirth with new identity
        Debug.Log($"[{identity.BeetleName}] Adult lifespan over — rebirthling...");
        spawner?.RebornBeetle(this);
    }

    private IEnumerator LogSpawnToFirebase()
    {
        yield return new WaitForSeconds(1f); // wait for identity to init
        if (FirebaseManager.Instance == null || identity == null) yield break;

        string json = $"{{\"type\":\"spawn\",\"beetleId\":\"{identity.HexId}\"," +
                      $"\"isTracked\":{isTracked.ToString().ToLower()}}}";
        FirebaseManager.Instance.Put(
            $"events/{System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", json);
    }
}
