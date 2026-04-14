// BeetleAnimationController.cs
// Place: Assets/Scripts/Beetle/BeetleAnimationController.cs
//
// Drives the Animator using all 78 animation clip names from BeetleData.cs.
// Covers: Idle, Locomotion, Flight (RL stages), Combat, Damage, Death sequences.
//
// SETUP:
//   • Attach to the root beetle GameObject that has an Animator component.
//   • The Animator controller must have a state for every clip name in BeetleAnims.
//   • Use an AnimatorOverrideController to map short state names if needed.

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BeetleAnimationController : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────
    [Header("Transition Durations")]
    [Tooltip("Blend time for looping animations (idle, run, flight idle)")]
    [SerializeField] private float loopBlend  = 0.15f;

    [Tooltip("Blend time for one-shot animations (attack, damage, death)")]
    [SerializeField] private float onceBlend  = 0.05f;

    // ── Private ───────────────────────────────────────────────────────
    private Animator _anim;

    // ── Animator Parameter Hashes ─────────────────────────────────────
    private static readonly int P_Speed    = Animator.StringToHash("Speed");
    private static readonly int P_IsFlying = Animator.StringToHash("IsFlying");
    private static readonly int P_IsDead   = Animator.StringToHash("IsDead");

    // ── Unity Lifecycle ───────────────────────────────────────────────
    private void Awake() => _anim = GetComponent<Animator>();

    // ═════════════════════════════════════════════════════════════════
    // SECTION 1 — PARAMETER SETTERS
    // Call these every frame from BeetleController to keep the
    // Animator parameters in sync (useful for blend trees).
    // ═════════════════════════════════════════════════════════════════

    /// Set movement speed parameter (used in blend trees).
    public void SetSpeed(float speed)   => _anim.SetFloat(P_Speed, speed);

    /// Notify animator whether the beetle is currently airborne.
    public void SetFlying(bool flying)  => _anim.SetBool(P_IsFlying, flying);

    /// Notify animator the beetle is dead (locks certain state exits).
    public void SetDead(bool dead)      => _anim.SetBool(P_IsDead, dead);

    // ═════════════════════════════════════════════════════════════════
    // SECTION 2 — IDLE
    // ═════════════════════════════════════════════════════════════════

    /// Default idle — standing still with micro-movement.
    public void PlayIdle()     => CrossFade(BeetleAnims.Idle);

    /// Aggro idle — beetle is tense, crouched, ready to strike.
    public void PlayIdleAggro() => PlayOnce(BeetleAnims.IdleAggro);

    /// Post-sleep / recovery shake.
    public void PlayShake()    => PlayOnce(BeetleAnims.Shake);

    /// Full intimidation roar.
    public void PlayRoar()     => PlayOnce(BeetleAnims.Roar);

    // ═════════════════════════════════════════════════════════════════
    // SECTION 3 — LOCOMOTION (Walk Speed)
    // Use when beetle is moving on foot at moderate speed.
    // ═════════════════════════════════════════════════════════════════

    /// Walk forward at lower speed.
    public void PlayWalkFwd()   => CrossFade(BeetleAnims.RunFwd);

    /// Walk left at lower speed.
    public void PlayWalkLeft()  => CrossFade(BeetleAnims.RunLeft);

    /// Walk right at lower speed.
    public void PlayWalkRight() => CrossFade(BeetleAnims.RunRight);

    /// Walk-to-run transition with a forward lunge jump.
    public void PlayRunStart()  => PlayOnce(BeetleAnims.RunStart);

    /// Automatically selects walk direction based on a -1…+1 direction value.
    /// direction: -1 = left | 0 = forward | +1 = right
    public void PlayWalk(float direction = 0f)
    {
        string clip = direction < -0.3f ? BeetleAnims.RunLeft
                    : direction >  0.3f ? BeetleAnims.RunRight
                    : BeetleAnims.RunFwd;
        CrossFade(clip);
    }

    // ═════════════════════════════════════════════════════════════════
    // SECTION 4 — LOCOMOTION (Ride / Sprint Speed)
    // Use when beetle is at full sprint speed (mounted or charging).
    // ═════════════════════════════════════════════════════════════════

    /// Full sprint forward.
    public void PlayRunFwd()   => CrossFade(BeetleAnims.RideRunFwd);

    /// Full sprint left.
    public void PlayRunLeft()  => CrossFade(BeetleAnims.RideRunLeft);

    /// Full sprint right.
    public void PlayRunRight() => CrossFade(BeetleAnims.RideRunRight);

    /// Automatically selects sprint direction based on a -1…+1 direction value.
    public void PlayRun(float direction = 0f)
    {
        string clip = direction < -0.3f ? BeetleAnims.RideRunLeft
                    : direction >  0.3f ? BeetleAnims.RideRunRight
                    : BeetleAnims.RideRunFwd;
        CrossFade(clip);
    }

    // ═════════════════════════════════════════════════════════════════
    // SECTION 5 — TURNING
    // ═════════════════════════════════════════════════════════════════

    /// Slight left drift turn (also used for damaged strafe).
    public void PlayTurnLeft()     => PlayOnce(BeetleAnims.TurnLeft);

    /// Slight right drift turn (also used for damaged strafe).
    public void PlayTurnRight()    => PlayOnce(BeetleAnims.TurnRight);

    /// Sharp 180° spin to the left.
    public void PlayTurnLeft180()  => PlayOnce(BeetleAnims.TurnLeft180);

    /// Sharp 180° spin to the right.
    public void PlayTurnRight180() => PlayOnce(BeetleAnims.TurnRight180);

    // ═════════════════════════════════════════════════════════════════
    // SECTION 6 — FLIGHT (RL Learning Stages)
    // Call these in order as the beetle learns to fly through RL rewards.
    // Stage 1 → 2 → 3 → Full Flight
    // ═════════════════════════════════════════════════════════════════

    /// Stage 1 — Opens wings and flaps, does NOT leave the ground.
    public void PlayFlightLearnStage1_WingFlap()   => PlayOnce(BeetleAnims.MultiSlamAggro);

    /// Stage 2 — Opens wings, flaps, and makes a small jump.
    public void PlayFlightLearnStage2_FlapJump()   => PlayOnce(BeetleAnims.MultiSlamAttack);

    /// Stage 3 — Wings fully out, flapping in place — first real flight attempt.
    public void PlayFlightLearnStage3_Attempt()    => PlayOnce(BeetleAnims.PounceAggro);

    /// Full takeoff — beetle leaves the ground (call SetFlying(true) after).
    public void PlayFlightStart()  => PlayOnce(BeetleAnims.FlightStart);

    /// Steady in-air flying idle loop.
    public void PlayFlightIdle()   => CrossFade(BeetleAnims.FlightIdle);

    /// Graceful landing from flight (call SetFlying(false) after).
    public void PlayFlightLand()   => PlayOnce(BeetleAnims.FlightLand);

    // ── Ride/Jump style flight (higher speed, mount context) ──────────

    /// Jump-launch into flight.
    public void PlayRideJumpStart()  => PlayOnce(BeetleAnims.RideJumpStart);

    /// In-flight idle at ride/mount speed.
    public void PlayRideJumpIdle()   => CrossFade(BeetleAnims.RideJumpIdle);

    /// Land from jump-flight and continue running.
    public void PlayRideJumpLand()   => PlayOnce(BeetleAnims.RideJumpLand);

    /// Mid-flight collision with obstacle — out-of-control fall.
    public void PlayFlightCrash()    => PlayOnce(BeetleAnims.RideJumpCrash);

    /// Struck in flight — beetle tumbles down out of control.
    public void PlayFlightHitMiss()  => PlayOnce(BeetleAnims.RideJumpMiss);

    // ═════════════════════════════════════════════════════════════════
    // SECTION 7 — COMBAT
    // ═════════════════════════════════════════════════════════════════

    /// Horn lunge upward — main melee attack (collision / rival encounter).
    public void PlayAttack()       => PlayOnce(BeetleAnims.Attack);

    /// Jump + slam full body weight into the ground.
    public void PlayStompAttack()  => PlayOnce(BeetleAnims.StompAttack);

    /// Attack while at ride/sprint speed.
    public void PlayRideAttack()   => PlayOnce(BeetleAnims.RideAttack);

    // ═════════════════════════════════════════════════════════════════
    // SECTION 8 — TAKE DAMAGE
    // ═════════════════════════════════════════════════════════════════

    /// Heavy full-body stagger — large damage or knockback.
    public void PlayTakeDamageHeavy() => PlayOnce(BeetleAnims.TakeDamage);

    /// Light hit from the front — head snaps sideways.
    public void PlayTakeDamageFront() => PlayOnce(BeetleAnims.TakeDamageFront);

    /// Light hit from the left — head snaps sideways.
    public void PlayTakeDamageLeft()  => PlayOnce(BeetleAnims.TakeDamageLeft);

    /// Light hit from the right — head snaps sideways.
    public void PlayTakeDamageRight() => PlayOnce(BeetleAnims.TakeDamageRight);

    /// Smart damage picker: 0=heavy, 1=front, 2=left, 3=right.
    public void PlayTakeDamage(int variant = 0)
    {
        switch (Mathf.Clamp(variant, 0, 3))
        {
            case 1:  PlayTakeDamageFront(); break;
            case 2:  PlayTakeDamageLeft();  break;
            case 3:  PlayTakeDamageRight(); break;
            default: PlayTakeDamageHeavy(); break;
        }
    }

    // ═════════════════════════════════════════════════════════════════
    // SECTION 9 — CRASH / DEATH (Normal Beetle)
    // ═════════════════════════════════════════════════════════════════

    /// Tried to fly and failed — crashes forward and falls.
    public void PlayCrashForward()    => PlayOnce(BeetleAnims.CrashFwd);

    /// Normal death collapse — falls and lies still.
    public void PlayCrashDown()       => PlayOnce(BeetleAnims.CrashMh);

    /// Hit while sprinting — tumbles and falls.
    public void PlayRunningDeath()    => PlayOnce(BeetleAnims.RideRunDeath);

    /// Long 9-second sequence: wakes from crash → scrambles → final collapse.
    public void PlayCrashRecoverSequence() => PlayOnce(BeetleAnims.CrashRideRunStart);

    /// Wakes from crash / sleep — shakes body, stands up.
    public void PlayCrashExit()       => PlayOnce(BeetleAnims.CrashExit);

    // ═════════════════════════════════════════════════════════════════
    // SECTION 10 — HEADLESS DEATH SEQUENCE (Cody_)
    // Play these in order when HP reaches the "headless" threshold.
    // The beetle loses its head and continues spasming through death stages.
    // ═════════════════════════════════════════════════════════════════

    /// Headless confirmed-death collapse.
    public void PlayCodyDeathCollapse()   => PlayOnce(BeetleAnims.CodyCrashMh);

    /// Headless sprint right (stage 6 — slowing down).
    public void PlayCodyRunRight()        => CrossFade(BeetleAnims.CodyRunRight);

    /// Headless sprint left (stage 5 — slowing down).
    public void PlayCodyRunLeft()         => CrossFade(BeetleAnims.CodyRunLeft);

    /// Headless sprint forward (stage 4 — slowing down).
    public void PlayCodyRunFwd()          => CrossFade(BeetleAnims.CodyRunFwd);

    /// Headless running death (stage 3 — still trying to run).
    public void PlayCodyRunDeath()        => PlayOnce(BeetleAnims.CodyRunDeath);

    /// Headless recovery attempt phase 2.
    public void PlayCodyJumpStart()       => PlayOnce(BeetleAnims.CodyJumpStart);

    /// Headless: tries to recover, fails (stage 3).
    public void PlayCodyJumpMiss()        => PlayOnce(BeetleAnims.CodyJumpMiss);

    /// Headless: still moving but fully dead (stage 2).
    public void PlayCodyJumpIdle()        => CrossFade(BeetleAnims.CodyJumpIdle);

    /// Headless: jump-land death (stage 1).
    public void PlayCodyJumpLand()        => PlayOnce(BeetleAnims.CodyJumpLand);

    /// Headless: crash into wall/tree while flying.
    public void PlayCodyJumpCrash()       => PlayOnce(BeetleAnims.CodyJumpCrash);

    /// Headless: vibrating pain attack while still trying to fight.
    public void PlayCodyRideAttack()      => PlayOnce(BeetleAnims.CodyRideAttack);

    /// Headless long 9-second sequence: climbs tree → final death.
    public void PlayCodyLongDeathSequence() => PlayOnce(BeetleAnims.CodyCrashRunStart);

    /// Convenience: starts the full headless death sequence from the beginning.
    public void PlayCodyDeathSequence()
    {
        // Trigger the long sequence; individual stages play on coroutine/state machine
        PlayCodyLongDeathSequence();
    }

    // ═════════════════════════════════════════════════════════════════
    // SECTION 11 — UPSIDE-DOWN DEATH SEQUENCE (May_)
    // Play these when the beetle dies flipped onto its back (belly up).
    // ═════════════════════════════════════════════════════════════════

    // ── Animated upside-down clips ────────────────────────────────────

    /// Upside-down 45° crash collapse.
    public void PlayMayDeathCollapse()    => PlayOnce(BeetleAnims.MayCrashMh);

    /// Upside-down long 9-second struggle sequence.
    public void PlayMayLongDeathSequence() => PlayOnce(BeetleAnims.MayCrashRunStart);

    /// Upside-down pain spasm: ass-up with sideways body wiggle.
    public void PlayMayBodySpasm()        => PlayOnce(BeetleAnims.MayShoot);

    /// Upside-down fight attempt (still trying to attack in pain).
    public void PlayMayRideAttack()       => PlayOnce(BeetleAnims.MayRideAttack);

    /// Upside-down: jump then crash, death scene.
    public void PlayMayJumpCrash()        => PlayOnce(BeetleAnims.MayJumpCrash);

    /// Upside-down: jump-land death with struggle.
    public void PlayMayJumpLand()         => PlayOnce(BeetleAnims.MayJumpLand);

    /// Upside-down: dead but still moving, no struggle.
    public void PlayMayJumpIdle()         => CrossFade(BeetleAnims.MayJumpIdle);

    /// Upside-down: ass falls sideways.
    public void PlayMayJumpMiss()         => PlayOnce(BeetleAnims.MayJumpMiss);

    /// Upside-down: ass struggles sideways.
    public void PlayMayJumpStart()        => PlayOnce(BeetleAnims.MayJumpStart);

    /// Upside-down: final fall to death.
    public void PlayMayRunDeath()         => PlayOnce(BeetleAnims.MayRunDeath);

    /// Upside-down: dying locomotion forward.
    public void PlayMayRunFwd()           => CrossFade(BeetleAnims.MayRunFwd);

    /// Upside-down: dying locomotion left.
    public void PlayMayRunLeft()          => CrossFade(BeetleAnims.MayRunLeft);

    /// Upside-down: dying locomotion right.
    public void PlayMayRunRight()         => CrossFade(BeetleAnims.MayRunRight);

    // ── Static upside-down death POSES (instant snap, 0 frames) ──────
    // Call one of these to freeze the beetle in a final death resting pose.

    public void PlayMayPoseMid()           => SnapPose(BeetleAnims.MayPoseMid);
    public void PlayMayPoseMidDown45()     => SnapPose(BeetleAnims.MayPoseMidDown45);
    public void PlayMayPoseMidDown90()     => SnapPose(BeetleAnims.MayPoseMidDown90);
    public void PlayMayPoseMidUp45()       => SnapPose(BeetleAnims.MayPoseMidUp45);
    public void PlayMayPoseMidUp90()       => SnapPose(BeetleAnims.MayPoseMidUp90);
    public void PlayMayPoseLeft90()        => SnapPose(BeetleAnims.MayPoseLeft90);
    public void PlayMayPoseLeft90Down45()  => SnapPose(BeetleAnims.MayPoseLeft90Down45);
    public void PlayMayPoseLeft90Down90()  => SnapPose(BeetleAnims.MayPoseLeft90Down90);
    public void PlayMayPoseLeft90Up45()    => SnapPose(BeetleAnims.MayPoseLeft90Up45);
    public void PlayMayPoseLeft90Up90()    => SnapPose(BeetleAnims.MayPoseLeft90Up90);
    public void PlayMayPoseRight90()       => SnapPose(BeetleAnims.MayPoseRight90);
    public void PlayMayPoseRight90Down45() => SnapPose(BeetleAnims.MayPoseRight90Down45);
    public void PlayMayPoseRight90Down90() => SnapPose(BeetleAnims.MayPoseRight90Down90);
    public void PlayMayPoseRight90Up45()   => SnapPose(BeetleAnims.MayPoseRight90Up45);
    public void PlayMayPoseRight90Up90()   => SnapPose(BeetleAnims.MayPoseRight90Up90);

    /// Picks a random upside-down death pose — useful after the death sequence finishes.
    public void PlayRandomMayDeathPose()
    {
        string[] poses =
        {
            BeetleAnims.MayPoseMid,
            BeetleAnims.MayPoseMidDown45,
            BeetleAnims.MayPoseMidDown90,
            BeetleAnims.MayPoseMidUp45,
            BeetleAnims.MayPoseMidUp90,
            BeetleAnims.MayPoseLeft90,
            BeetleAnims.MayPoseLeft90Down45,
            BeetleAnims.MayPoseLeft90Down90,
            BeetleAnims.MayPoseLeft90Up45,
            BeetleAnims.MayPoseLeft90Up90,
            BeetleAnims.MayPoseRight90,
            BeetleAnims.MayPoseRight90Down45,
            BeetleAnims.MayPoseRight90Down90,
            BeetleAnims.MayPoseRight90Up45,
            BeetleAnims.MayPoseRight90Up90
        };
        SnapPose(poses[Random.Range(0, poses.Length)]);
    }

    // ═════════════════════════════════════════════════════════════════
    // SECTION 12 — HIGH-LEVEL DEATH DISPATCHER
    // Call this from BeetleLifeCycle or BeetleController.
    // It picks the correct death category automatically.
    // ═════════════════════════════════════════════════════════════════

    /// Play the opening death animation for the given category.
    /// Follow up with stage-specific calls from your state machine / coroutine.
    public void PlayDeathSequence(DeathCategory cat = DeathCategory.Normal)
    {
        SetDead(true);

        switch (cat)
        {
            case DeathCategory.Normal:
                PlayCrashDown();           // Beetle_Crash_mh
                break;

            case DeathCategory.Headless:
                PlayCodyDeathCollapse();   // Cody_Crash_mh → follow with Cody sequence
                break;

            case DeathCategory.UpsideDown:
                PlayMayDeathCollapse();    // May_Crash_mh → follow with May sequence
                break;
        }
    }

    // ═════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═════════════════════════════════════════════════════════════════

    /// Smooth blend into a LOOPING animation (idle, run, flight idle).
    private void CrossFade(string clip, float duration = -1f)
    {
        float t = duration < 0 ? loopBlend : duration;
        _anim.CrossFadeInFixedTime(clip, t);
    }

    /// Blend into a ONE-SHOT animation (attack, damage, turn, death).
    private void PlayOnce(string clip, float duration = -1f)
    {
        float t = duration < 0 ? onceBlend : duration;
        _anim.CrossFadeInFixedTime(clip, t);
    }

    /// Instant snap to a STATIC POSE with no blend (0-frame clips like May_ poses).
    private void SnapPose(string clip)
    {
        _anim.CrossFadeInFixedTime(clip, 0f);
    }
}