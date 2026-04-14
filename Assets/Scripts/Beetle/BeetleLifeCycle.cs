// BeetleLifeCycle.cs
// Place: Assets/Scripts/Beetle/BeetleLifeCycle.cs
// Tracks age, stage transitions (Child → Youth → Adult), and triggers events

using System;
using UnityEngine;

public class BeetleLifeCycle : MonoBehaviour
{
    [Header("Config")]
    public StageConfig config = new StageConfig();

    [Header("References")]
    public BeetleIdentity identity;
    public BeetleStats    stats;

    // ── State ─────────────────────────────────────────────────────────
    public BeetleStage  CurrentStage  { get; private set; } = BeetleStage.Child;
    public BeetleGender Gender        { get; private set; }

    // Age in in-game years
    public float AgeYears { get; private set; } = 0f;

    // Adult-only: years spent in adult stage (for mating gate & death)
    public float AdultYears { get; private set; } = 0f;

    public bool CanMate => CurrentStage == BeetleStage.Adult
                        && AdultYears >= config.matingReadyAge;

    // ── Events ────────────────────────────────────────────────────────
    public event Action<BeetleStage> OnStageChanged;   // new stage
    public event Action              OnAdultExpired;   // 10 adult years, no offspring

    private bool _adultExpired = false;

    void Start()
    {
        Gender = identity?.Gender ?? BeetleGender.Male;
        ApplyScale();

        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnNewYear += OnNewYear;
    }

    void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnNewYear -= OnNewYear;
    }

    // ── Tick ──────────────────────────────────────────────────────────

    private void OnNewYear()
    {
        AgeYears++;

        if (CurrentStage == BeetleStage.Adult)
            AdultYears++;

        EvaluateStage();
        stats?.RefreshMaxStats();
        SyncToFirebase();
    }

    private void EvaluateStage()
    {
        BeetleStage before = CurrentStage;

        if (AgeYears >= config.childDuration + config.youthDuration)
        {
            if (CurrentStage != BeetleStage.Adult)
            {
                CurrentStage = BeetleStage.Adult;
                AdultYears   = 0f;
            }

            // Check adult expiry
            if (!_adultExpired && AdultYears >= config.adultLifespan)
            {
                _adultExpired = true;
                Debug.Log($"[{identity?.BeetleName}] Adult lifespan expired.");
                OnAdultExpired?.Invoke();
            }
        }
        else if (AgeYears >= config.childDuration)
        {
            CurrentStage = BeetleStage.Youth;
        }

        if (CurrentStage != before)
        {
            Debug.Log($"[{identity?.BeetleName}] Stage → {CurrentStage}");
            ApplyScale();
            OnStageChanged?.Invoke(CurrentStage);
        }
    }

    // ── Scale by Stage ────────────────────────────────────────────────

    private void ApplyScale()
    {
        float s = CurrentStage switch
        {
            BeetleStage.Child => config.childScale,
            BeetleStage.Youth => config.youthScale,
            _                 => config.adultScale
        };
        transform.localScale = Vector3.one * s;
    }

    // ── Firebase Sync ─────────────────────────────────────────────────

    private void SyncToFirebase()
    {
        if (FirebaseManager.Instance == null || identity == null) return;

        string json = $"{{" +
            $"\"stage\":\"{CurrentStage}\"," +
            $"\"ageYears\":{AgeYears:F1}," +
            $"\"adultYears\":{AdultYears:F1}," +
            $"\"canMate\":{CanMate.ToString().ToLower()}" +
            $"}}";

        FirebaseManager.Instance.Patch($"beetles/{identity.HexId}/lifecycle", json);
    }
}
