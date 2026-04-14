// BeetleData.cs
// Place: Assets/Scripts/Core/BeetleData.cs
// Shared enums, structs, constants, and ALL 78 animation clip names for the beetle system

using System;
using UnityEngine;

// ── Enums ────────────────────────────────────────────────────────────

public enum BeetleStage   { Child, Youth, Adult }
public enum BeetleGender  { Male, Female }
public enum BeetleState   { Idle, Walking, Running, Climbing, Flying, Fighting,
                            Eating, NestBuilding, Guarding, Dead }

// ── Death Category ────────────────────────────────────────────────────
// Used by BeetleAnimationController to pick the correct death animation set
public enum DeathCategory
{
    Normal,       // Standard beetle death  → Beetle_ clips
    Headless,     // Cody_ headless sequence → Cody_ clips
    UpsideDown    // May_ upside-down death  → May_ clips
}

// ── Life Stage Config ─────────────────────────────────────────────────

[Serializable]
public class StageConfig
{
    [Header("Stage: Child")]
    public float childBaseHP     = 50f;
    public float childDuration   = 2f;   // in-game years

    [Header("Stage: Youth")]
    public float youthMinHP      = 70f;
    public float youthMaxHP      = 80f;
    public float youthDuration   = 5f;   // in-game years

    [Header("Stage: Adult")]
    public float adultMinHP      = 100f;
    public float adultMaxHP      = 200f;
    public float adultLifespan   = 10f;  // in-game years
    public float matingReadyAge  = 2f;   // years into adult stage before can mate

    [Header("Scales")]
    public float childScale      = 0.4f;
    public float youthScale      = 0.7f;
    public float adultScale      = 1.0f;
}

// ── Beetle Save Data (stored in Firebase) ────────────────────────────

[Serializable]
public class BeetleSaveData
{
    public string hexId;
    public string name;
    public string gender;         // "Male" | "Female"
    public string stage;          // "Child" | "Youth" | "Adult"
    public float  hp;
    public float  maxHp;
    public float  ep;
    public float  maxEp;
    public float  hunger;
    public float  strength;
    public float  ageYears;
    public int    generation;
    public string parentId;
    public bool   isTracked;      // true = camera beetle
    public string rlMemoryJson;   // serialised RL memory blob
}

// ── Animation Clip Names ──────────────────────────────────────────────
// IMPORTANT: Unity FBX animations require the full path format:
//   "Object_5|Beetle_ao|ClipName"
// All 78 clips are listed below, grouped by their behaviour category.

public static class BeetleAnims
{
    // The FBX hierarchy prefix shared by every clip
    private const string PRE = "Object_5|Beetle_ao|";

    // ══════════════════════════════════════════════════════════════════
    // BEETLE_  —  Normal / Healthy beetle (38 clips)
    // ══════════════════════════════════════════════════════════════════

    // ── Idle ──────────────────────────────────────────────────────────
    /// Standing still but showing micro-movement (not truly motionless)
    public const string Idle              = PRE + "Beetle_mh";

    /// Crouching forward, ready to strike — pre-attack tension pose
    public const string IdleAggro        = PRE + "Beetle_mh_Aggro";

    // ── Locomotion (walk-speed) ───────────────────────────────────────
    /// Walking forward at lower speed
    public const string RunFwd            = PRE + "Beetle_Run_Fwd";

    /// Walking left at lower speed
    public const string RunLeft           = PRE + "Beetle_Run_Left";

    /// Walking right at lower speed
    public const string RunRight          = PRE + "Beetle_Run_Right";

    /// Walk → run transition with a small jumping lunge forward
    public const string RunStart          = PRE + "Beetle_Run_Start";

    // ── Locomotion (ride/run speed) ───────────────────────────────────
    /// Full sprint forward (mounted / high speed)
    public const string RideRunFwd        = PRE + "Beetle_Ride_Run_Fwd";

    /// Full sprint left
    public const string RideRunLeft       = PRE + "Beetle_Ride_Run_Left";

    /// Full sprint right
    public const string RideRunRight      = PRE + "Beetle_Ride_Run_Right";

    // ── Turning ───────────────────────────────────────────────────────
    /// Slight left drift / damaged-strafe turn
    public const string TurnLeft          = PRE + "Beetle_TurnInPlace_Left";

    /// Sharp 180° spin to the left
    public const string TurnLeft180       = PRE + "Beetle_TurnInPlace_Left180";

    /// Slight right drift / damaged-strafe turn
    public const string TurnRight         = PRE + "Beetle_TurnInPlace_Right";

    /// Sharp 180° spin to the right
    public const string TurnRight180      = PRE + "Beetle_TurnInPlace_Right180";

    // ── Flight ────────────────────────────────────────────────────────
    /// Opens wings + flap only — earliest flight learning stage
    public const string MultiSlamAggro   = PRE + "Beetle_MultiSlam_Aggro";

    /// Opens wings + flap + small jump — second flight learning stage
    public const string MultiSlamAttack  = PRE + "Beetle_MultiSlam_Attack";

    /// Wings out + flap without leaving the ground — flight attempt stage
    public const string PounceAggro      = PRE + "Beetle_Pounce_Aggro";

    /// Full takeoff — beetle leaves the ground
    public const string FlightStart      = PRE + "Beetle_Pounce_Start";

    /// Steady mid-air flying idle loop
    public const string FlightIdle       = PRE + "Beetle_Pounce_Flight_Mh";

    /// Landing from flight gracefully
    public const string FlightLand       = PRE + "Beetle_Pounce_Land";

    /// Jump-launch into flight (ride style)
    public const string RideJumpStart    = PRE + "Beetle_Ride_Jump_Start";

    /// Alternate in-flight idle (ride / mount context)
    public const string RideJumpIdle     = PRE + "Beetle_Ride_Jump_Mh";

    /// Flight land with forward momentum, continues running
    public const string RideJumpLand     = PRE + "Beetle_Ride_Jump_Land";

    /// Mid-flight collision with obstacle — out-of-control fall
    public const string RideJumpCrash    = PRE + "Beetle_Ride_Jump_Crash";

    /// Struck in flight — beetle tumbles and falls out of control
    public const string RideJumpMiss     = PRE + "Beetle_Ride_Jump_Miss";

    // ── Combat ────────────────────────────────────────────────────────
    /// Lunge forward, horn strike upward — collision / rival attack
    public const string Attack            = PRE + "Beetle_Attack";

    /// Jump + ground slam with full body weight
    public const string StompAttack       = PRE + "Beetle_StompAttack";

    /// Attack animation while mounted at ride speed
    public const string RideAttack        = PRE + "Beetle_Ride_Attack";

    /// Full roar — intimidation gesture
    public const string Roar              = PRE + "Beetle_mh_Gesture_Roar";

    // ── Take Damage ───────────────────────────────────────────────────
    /// Heavy hit reaction — full body stagger
    public const string TakeDamage        = PRE + "Beetle_TakeDamage";

    /// Light hit from the front — head snaps sideways (type 1)
    public const string TakeDamageFront   = PRE + "Beetle_TakeDamage_Minor_Front";

    /// Light hit from the left  — head snaps sideways (type 2)
    public const string TakeDamageLeft    = PRE + "Beetle_TakeDamage_Minor_Left";

    /// Light hit from the right — head snaps sideways (type 3)
    public const string TakeDamageRight   = PRE + "Beetle_TakeDamage_Minor_Right";

    // ── Gestures ──────────────────────────────────────────────────────
    /// Post-sleep or recovery shake
    public const string Shake             = PRE + "Beetle_mh_Gesture_Shake";

    // ── Death / Crash ─────────────────────────────────────────────────
    /// Tried to fly, failed — crashes forward and down
    public const string CrashFwd          = PRE + "Beetle_Crash_Fwd";

    /// Falls and lies still — normal tired/death collapse
    public const string CrashMh           = PRE + "Beetle_Crash_mh";

    /// Running death — hit while sprinting, tumbles and falls
    public const string RideRunDeath      = PRE + "Beetle_Ride_Run_Death_Var1";

    /// Long sequence: wakes from crash → scrambles → jumps (9.63 s)
    public const string CrashRideRunStart = PRE + "Beetle_Crash_Ride_Run_Start";

    /// Wakes up from crash / sleep — body shake, stands up
    public const string CrashExit         = PRE + "Beetle_Crash_Exit";

    // ══════════════════════════════════════════════════════════════════
    // CODY_  —  Headless death sequence (12 clips)
    // Plays when beetle HP reaches the "headless" damage threshold.
    // Uses the same state names as Beetle_ but with broken/spasming motion.
    // ══════════════════════════════════════════════════════════════════

    /// Headless confirmed-death collapse
    public const string CodyCrashMh       = PRE + "Cody_Crash_mh";

    /// Headless: long death thrash → climbs tree → final death (9.63 s)
    public const string CodyCrashRunStart = PRE + "Cody_Crash_Ride_Run_Start";

    /// Headless: vibrating pain attack (still trying to fight)
    public const string CodyRideAttack    = PRE + "Cody_Ride_Attack";

    /// Headless: crash into wall / tree while flying
    public const string CodyJumpCrash     = PRE + "Cody_Ride_Jump_Crash";

    /// Headless: jump-land — death stage 1
    public const string CodyJumpLand      = PRE + "Cody_Ride_Jump_Land";

    /// Headless: still moving but dead — death stage 2
    public const string CodyJumpIdle      = PRE + "Cody_Ride_Jump_Mh";

    /// Headless: tries to recover, fails — death stage 3
    public const string CodyJumpMiss      = PRE + "Cody_Ride_Jump_Miss";

    /// Headless: recovery attempt phase 2
    public const string CodyJumpStart     = PRE + "Cody_Ride_Jump_Start";

    /// Headless: running death phase 3
    public const string CodyRunDeath      = PRE + "Cody_Ride_Run_Death_Var1";

    /// Headless: slowing run forward phase 4
    public const string CodyRunFwd        = PRE + "Cody_Ride_Run_Fwd";

    /// Headless: slowing run left phase 5
    public const string CodyRunLeft       = PRE + "Cody_Ride_Run_Left";

    /// Headless: slowing run right phase 6
    public const string CodyRunRight      = PRE + "Cody_Ride_Run_Right";

    // ══════════════════════════════════════════════════════════════════
    // MAY_  —  Upside-down / ass-up death sequence (16 clips)
    // Plays when beetle dies flipped onto its back.
    // ══════════════════════════════════════════════════════════════════

    // ── Static Death Poses (0-frame, instant pose snap) ──────────────
    /// Upside-down: belly up, facing straight — base death pose
    public const string MayPoseMid          = PRE + "May_Bhv_BeetleRide_AS_Mid";

    /// Upside-down: body tilted 45° down
    public const string MayPoseMidDown45    = PRE + "May_Bhv_BeetleRide_AS_Mid_Down45";

    /// Upside-down: body tilted 90° down
    public const string MayPoseMidDown90    = PRE + "May_Bhv_BeetleRide_AS_Mid_Down90";

    /// Upside-down: body tilted 45° up
    public const string MayPoseMidUp45      = PRE + "May_Bhv_BeetleRide_AS_Mid_Up45";

    /// Upside-down: body tilted 90° up
    public const string MayPoseMidUp90      = PRE + "May_Bhv_BeetleRide_AS_Mid_Up90";

    /// Upside-down: rolled 90° left
    public const string MayPoseLeft90       = PRE + "May_Bhv_BeetleRide_AS_Left90";

    /// Upside-down: rolled 90° left + 45° forward tilt
    public const string MayPoseLeft90Down45 = PRE + "May_Bhv_BeetleRide_AS_Left90Down45";

    /// Upside-down: rolled 90° left + 90° forward tilt
    public const string MayPoseLeft90Down90 = PRE + "May_Bhv_BeetleRide_AS_Left90Down90";

    /// Upside-down: rolled 90° left + 45° back tilt
    public const string MayPoseLeft90Up45   = PRE + "May_Bhv_BeetleRide_AS_Left90Up45";

    /// Upside-down: rolled 90° left + 90° back tilt
    public const string MayPoseLeft90Up90   = PRE + "May_Bhv_BeetleRide_AS_Left90Up90";

    /// Upside-down: rolled 90° right
    public const string MayPoseRight90      = PRE + "May_Bhv_BeetleRide_AS_Right90";

    /// Upside-down: rolled 90° right + 45° forward tilt
    public const string MayPoseRight90Down45= PRE + "May_Bhv_BeetleRide_AS_Right90Down45";

    /// Upside-down: rolled 90° right + 90° forward tilt
    public const string MayPoseRight90Down90= PRE + "May_Bhv_BeetleRide_AS_Right90Down90";

    /// Upside-down: rolled 90° right + 45° back tilt
    public const string MayPoseRight90Up45  = PRE + "May_Bhv_BeetleRide_AS_Right90Up45";

    /// Upside-down: rolled 90° right + 90° back tilt
    public const string MayPoseRight90Up90  = PRE + "May_Bhv_BeetleRide_AS_Right90Up90";

    // ── Animated May_ clips ───────────────────────────────────────────
    /// Pain spasm: ass-up with sideways body wiggle
    public const string MayShoot           = PRE + "May_Bhv_BeetleRide_Shoot";

    /// Upside-down 45° crash collapse
    public const string MayCrashMh         = PRE + "May_Crash_mh";

    /// Upside-down long death struggle sequence (9.63 s)
    public const string MayCrashRunStart   = PRE + "May_Crash_Ride_Run_Start";

    /// Upside-down: fight attempt while in pain / struggling
    public const string MayRideAttack      = PRE + "May_Ride_Attack";

    /// Upside-down: jump then crash, death scene
    public const string MayJumpCrash       = PRE + "May_Ride_Jump_Crash";

    /// Upside-down: jump-land death scene with struggle
    public const string MayJumpLand        = PRE + "May_Ride_Jump_Land";

    /// Upside-down: dead but still moving (no struggle)
    public const string MayJumpIdle        = PRE + "May_Ride_Jump_Mh";

    /// Upside-down: ass falls sideways from upright position
    public const string MayJumpMiss        = PRE + "May_Ride_Jump_Miss";

    /// Upside-down: ass struggles sideways
    public const string MayJumpStart       = PRE + "May_Ride_Jump_Start";

    /// Upside-down: ass struggles then falls to final death
    public const string MayRunDeath        = PRE + "May_Ride_Run_Death_Var1";

    /// Upside-down: slowing run forward (dying locomotion)
    public const string MayRunFwd          = PRE + "May_Ride_Run_Fwd";

    /// Upside-down: slowing run left (dying locomotion)
    public const string MayRunLeft         = PRE + "May_Ride_Run_Left";

    /// Upside-down: slowing run right (dying locomotion)
    public const string MayRunRight        = PRE + "May_Ride_Run_Right";
}