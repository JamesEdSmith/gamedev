using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouthMovement : MonoBehaviour
{

    float interval = 2;
    bool goTime;
    float mouthTime;
    float timer;
    Quaternion target;
    Quaternion orig;
    public float biteAngleMax;
    public float angleMax= 30;
    public float angleMin = 15;
    public float timeMax = 1;
    public float timeMin = 0.25f;
    float openAngle;
    bool controlled;

    // Start is called before the first frame update
    void Start()
    {
        orig = transform.localRotation;
    }

    public void control()
    {
        controlled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!controlled)
        {
            timer += Time.deltaTime;
            if (!goTime)
            {
                if (timer > interval)
                {
                    goTime = true;
                    timer = 0;
                    interval = Random.Range(1, 5);
                    mouthTime = Random.Range(timeMin, timeMax);
                    openAngle = Random.Range(angleMin, angleMax);
                    target = transform.localRotation * Quaternion.Euler(openAngle, 0, 0);
                }
            }
            else
            {
                if (timer > mouthTime)
                {
                    timer = 0;
                    goTime = false;
                }
                else
                {
                    if (timer < mouthTime * 0.5f)
                    {
                        transform.localRotation = Quaternion.Slerp(orig, target, timer / mouthTime);
                    }
                    else
                    {
                        transform.localRotation = Quaternion.Slerp(orig, target, ((mouthTime * 0.5f) - (timer - mouthTime * 0.5f)) / mouthTime);
                    }
                }
            }
        }
    }

    internal void setAngle(float v)
    {
        transform.localRotation = orig * Quaternion.Euler(v, 0, 0);
    }

    internal void controlOff()
    {
        controlled = false;
    }
}
