// GameTimeManager.cs
// Place: Assets/Scripts/Core/GameTimeManager.cs
// 10 real minutes = 1 in-game month
// 1 in-game day  = 20 real seconds
// 1 in-game year = 120 real minutes (2 hours)

using System;
using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    // ── Time Constants ───────────────────────────────────────────────
    public const float REAL_SECONDS_PER_GAME_MONTH = 600f;   // 10 minutes
    public const float REAL_SECONDS_PER_GAME_DAY   = 20f;    // 10min / 30 days
    public const float REAL_SECONDS_PER_GAME_YEAR  = 7200f;  // 2 real hours
    // ────────────────────────────────────────────────────────────────

    // Current world time (in game-months elapsed since start)
    public float TotalGameMonths { get; private set; } = 0f;

    public int CurrentMonth => Mathf.FloorToInt(TotalGameMonths % 12);
    public int CurrentYear  => Mathf.FloorToInt(TotalGameMonths / 12);
    public float CurrentDayFraction => (TotalGameMonths * 30f) % 1f; // 0..1 within a day

    // Events
    public event Action OnNewDay;
    public event Action OnNewMonth;
    public event Action OnNewYear;

    private float _dayAccum   = 0f;
    private float _monthAccum = 0f;
    private float _yearAccum  = 0f;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Advance total game months
        TotalGameMonths += dt / REAL_SECONDS_PER_GAME_MONTH;

        // Day tick
        _dayAccum += dt;
        if (_dayAccum >= REAL_SECONDS_PER_GAME_DAY)
        {
            _dayAccum -= REAL_SECONDS_PER_GAME_DAY;
            OnNewDay?.Invoke();
        }

        // Month tick
        _monthAccum += dt;
        if (_monthAccum >= REAL_SECONDS_PER_GAME_MONTH)
        {
            _monthAccum -= REAL_SECONDS_PER_GAME_MONTH;
            OnNewMonth?.Invoke();
        }

        // Year tick
        _yearAccum += dt;
        if (_yearAccum >= REAL_SECONDS_PER_GAME_YEAR)
        {
            _yearAccum -= REAL_SECONDS_PER_GAME_YEAR;
            OnNewYear?.Invoke();
        }
    }

    // ── Utilities ────────────────────────────────────────────────────

    /// Convert game-years to real seconds
    public static float GameYearsToRealSeconds(float gameYears)
        => gameYears * REAL_SECONDS_PER_GAME_YEAR;

    /// Convert real seconds elapsed to game-years
    public static float RealSecondsToGameYears(float realSeconds)
        => realSeconds / REAL_SECONDS_PER_GAME_YEAR;
}
