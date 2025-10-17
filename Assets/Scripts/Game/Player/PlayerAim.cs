using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PlayerAim : MonoBehaviour
{
    [Header("Setup")]
    public Camera mainCam;

    [Header("Laser")]
    public float length = 10f;
    public float width = 0.06f;
    public Color color = Color.yellow;
    public bool hideSysCursor = true;

    [Header("Gamepad")]
    [Range(0f, 1f)] public float stickDeadZone = 0.2f;

    [Header("Optional")]
    public bool rotateToAim = false;
    public float spriteAngleOffset = -90f;

    private LineRenderer lr;
    private Vector2 aimDirect = Vector2.right;
    private Vector2 lastMouseScreen;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();

        lr.startWidth = width;
        lr.endWidth = width;
        lr.startColor = color;
        lr.endColor = color;

        if (hideSysCursor)
        {
            Cursor.visible = false;
        }
    }

    void OnDestroy()
    {
        if (hideSysCursor)
        {
            Cursor.visible = true;
        }
    }

    public void OnAim(InputValue value)
    {
        Vector2 stick = value.Get<Vector2>();
        if (stick.sqrMagnitude > stickDeadZone * stickDeadZone)
        {
            aimDirect = stick.normalized;
        }
    }

    void Update()
    {
        if (Mouse.current != null)
        {
            Vector2 now = Mouse.current.position.ReadValue();
            if ((now - lastMouseScreen).sqrMagnitude > 0.001f)
            {
                Vector3 world = mainCam.ScreenToWorldPoint(new Vector3(now.x, now.y, -mainCam.transform.position.z));
                Vector2 v = (Vector2)(world - transform.position);
                if (v.sqrMagnitude > 0.0001f)
                {
                    aimDirect = v.normalized;
                }
                lastMouseScreen = now;
            }
        }

        Vector3 origin = transform.position;
        Vector3 end = origin + (Vector3)(aimDirect * length);

        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, end);

        if (rotateToAim)
        {
            float angle = Mathf.Atan2(aimDirect.y, aimDirect.x);
            transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);
        }
    }
}
