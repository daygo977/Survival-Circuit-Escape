using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public float onTime = 0.05f;
    float t;

    void OnEnable()
    {
        //reset timer each time it is shown
        t = onTime;
    }

    void Update()
    {
        //flash will disappear until next shoot happens
        t -= Time.deltaTime;
        if (t <= 0f)
        {
            gameObject.SetActive(false);
        }
    }
}
