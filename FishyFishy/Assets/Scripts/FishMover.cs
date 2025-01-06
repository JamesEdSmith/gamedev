using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FishMover : MonoBehaviour
{
    const int STATE_IDLE = 1;
    const int STATE_ANTIC_STRAIGHT = 2;
    const int STATE_ANTIC_STRAIGHT_STOP = 3;
    const int STATE_CHASE = 4;
    const int STATE_NOTICE = 5;
    float timer;
    float idleTime = 5;

    int state;
    private float noticeTimer;
    private float desire;
    private float boredom;

    float acceleration;
    float currSpeed;
    float speed;
    float moveTime;
    private float rotRate;
    private Vector3 rotDir;
    private Quaternion currRotation;
    private Quaternion newRotation;
    HeadMovement headMovement;

    Transform lureTransform;
    Vector3 lurePrevPosition;
    List<float> lureSpeeds;
    float lureSpeed;
    public float sightRadius = 50;
    public float sightAngle = 100;
    public float minAttractSpeed = 1f;
    private float rotSpeed;
    private float chaseSpeed;
    public float maxSpeed = 25;
    public float minChaseSpeed;

    public TMP_Text debugText;
    public TMP_Text debugText2;

    Color testColor;

    // Start is called before the first frame update
    void Start()
    {
        testColor = new Color();
        headMovement = GetComponentInChildren<HeadMovement>();
        state = STATE_IDLE;
        lureTransform = GameObject.Find("lure").transform;
        lureSpeeds = new List<float>();
        lurePrevPosition = lureTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (debugText != null && debugText2 != null)
        {
            debugText2.text = debugText.text = "";
        }

        if (noticeTimer > 0)
        {
            noticeTimer -= Time.deltaTime;
        }

        lureSpeeds.Add(Vector3.Distance(lureTransform.position, lurePrevPosition));
        lurePrevPosition = lureTransform.position;

        if (lureSpeeds.Count > 60)
        {
            lureSpeeds.RemoveAt(0);
        }

        lureSpeed = 0;
        foreach (float speed in lureSpeeds)
        {
            lureSpeed += speed;
        }
        lureSpeed /= lureSpeeds.Count;

        if (state == STATE_NOTICE)
        {
            rotSpeed = lureSpeed * 10;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lureTransform.position - transform.position, Vector3.up), rotSpeed);

            if (lureSpeed/Time.deltaTime > minAttractSpeed)
            {
                desire += Time.deltaTime * 15f;
            }
            else
            {
                desire -= Time.deltaTime * 5f;
            }

            if (Mathf.Abs(lureSpeeds[0] - lureSpeed)/Time.deltaTime > minAttractSpeed)
            {
                boredom -= Time.deltaTime * 5f;
            }
            else
            {
                boredom += Time.deltaTime * 5f;
            }

            if (desire <= 0 || boredom > 100)
            {
                state = STATE_IDLE;
                noticeTimer = 3f;
            }
            else if (desire >= 100)
            {
                state = STATE_CHASE;
                chaseSpeed = 0;
                boredom = 0;
                desire = 50;
            }

            if (debugText != null && debugText2 != null)
            {
                debugText.text = "boredom: " + boredom + "lureSpeed: " + lureSpeed; 
                debugText2.text = "desire: " + desire;

                testColor.a = 1;
                testColor.r = 1;
                testColor.g = testColor.b = boredom / 100f;
                debugText.color = testColor;
                testColor.g = 1;
                testColor.r = testColor.b = desire / 100f;
                debugText2.color = testColor;

            }

        }
        else if (state == STATE_CHASE)
        {
            rotSpeed = lureSpeed * 10;

            float targetSpeed = Mathf.Clamp(lureSpeed * 1.5f / Time.deltaTime, minChaseSpeed, maxSpeed);
            if (targetSpeed > chaseSpeed)
            {
                chaseSpeed = Mathf.Lerp(chaseSpeed, targetSpeed, Time.deltaTime * 2f);
            }
            else
            {
                chaseSpeed = Mathf.Lerp(chaseSpeed, targetSpeed, Time.deltaTime / 2f);
            }

            headMovement.setSpeed(chaseSpeed);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lureTransform.position - transform.position, Vector3.up), rotSpeed);
            transform.Translate(Vector3.forward * chaseSpeed * Time.deltaTime);

            if (Vector3.Distance(lureTransform.position, transform.position) < 1f)
            {
                state = STATE_IDLE;
            }

            if (lureSpeed / Time.deltaTime > minAttractSpeed)
            {
                desire += Time.deltaTime * 15f;
            }
            else
            {
                desire -= Time.deltaTime * 5f;
            }

            if (Mathf.Abs(lureSpeeds[0] - lureSpeed) / Time.deltaTime > minAttractSpeed)
            {
                boredom -= Time.deltaTime * 5f;
            }
            else
            {
                boredom += Time.deltaTime * 5f;
            }

            if (desire <= 0 || boredom > 100)
            {
                state = STATE_IDLE;
                noticeTimer = 3f;
            }
        }
        else
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

            if (Vector3.Distance(lureTransform.position, transform.position) < sightRadius
                && Vector3.Angle(transform.forward, lureTransform.position - transform.position) < sightAngle
                && lureSpeed / Time.deltaTime >= minAttractSpeed
                && noticeTimer <= 0)
            {
                state = STATE_NOTICE;
                desire = 50f;
                boredom = 0;
            }
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
            speed = Random.Range(1f, 8f);
            moveTime = Random.Range(1f, 8f);
            rotRate = Random.Range(1f, 10f);
            rotDir = new Vector3(Random.Range(0, 1f), Random.Range(0, 5f), Random.Range(0, 0.1f));
            currSpeed = 0;

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
            transform.Translate(0, 0, currSpeed * Time.deltaTime);
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

            float t = (speed - currSpeed) / speed;
            transform.rotation = Quaternion.Slerp(currRotation, newRotation, t);
        }
        else
        {
            timer = 0;
            state = STATE_IDLE;
            idleTime = Random.Range(1f, 10f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (state == STATE_CHASE)
            Gizmos.color = Color.red;
        else if (state == STATE_NOTICE)
            Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }


}
