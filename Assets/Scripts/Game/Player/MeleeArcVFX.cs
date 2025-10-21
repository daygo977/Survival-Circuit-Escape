using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MeleeArcVFX : MonoBehaviour
{
    public float onTime = 0.08f;    //How long the arc stays visible
    public int segments = 16;       //More segments equals smoother arc

    LineRenderer lr;
    float t;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;            //Positions are in world space
    }

    /// <summary>
    /// 
    /// Creates a visuals arc centered on origin, angled by direction (of mouse or stick input), 
    /// with range radius, and an arc with degrees wide.
    /// 
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direct"></param>
    /// <param name="range"></param>
    /// <param name="arcDeg"></param>
    public void Show(Vector3 origin, Vector2 direct, float range, float arcDeg)
    {
        t = onTime;
        lr.positionCount = segments + 1;        //+1 so we can fill 0 to n segments

        //degrees to radians, builds half arc
        float half = arcDeg * 0.5f * Mathf.Deg2Rad;

        //Base angle (in radians) that points along the direction
        //Atan2 will return angle in radians for vectors (y, x)
        float start = Mathf.Atan2(direct.y, direct.x) - half;

        //Steps along the arc and places a vertex on the circle radius range
        for (int i = 0; i <= segments; i++)
        {
            float a = start + (half * 2f) * (i / (float)segments);  //Moves across the arc from start to start + 2*half
            //Coverts polar (angle + radius) to cartesian (x, y)
            Vector3 p = origin + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * range;
            lr.SetPosition(i, p);
        }

        gameObject.SetActive(true);
    }

    void Update()
    {
        //Life time countdown, then clean up
        t -= Time.deltaTime;
        if (t <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
