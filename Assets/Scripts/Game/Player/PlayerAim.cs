using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Fized-length aiming laser for top-down shooter (2D).
///  - No crosshair, however OS cursor will behidden.
///  - Directions of laser aim will come from mouse (continuous aim) or right stick on controller (when pushed passed deadzone).
///  - Optional Setting: rotate player sprite to face direction of aim using Rigidbody2D.MoveRotation.
/// 
/// Requirements:
///  - Put this on Player game object (for component).
///  - Player has to have LineRenderer and Rigidbody2D (Grvity = 0, Rotation z not frozen).
///  - PlayerInput uses Input System with behavior set as "Send Message."
///  - Input Actions: Player/Aim (Vector2) bound to Gamepad Right Stick (no bound for mouse since script handles mouse input).
/// </summary>
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAim : MonoBehaviour
{
    // References set in Inspector (Main Camera will be auto-filled if left empty)
    [Header("Setup")]
    public Camera mainCam;

    // Laser appearence/behavior (fixed length)
    [Header("Laser")]
    public float length = 10f;          //World-space length of the laser
    public float width = 0.06f;         //Line thickness
    public Color color = Color.yellow;  //Laser color
    public bool hideSysCursor = true;   //Hide OS cursor while playing

    // Gamepad (Controller) input settings
    [Header("Gamepad")]
    [Range(0f, 1f)] public float stickDeadZone = 0.2f;  //Ignore tiny stick noise (stick drift)

    // Optional rotation for the player towards aim direction
    [Header("Optional")]
    public bool rotateToAim = false;        //Rotation togle, off/on
    public float rotationSpeed = 720f;      //Degrees/sec smoothing for rotation
    public float spriteAngleOffset = -90f;  //Align sprite's foward with computed angle

    // Private fields
    private LineRenderer lr;                    //Laser line
    private Rigidbody2D rb;                     //Player physics body
    private Vector2 aimDirect = Vector2.right;  //Current normalized aim direction
    private Vector2 lastMouseScreen;            //Last screen-space mouse position (to detect movement)
    private float targetAngleDeg;               //Target facing angle in degrees

    void Awake()
    {
        // Cache components
        lr = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Auto-assign main camera if not set
        if (!mainCam)
        {
            mainCam = Camera.main;
        }

        // Configure the line to render in world space and to match the desired look
        lr.useWorldSpace = true;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.startColor = color;
        lr.endColor = color;

        // If hidden cursor option true, then hide it
        if (hideSysCursor)
        {
            Cursor.visible = false;
        }
    }

    void OnDestroy()
    {
        // Restores cursor visibilty when pbject is destroyed (e.g. when stoping play mode)
        if (hideSysCursor)
        {
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Called automatically by PlayerInput (Send Messages) when "Aim" actions updates.
    /// Updates aim direction from the right stick only when pushed past deadzone.
    /// </summary>
    public void OnAim(InputValue value)
    {
        Vector2 stick = value.Get<Vector2>();
        if (stick.sqrMagnitude > stickDeadZone * stickDeadZone)
        {
            // Normalize to get direction only
            aimDirect = stick.normalized;
        }
    }

    void Update()
    {
        // Mouse aims continuously, if mouse movement is not null, update aim towards world position
        if (Mouse.current != null)
        {
            Vector2 now = Mouse.current.position.ReadValue();

            // Only recompute when mouse is actually moved (will help in reducing jitters)
            if ((now - lastMouseScreen).sqrMagnitude > 0.001f)
            {
                // Convert screen to world using camera, use camera Z so orthographic works correctly (keeps world position correctly above player)
                Vector3 world = mainCam.ScreenToWorldPoint(new Vector3(now.x, now.y, -mainCam.transform.position.z));

                // Vector from player to mouse
                Vector2 v = (Vector2)(world - transform.position);

                // Normalize only if direction isn't zero
                if (v.sqrMagnitude > 0.0001f)
                {
                    aimDirect = v.normalized;
                }
                lastMouseScreen = now;
            }
        }

        // Update laser endpoints (fized length)
        Vector3 origin = transform.position;                    // Player
        Vector3 end = origin + (Vector3)(aimDirect * length);   // Fixed length and angle set to angle where mouse is

        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, end);

        // If rotation is on, compute player facing angle (in degrees)
        if (rotateToAim)
        {   
            // Mathf/Atan2 returns radians, so convert to degrees, then add to sprite offset
            targetAngleDeg = Mathf.Atan2(aimDirect.y, aimDirect.x) * Mathf.Rad2Deg + spriteAngleOffset;
        }
    }

    void FixedUpdate()
    {

        if (!rotateToAim)
        {
            return;
        }

        // Current rigidbody rotation (degrees)
        float current = rb.rotation;
        // Smoothly move towards player angle at rotationSpeed (deg/sec)
        float next = Mathf.MoveTowardsAngle(current, targetAngleDeg, rotationSpeed * Time.fixedDeltaTime);

        // Rotate via physics API (helps avoid any conflicts with Rigidbody2D)
        rb.MoveRotation(next);
    }
}
