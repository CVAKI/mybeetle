// BeetleHUD.cs
// Place: Assets/Scripts/UI/BeetleHUD.cs
// Displays HP, EP, Hunger, Strength for the tracked beetle
// Requires: Canvas (Screen Space Overlay) with assigned UI elements

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BeetleHUD : MonoBehaviour
{
    [Header("Bars")]
    public Slider hpBar;
    public Slider epBar;
    public Slider hungerBar;
    public Slider strengthBar;

    [Header("Bar Fill Colors")]
    public Image hpFill;
    public Image epFill;
    public Image hungerFill;

    [Header("Labels")]
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI stageLabel;
    public TextMeshProUGUI genLabel;
    public TextMeshProUGUI actionLabel;
    public TextMeshProUGUI ageLabel;

    [Header("Colors")]
    public Color hpHighColor    = new Color(0.2f, 0.9f, 0.2f);
    public Color hpMidColor     = new Color(0.9f, 0.7f, 0.1f);
    public Color hpLowColor     = new Color(0.9f, 0.1f, 0.1f);

    private BeetleController _target;
    private float _strengthMax = 200f; // expected max for bar display

    void Start()
    {
        // Auto-find tracked beetle
        var beetles = FindObjectsByType<BeetleController>(FindObjectsSortMode.None);
        foreach (var b in beetles)
            if (b.isTracked) { SetTarget(b); break; }
    }

    public void SetTarget(BeetleController beetle)
    {
        _target = beetle;
    }

    void Update()
    {
        if (_target == null || _target.stats == null) return;

        var s  = _target.stats;
        var lc = _target.lifeCycle;
        var id = _target.identity;
        var rl = _target.rl;

        // ── Bars ─────────────────────────────────────────────────────
        SetBar(hpBar, s.HP, s.MaxHP);
        SetBar(epBar, s.EP, s.MaxEP);
        SetBar(hungerBar, s.Hunger, 100f);
        SetBar(strengthBar, s.Strength, _strengthMax);

        // ── HP color ─────────────────────────────────────────────────
        if (hpFill != null)
        {
            float ratio = s.HP / s.MaxHP;
            hpFill.color = ratio > 0.6f ? hpHighColor
                         : ratio > 0.3f ? hpMidColor
                         :                hpLowColor;
        }

        // ── Hunger color ─────────────────────────────────────────────
        if (hungerFill != null)
            hungerFill.color = s.Hunger < 30f ? hpLowColor : hpHighColor;

        // ── Text labels ───────────────────────────────────────────────
        if (nameLabel  != null) nameLabel.text  = id?.BeetleName ?? "...";
        if (stageLabel != null) stageLabel.text = lc?.CurrentStage.ToString() ?? "";
        if (genLabel   != null) genLabel.text   = $"Gen {id?.Generation ?? 1}";
        if (actionLabel != null) actionLabel.text = rl?.CurrentAction.ToString() ?? "";
        if (ageLabel   != null)
        {
            int years  = Mathf.FloorToInt(lc?.AgeYears ?? 0f);
            int months = Mathf.FloorToInt(
                (GameTimeManager.Instance?.TotalGameMonths ?? 0f) % 12f);
            ageLabel.text = $"Age: {years}y {months}m";
        }
    }

    private void SetBar(Slider bar, float val, float max)
    {
        if (bar == null) return;
        bar.maxValue = max;
        bar.value    = Mathf.Max(0f, val);
    }
}
