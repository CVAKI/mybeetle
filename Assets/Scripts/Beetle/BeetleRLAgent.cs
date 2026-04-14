// BeetleRLAgent.cs
// Place: Assets/Scripts/Beetle/BeetleRLAgent.cs
// Simple reward-based Q-learning agent.
// At birth: only WALK is unlocked. Other actions unlock as beetle earns enough Q-value.
// Memory (Q-table) is saved to Firebase on death and loaded on respawn.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeetleRLAgent : MonoBehaviour
{
    // ── Actions the beetle can learn ──────────────────────────────────
    public enum BeetleAction
    {
        Walk = 0,       // always unlocked from birth
        Run,            // unlocked when walk Q > 5
        Climb,          // unlocked when run Q > 8
        Fly,            // unlocked when climb Q > 10
        Fight,          // unlocked in youth stage
        Eat,
        BuildNest,      // adult stage only
        GuardMate,      // adult stage, after mating
        Count
    }

    // ── Q-Table ───────────────────────────────────────────────────────
    // State is simplified: [Hungry, LowHP, EnemyNear, FoodNear, AtNest]
    // Each state-action pair stores a Q value

    [Serializable]
    public class QTable
    {
        public float[] values = new float[(int)BeetleAction.Count * 32]; // 32 states
    }

    // ── Config ────────────────────────────────────────────────────────
    [Header("RL Hyperparameters")]
    [Range(0f,1f)] public float learningRate   = 0.1f;
    [Range(0f,1f)] public float discountFactor = 0.9f;
    [Range(0f,1f)] public float explorationRate = 0.3f; // epsilon-greedy

    [Header("References")]
    public BeetleIdentity  identity;
    public BeetleStats     stats;
    public BeetleLifeCycle lifeCycle;

    // ── State ─────────────────────────────────────────────────────────
    private QTable _qTable = new QTable();
    private BeetleAction _currentAction = BeetleAction.Walk;
    private int _lastState = 0;

    // Unlock thresholds (Q-value needed to unlock next behaviour)
    private Dictionary<BeetleAction, float> _unlockThreshold = new()
    {
        { BeetleAction.Walk,      0f   },
        { BeetleAction.Run,       5f   },
        { BeetleAction.Climb,     8f   },
        { BeetleAction.Fly,       10f  },
        { BeetleAction.Fight,     5f   },
        { BeetleAction.Eat,       3f   },
        { BeetleAction.BuildNest, 15f  },
        { BeetleAction.GuardMate, 20f  },
    };

    public event Action<BeetleAction> OnActionChanged;

    void Start()
    {
        // Try to load memory from Firebase
        LoadMemory();
    }

    // ── Public: Tick the agent (call every few seconds, not every frame) ──

    public void Tick(bool hungry, bool lowHP, bool enemyNear, bool foodNear, bool atNest)
    {
        int state = EncodeState(hungry, lowHP, enemyNear, foodNear, atNest);
        BeetleAction chosen = ChooseAction(state);

        if (chosen != _currentAction)
        {
            _currentAction = chosen;
            OnActionChanged?.Invoke(chosen);
        }

        _lastState = state;
    }

    /// Called by the game after an action to provide reward
    public void GiveReward(float reward)
    {
        int state  = _lastState;
        int action = (int)_currentAction;

        float oldQ = GetQ(state, action);
        float maxNextQ = GetMaxQ(state);
        float newQ = oldQ + learningRate * (reward + discountFactor * maxNextQ - oldQ);
        SetQ(state, action, newQ);

        // Reduce exploration over time (experience)
        explorationRate = Mathf.Max(0.05f, explorationRate - 0.0001f);
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private BeetleAction ChooseAction(int state)
    {
        // Epsilon-greedy
        if (UnityEngine.Random.value < explorationRate)
        {
            // Random unlocked action
            return RandomUnlockedAction();
        }

        // Greedy: best Q among unlocked actions
        float best = float.MinValue;
        BeetleAction bestAction = BeetleAction.Walk;

        for (int a = 0; a < (int)BeetleAction.Count; a++)
        {
            BeetleAction act = (BeetleAction)a;
            if (!IsUnlocked(act)) continue;
            float q = GetQ(state, a);
            if (q > best) { best = q; bestAction = act; }
        }
        return bestAction;
    }

    private bool IsUnlocked(BeetleAction action)
    {
        // Stage gates
        if (action == BeetleAction.BuildNest &&
            lifeCycle?.CurrentStage != BeetleStage.Adult) return false;
        if (action == BeetleAction.GuardMate &&
            lifeCycle?.CurrentStage != BeetleStage.Adult) return false;
        if (action == BeetleAction.Fight &&
            lifeCycle?.CurrentStage == BeetleStage.Child) return false;

        // Q-value gate: check if prerequisite skill has enough Q
        float threshold = _unlockThreshold.GetValueOrDefault(action, 99f);
        if (threshold <= 0f) return true;

        // Use the average Q for this action across all states
        float avgQ = 0f;
        for (int s = 0; s < 32; s++) avgQ += GetQ(s, (int)action);
        avgQ /= 32f;
        return avgQ >= threshold;
    }

    private BeetleAction RandomUnlockedAction()
    {
        var unlocked = new List<BeetleAction>();
        for (int a = 0; a < (int)BeetleAction.Count; a++)
            if (IsUnlocked((BeetleAction)a)) unlocked.Add((BeetleAction)a);
        return unlocked.Count > 0
            ? unlocked[UnityEngine.Random.Range(0, unlocked.Count)]
            : BeetleAction.Walk;
    }

    private int EncodeState(bool hungry, bool lowHP, bool enemyNear, bool foodNear, bool atNest)
    {
        // 5-bit state encoding → 0-31
        return (hungry ? 1 : 0)
             | (lowHP   ? 2 : 0)
             | (enemyNear ? 4 : 0)
             | (foodNear  ? 8 : 0)
             | (atNest    ? 16 : 0);
    }

    private float GetQ(int state, int action)
        => _qTable.values[state * (int)BeetleAction.Count + action];

    private void SetQ(int state, int action, float val)
        => _qTable.values[state * (int)BeetleAction.Count + action] = val;

    private float GetMaxQ(int state)
    {
        float max = float.MinValue;
        for (int a = 0; a < (int)BeetleAction.Count; a++)
            max = Mathf.Max(max, GetQ(state, a));
        return max;
    }

    // ── Firebase Memory Persistence ───────────────────────────────────

    public void SaveMemory()
    {
        if (FirebaseManager.Instance == null || identity == null) return;
        string json = JsonUtility.ToJson(_qTable);
        // Escape for Firebase string field
        string escaped = json.Replace("\"", "\\\"");
        string payload = $"{{\"rlMemoryJson\":\"{escaped}\"," +
                         $"\"explorationRate\":{explorationRate:F4}}}";
        FirebaseManager.Instance.Patch($"beetles/{identity.HexId}/rl", payload);
        Debug.Log($"[RL] Memory saved for {identity.BeetleName}");
    }

    private void LoadMemory()
    {
        if (FirebaseManager.Instance == null || identity == null) return;
        FirebaseManager.Instance.Get($"beetles/{identity.HexId}/rl", OnMemoryLoaded);
    }

    private void OnMemoryLoaded(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "null") return;
        try
        {
            // Extract rlMemoryJson field
            int start = json.IndexOf("\"rlMemoryJson\":\"") + 16;
            if (start < 16) return;
            int end = json.IndexOf("\"}", start);
            if (end < 0) return;
            string memJson = json.Substring(start, end - start).Replace("\\\"", "\"");
            JsonUtility.FromJsonOverwrite(memJson, _qTable);
            Debug.Log($"[RL] Memory loaded for {identity.BeetleName}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[RL] Failed to parse memory: {e.Message}");
        }
    }

    public BeetleAction CurrentAction => _currentAction;
}
