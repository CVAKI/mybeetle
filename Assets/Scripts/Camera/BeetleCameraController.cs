// BeetleCameraController.cs
// Place: Assets/Scripts/Camera/BeetleCameraController.cs
// Cinematic follow camera that dynamically shifts angles based on beetle action

using UnityEngine;

public class BeetleCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public float followSpeed   = 5f;
    public float rotationSpeed = 3f;
    public float lookAheadDist = 2f;

    [Header("Cinematic Profiles")]
    public CameraProfile walkProfile;
    public CameraProfile runProfile;
    public CameraProfile flyProfile;
    public CameraProfile fightProfile;
    public CameraProfile deathProfile;

    [System.Serializable]
    public class CameraProfile
    {
        public Vector3 offset      = new Vector3(0f, 2f, -4f);
        public float   fieldOfView = 60f;
        public float   tiltAngle   = 10f;
    }

    private Camera _cam;
    private CameraProfile _currentProfile;
    private BeetleController _trackedBeetle;
    private Vector3 _smoothVelocity;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;
        _currentProfile = walkProfile;
    }

    public void AttachTo(Transform newTarget)
    {
        target = newTarget;
        _trackedBeetle = newTarget?.GetComponent<BeetleController>();

        // Instantly snap on first attach
        if (target != null)
            transform.position = target.position + walkProfile.offset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Pick profile based on RL action
        CameraProfile desired = GetDesiredProfile();

        // Smooth FOV
        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, desired.fieldOfView, Time.deltaTime * 2f);

        // Desired camera position
        Vector3 worldOffset   = target.TransformDirection(desired.offset);
        Vector3 desiredPos    = target.position + worldOffset;

        // Smooth follow
        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos, ref _smoothVelocity, 1f / followSpeed);

        // Cinematic look-ahead
        Vector3 lookTarget = target.position + target.forward * lookAheadDist;
        Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position);
        desiredRot *= Quaternion.Euler(desired.tiltAngle, 0f, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot,
                                               Time.deltaTime * rotationSpeed);
    }

    private CameraProfile GetDesiredProfile()
    {
        if (_trackedBeetle == null) return walkProfile;

        var action = _trackedBeetle.rl?.CurrentAction;

        return action switch
        {
            BeetleRLAgent.BeetleAction.Fly    => flyProfile,
            BeetleRLAgent.BeetleAction.Fight  => fightProfile,
            BeetleRLAgent.BeetleAction.Run    => runProfile,
            _ => _trackedBeetle.stats?.IsDead == true ? deathProfile : walkProfile
        };
    }
}
