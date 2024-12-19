using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishMover : MonoBehaviour
{
    const int STATE_IDLE = 1;
    const int STATE_ANTIC_STRAIGHT = 2;
    const int STATE_ANTIC_STRAIGHT_STOP = 3;

    float timer;
    float idleTime = 5;

    int state;
    float acceleration;
    float currSpeed;
    float speed;
    float moveTime;
    private float rotRate;
    private Vector3 rotDir;
    private Quaternion currRotation;
    private Quaternion newRotation;
    HeadMovement headMovement;

    // Start is called before the first frame update
    void Start()
    {
        headMovement = GetComponentInChildren<HeadMovement>();
        state = STATE_IDLE;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == STATE_IDLE)
        {
            doIdle();
        }
        else if (state == STATE_ANTIC_STRAIGHT)
        {
            doAnticStraight();
        }
        else if (state == STATE_ANTIC_STRAIGHT_STOP)
        {
            doAnticStraightStop();
        }
    }

    private void doAnticStraight()
    {
        if (timer < moveTime)
        {
            timer += Time.deltaTime;
            if (currSpeed < speed)
            {
                currSpeed += acceleration;
                currSpeed = Mathf.Clamp(currSpeed, 0, speed);
            }
            transform.Translate(0, 0, speed * Time.deltaTime);
            headMovement.setSpeed(currSpeed);

            transform.Rotate(rotDir, rotRate * Time.deltaTime);
        }
        else
        {
            timer = 0;
            state = STATE_ANTIC_STRAIGHT_STOP;
            acceleration = Random.Range(1f, 5f);
            newRotation = currRotation = transform.rotation;
            newRotation.Set(newRotation.x, newRotation.y, 0, newRotation.w);
            newRotation.Set(0, newRotation.y, newRotation.z, newRotation.w);
        }
    }

    private void doAnticStraightStop()
    {
        if (currSpeed > 0)
        {
            currSpeed -= acceleration * Time.deltaTime;
            currSpeed = Mathf.Clamp(currSpeed, 0, speed);
            transform.Translate(0, 0, currSpeed * Time.deltaTime);
            headMovement.setSpeed(currSpeed);

            float t = (speed - currSpeed)/speed;
            transform.rotation = Quaternion.Slerp(currRotation, newRotation, t);
        }
        else
        {
            timer = 0;
            state = STATE_IDLE;
            idleTime = Random.Range(1f, 10f);
        }
    }

    private void doIdle()
    {
        if (timer < idleTime)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
            state = STATE_ANTIC_STRAIGHT;
            acceleration = Random.Range(1f, 5f);
            speed = Random.Range(1f, 5f);
            moveTime = Random.Range(1f, 10f);
            rotRate = Random.Range(1f, 10f);
            rotDir = new Vector3(Random.Range(0, 1f), Random.Range(0, 5f), Random.Range(0, 0.1f));
            currSpeed = 0;

        }
    }
}
