// BeetleStats.cs
// Place: Assets/Scripts/Beetle/BeetleStats.cs
// Manages HP, EP, Hunger, Strength — ticks hunger damage daily
// Syncs to Firebase every 30 real seconds

using System;
using UnityEngine;

public class BeetleStats : MonoBehaviour
{
    [Header("References")]
    public BeetleIdentity identity;
    public BeetleLifeCycle lifeCycle;

    // ── Live Stats ────────────────────────────────────────────────────
    public float HP       { get; private set; }
    public float MaxHP    { get; private set; }
    public float EP       { get; private set; }   // always == MaxHP
    public float MaxEP    => MaxHP;
    public float Hunger   { get; private set; } = 100f; // 100 = full, 0 = starving
    public float Strength { get; private set; } = 0f;

    // ── Events ────────────────────────────────────────────────────────
    public event Action OnDeath;
    public event Action<float> OnDamaged;   // passes damage amount
    public event Action<float> OnHealed;

    // ── Constants ────────────────────────────────────────────────────
    private const float HUNGER_DRAIN_PER_DAY   = 20f;   // hunger points per in-game day
    private const float HUNGER_DAMAGE_PER_DAY  = 5f;    // HP lost if hunger reaches 0
    private const float EP_REGEN_RATE          = 2f;    // EP/sec when idle
    private const float FIREBASE_SYNC_INTERVAL = 30f;

    private bool _isDead = false;
    private float _syncTimer = 0f;

    void Start()
    {
        // Set max HP based on current stage (called after BeetleLifeCycle sets stage)
        RefreshMaxStats();
        HP = MaxHP;
        EP = MaxHP;

        // Subscribe to day tick for hunger drain
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnNewDay += OnNewDay;
    }

    void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnNewDay -= OnNewDay;
    }

    void Update()
    {
        if (_isDead) return;

        // EP passive regen when not fighting/flying
        if (EP < MaxEP)
            EP = Mathf.Min(MaxEP, EP + EP_REGEN_RATE * Time.deltaTime);

        // Firebase sync
        _syncTimer += Time.deltaTime;
        if (_syncTimer >= FIREBASE_SYNC_INTERVAL)
        {
            _syncTimer = 0f;
            SyncToFirebase();
        }
    }

    // ── Public Methods ────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        HP = Mathf.Max(0f, HP - amount);
        OnDamaged?.Invoke(amount);
        if (HP <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (_isDead) return;
        float healed = Mathf.Min(MaxHP - HP, amount);
        HP += healed;
        OnHealed?.Invoke(healed);
    }

    public void ConsumeEP(float amount)
    {
        EP = Mathf.Max(0f, EP - amount);
    }

    public void Eat(float nutritionValue)
    {
        Hunger = Mathf.Min(100f, Hunger + nutritionValue);
        // Eating also slightly heals
        Heal(nutritionValue * 0.1f);
    }

    public void GainStrength(float amount = 5f)
    {
        Strength += amount;
        Debug.Log($"[{identity?.BeetleName}] Strength +{amount} → {Strength}");
    }

    /// Call this when stage changes so MaxHP updates
    public void RefreshMaxStats()
    {
        if (lifeCycle == null) return;

        switch (lifeCycle.CurrentStage)
        {
            case BeetleStage.Child:
                MaxHP = 50f;
                break;
            case BeetleStage.Youth:
                MaxHP = Mathf.Lerp(70f, 80f, Strength / 100f);
                break;
            case BeetleStage.Adult:
                MaxHP = Mathf.Lerp(100f, 200f, Strength / 200f);
                break;
        }
        // Clamp current HP to new max
        HP = Mathf.Min(HP, MaxHP);
        EP = Mathf.Min(EP, MaxEP);
    }

    // ── Internal ─────────────────────────────────────────────────────

    private void OnNewDay()
    {
        if (_isDead) return;

        // Drain hunger
        Hunger = Mathf.Max(0f, Hunger - HUNGER_DRAIN_PER_DAY);

        // If starving, lose HP
        if (Hunger <= 0f)
        {
            TakeDamage(HUNGER_DAMAGE_PER_DAY);
            Debug.Log($"[{identity?.BeetleName}] Starving! -{HUNGER_DAMAGE_PER_DAY} HP");
        }
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;
        Debug.Log($"[{identity?.BeetleName}] DIED");
        SyncToFirebase();
        OnDeath?.Invoke();
    }

    private void SyncToFirebase()
    {
        if (FirebaseManager.Instance == null || identity == null) return;

        string json = $"{{" +
            $"\"hp\":{HP:F1}," +
            $"\"maxHp\":{MaxHP:F1}," +
            $"\"ep\":{EP:F1}," +
            $"\"hunger\":{Hunger:F1}," +
            $"\"strength\":{Strength:F1}" +
            $"}}";

        FirebaseManager.Instance.Patch($"beetles/{identity.HexId}/stats", json);
    }

    public bool IsDead => _isDead;
    public void Revive() { _isDead = false; HP = MaxHP * 0.5f; EP = MaxEP * 0.5f; Hunger = 50f; }
}
