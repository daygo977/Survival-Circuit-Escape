using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public float onTime = 0.05f;
    float t;

    void OnEnable()
    {
        t = onTime;
    }

    void Update()
    {
        t -= Time.deltaTime;
        if (t <= 0f)
        {
            gameObject.SetActive(false);
        }
    }
}
