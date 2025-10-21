using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MeleeArcVFX : MonoBehaviour
{
    public float onTime = 0.08f;
    public int segments = 16;       //More segments equals smoother arc

    LineRenderer lr;
    float t;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
    }

    public void Show(Vector3 origin, Vector2 direct, float range, float arcDeg)
    {
        t = onTime;
        lr.positionCount = segments + 1;

        float half = arcDeg * 0.5f * Mathf.Deg2Rad;
        float start = Mathf.Atan2(direct.y, direct.x) - half;

        for (int i = 0; i <= segments; i++)
        {
            float a = start + (half * 2f) * (i / (float)segments);
            Vector3 p = origin + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * range;
            lr.SetPosition(i, p);
        }

        gameObject.SetActive(true);
    }

    void Update()
    {
        t -= Time.deltaTime;
        if (t <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
