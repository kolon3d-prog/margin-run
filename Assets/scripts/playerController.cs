using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController _cc;
    public float MaxSpeed = 10f;
    public float MaxAirSpeed = 1.2f;
    public float GroundAccelerate = 10f;
    public float AirAccelerate = 50f;
    public float Friction = 4f;
    public float StopSpeed = 3f;
    public float MouseSensitivity = 0.06f;
    public float JumpForce = 10f;
    public float Gravity = -30f;

    private float _rotationY;
    private Vector3 _velocity;
    private float _speed;
    private GUIStyle _speedStyle;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _cc.minMoveDistance = 0f;
    }

    public void Move(Vector2 input, bool jumpHeld)
    {
        float dt = Time.deltaTime;
        Vector3 wish = transform.TransformDirection(new Vector3(input.x, 0f, input.y));
        wish.y = 0f;
        float wishMag = Mathf.Min(wish.magnitude, 1f);
        Vector3 wishDir = wishMag > 0.001f ? wish.normalized : Vector3.zero;
        float wishSpeed = MaxSpeed * wishMag;

        bool grounded = _cc.isGrounded && _velocity.y <= 0f;

        if (grounded)
        {
            if (!jumpHeld)
                ApplyFriction(dt);

            Accelerate(wishDir, wishSpeed, GroundAccelerate, dt);
            _velocity.y = 0f;

            if (jumpHeld)
                _velocity.y = JumpForce;
        }
        else
        {
            ApplyAirAccelerate(wishDir, wishSpeed, dt);
            _velocity.y += Gravity * dt;
        }

        _cc.Move(_velocity * dt);
        _speed = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
    }

    void ApplyFriction(float dt)
    {
        Vector3 vel = new Vector3(_velocity.x, 0f, _velocity.z);
        float speed = vel.magnitude;
        if (speed < 0.01f)
        {
            _velocity.x = 0f;
            _velocity.z = 0f;
            return;
        }

        float control = speed < StopSpeed ? StopSpeed : speed;
        float newSpeed = Mathf.Max(speed - control * Friction * dt, 0f) / speed;
        _velocity.x *= newSpeed;
        _velocity.z *= newSpeed;
    }

    void Accelerate(Vector3 wishDir, float wishSpeed, float accel, float dt)
    {
        if (wishDir.sqrMagnitude < 0.0001f)
            return;

        float add = wishSpeed - Vector3.Dot(_velocity, wishDir);
        if (add <= 0f)
            return;

        float acc = Mathf.Min(accel * wishSpeed * dt, add);
        _velocity.x += acc * wishDir.x;
        _velocity.z += acc * wishDir.z;
    }

    void ApplyAirAccelerate(Vector3 wishDir, float wishSpeed, float dt)
    {
        if (wishDir.sqrMagnitude < 0.0001f)
            return;

        float wishspd = Mathf.Min(wishSpeed, MaxAirSpeed);
        float add = wishspd - Vector3.Dot(_velocity, wishDir);
        if (add <= 0f)
            return;

        float acc = Mathf.Min(AirAccelerate * wishSpeed * dt, add);
        _velocity.x += acc * wishDir.x;
        _velocity.z += acc * wishDir.z;
    }

    void OnGUI()
    {
        if (_speedStyle == null)
        {
            _speedStyle = new GUIStyle(GUI.skin.label);
            _speedStyle.alignment = TextAnchor.LowerCenter;
            _speedStyle.fontSize = 28;
            _speedStyle.fontStyle = FontStyle.Bold;
            _speedStyle.normal.textColor = Color.white;
        }

        GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height - 48f), _speed.ToString("0.0"), _speedStyle);
    }

    public void Rotate(Vector2 rotationVector)
    {
        _rotationY += rotationVector.x * MouseSensitivity;
        transform.localRotation = Quaternion.Euler(0f, _rotationY, 0f);
    }
}
