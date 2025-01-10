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
    const int STATE_CHOMP = 6;
    const int STATE_HOOKED = 7;
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
    MouthMovement mouthMovement;

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
    public float chompTime;
    float chompTimer;
    float chompSpeed;

    public TMP_Text debugText;
    public TMP_Text debugText2;

    Color testColor;

    // Start is called before the first frame update
    void Start()
    {
        testColor = new Color();
        headMovement = GetComponentInChildren<HeadMovement>();
        mouthMovement = GetComponentInChildren<MouthMovement>();
        state = STATE_IDLE;
        lureTransform = GameObject.Find("lure").transform;
        lureSpeeds = new List<float>();
        lurePrevPosition = lureTransform.position;
        transform.Rotate(0, Random.Range(0, 360), 0);
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

        if (debugText != null)
            debugText.text = "lureSpeed: " + lureSpeed;

        if (state == STATE_NOTICE)
        {
            rotSpeed = lureSpeed * 10;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lureTransform.position - transform.position, Vector3.up), rotSpeed);

            if (lureSpeed > minAttractSpeed)
            {
                desire += Time.deltaTime * 35f;
            }
            else
            {
                desire -= Time.deltaTime * 5f;
            }

            if (Mathf.Abs(lureSpeeds[0] - lureSpeed) > minAttractSpeed)
            {
                boredom = Mathf.Max(boredom - Time.deltaTime * 15f, 0);
            }
            else
            {
                boredom += Time.deltaTime * 20f;
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

            if (lureSpeed > minAttractSpeed)
            {
                desire += Time.deltaTime * 35f;
            }
            else
            {
                desire -= Time.deltaTime * 5f;
            }

            if (Mathf.Abs(lureSpeeds[0] - lureSpeed) > minAttractSpeed)
            {
                boredom = Mathf.Max(boredom - Time.deltaTime * 15f, 0);
            }
            else
            {
                boredom += Time.deltaTime * 20f;
            }

            if (desire <= 0 || boredom > 100)
            {
                setupAnticStop();
                headMovement.setSpeed(0);
                noticeTimer = 3f;
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

            if (Vector3.Distance(transform.position, lureTransform.position) < 6)
            {
                state = STATE_CHOMP;
                chompTimer = 0;
                mouthMovement.control();
            }
        }
        else if (state == STATE_CHOMP)
        {
            rotSpeed = lureSpeed * 10;
            chompTimer += Time.deltaTime;
            if (chompTimer < chompTime / 4f)
            {
                float targetSpeed = chaseSpeed * 0.75f;
                chompSpeed = Mathf.Lerp(chaseSpeed, targetSpeed, chompTimer / (chompTime / 4f));
                mouthMovement.setAngle(Mathf.Lerp(0, mouthMovement.biteAngleMax, chompTimer / (chompTime / 4f)));
            }
            else
            {
                chompSpeed = Mathf.Lerp(chaseSpeed * 0.75f, maxSpeed * 2f, Mathf.Abs((chompTimer - chompTime / 4f) - (chompTime * 0.375f)) / (chompTime * 3f / 4f));
                mouthMovement.setAngle(Mathf.Lerp(mouthMovement.biteAngleMax, 0, (chompTimer - chompTime / 4f) / (chompTime * 3f / 4f)));
            }
            headMovement.setSpeed(chompSpeed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lureTransform.position - transform.position, Vector3.up), rotSpeed);
            transform.Translate(Vector3.forward * chompSpeed * Time.deltaTime);

            if (chompTimer >= chompTime * 0.6f &&
                Vector3.Distance(transform.position + Vector3.forward * 2.25f, lureTransform.position) <= 0.25f ||
                Vector3.Distance(transform.position, lureTransform.position) <= 1f)
            {
                state = STATE_HOOKED;
                CharacterJoint joint = gameObject.AddComponent<CharacterJoint>();
                joint.connectedBody = lureTransform.gameObject.GetComponent<Rigidbody>();
                joint.connectedAnchor = Vector3.zero;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector3.forward * 2.25f;
                SoftJointLimit limit = joint.lowTwistLimit;
                limit.limit = -177;
                joint.lowTwistLimit = limit;
                limit.limit = 177;
                joint.highTwistLimit = limit;
                joint.swing1Limit = limit;
                joint.swing2Limit = limit;
                joint.enableProjection = true;
                joint.projectionAngle = 0.1f;
                joint.projectionDistance = 0.1f;
            }
            else if (chompTimer >= chompTime)
            {
                mouthMovement.controlOff();

                if (Vector3.Distance(transform.position + Vector3.forward * 2.25f, lureTransform.position) > 0.25f ||
                Vector3.Distance(transform.position, lureTransform.position) <= 1f)
                {
                    state = STATE_CHASE;
                }
                else
                {
                    state = STATE_HOOKED;
                    CharacterJoint joint = gameObject.AddComponent<CharacterJoint>();
                    joint.connectedBody = lureTransform.gameObject.GetComponent<Rigidbody>();
                    joint.connectedAnchor = Vector3.zero;
                    joint.autoConfigureConnectedAnchor = false;
                    joint.anchor = Vector3.forward * 2.25f;
                    SoftJointLimit limit = joint.lowTwistLimit;
                    limit.limit = -177;
                    joint.lowTwistLimit = limit;
                    limit.limit = 177;
                    joint.highTwistLimit = limit;
                    joint.swing1Limit = limit;
                    joint.swing2Limit = limit;
                    joint.enableProjection = true;
                    joint.projectionAngle = 0.1f;
                    joint.projectionDistance = 0.1f;
                }
            }
        }
        else if (state == STATE_HOOKED)
        {

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
                && lureSpeed >= minAttractSpeed
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
            setupAnticStop();
        }
    }

    private void setupAnticStop()
    {
        timer = 0;
        state = STATE_ANTIC_STRAIGHT_STOP;
        acceleration = Random.Range(1f, 5f);
        newRotation = currRotation = transform.rotation;
        newRotation.Set(newRotation.x, newRotation.y, 0, newRotation.w);
        newRotation.Set(0, newRotation.y, newRotation.z, newRotation.w);
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
        else if (state == STATE_CHOMP)
            Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }


}
