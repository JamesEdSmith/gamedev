using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    const float MAX_MOVEMEN_SPEED = 12;
    const float CATCH_BALL_TIME = 0.5f;
    const float SHOOT_BALL_TIME = 0.5f;
    const float STUN_TIME = 1f;

    public GameController gameController;

    float canCatchBallTimer = CATCH_BALL_TIME;
    float stunTimer = 0;
    float shootBallTimer = SHOOT_BALL_TIME;
    public Ball ball;
    public float ballDistance = 1f;
    public float ballThrowingForce;
    public GameObject floor;

    public bool holdingBall = true;

    //values that will be set in the Inspector
    public Transform target;
    public float RotationSpeed;

    //values for internal use
    private Quaternion _lookRotation;
    private Vector3 _direction;
    
    public float movementSpeed = 0;
    public float decellMagnitude = 20;

    public float angleToBasket;
    private float gravity = Physics.gravity.y;

    internal void loseBall()
    {
        holdingBall = false;
        ball = null;
        stunTimer = STUN_TIME;
    }

    public Transform basket;
    Vector3 s0;
    Vector3 s1;
    internal bool canCatchBall = false;
    private bool shooting = false;
    public bool cpu = true;

    void Start()
    {
        if (cpu)
        {
            decellMagnitude = 2;
        }
        s0 = new Vector3();
        s1 = new Vector3();
    }

    internal void catchBall(Ball ball)
    {
        if (canCatchBall)
        {
            ball.GetComponent<Rigidbody>().useGravity = false;
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.ActivateTrail(false);
            holdingBall = true;
            this.ball = ball;
            canCatchBall = false;
            canCatchBallTimer = CATCH_BALL_TIME;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        BasketKey key = other.gameObject.GetComponent<BasketKey>();
        if (key != null)
        {
            shooting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BasketKey key = other.gameObject.GetComponent<BasketKey>();
        if (key != null)
        {
            shooting = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        BasketKey key = other.gameObject.GetComponent<BasketKey>();
        if (key != null)
        {
            basket = key.getHoop();
        }
    }

    void Update()
    {
        if (holdingBall)
        {
            ball.transform.position = transform.position + transform.forward * ballDistance;
            Vector3 basketTarget = basket.TransformPoint(Vector3.zero);
            float y = basketTarget.y - transform.position.y;
            basketTarget.y = 0;
            float x = (basketTarget - transform.position).sqrMagnitude;

            fts.solve_ballistic_arc(ball.transform.position, ballThrowingForce, basket.TransformPoint(Vector3.zero), Physics.gravity.y * -1, out s0, out s1);

            //click mouse

            //if (Input.GetMouseButtonDown(0))
            //{
            //    shootBall();
            //}
        }
        else
        {
            if (canCatchBallTimer > 0f)
            {
                canCatchBallTimer -= Time.deltaTime;
                if (canCatchBallTimer <= 0f)
                {
                    canCatchBall = true;
                }
            }
        }

        float sqrMagnitude = -1;
        if (stunTimer <= 0)
        {
            if (cpu)
            {
                if (!holdingBall)
                {
                    foreach (Ball ball in gameController.balls)
                    {
                        float newSqrMagnitude = (ball.transform.position - transform.position).sqrMagnitude;
                        if (sqrMagnitude < 0 || sqrMagnitude > newSqrMagnitude)
                        {
                            sqrMagnitude = newSqrMagnitude;
                            target = ball.transform;
                        }

                    }
                }
                else
                {
                    target = gameController.getActiveKey();
                }
            }

            //find the vector pointing from our position to the target
            Vector3 targetPosition = target.TransformPoint(Vector3.zero);
            targetPosition.y = transform.position.y;
            _direction = (targetPosition - transform.position).normalized;

            //create the rotation we need to be in to look at the target
            _lookRotation = Quaternion.LookRotation(_direction);

            //rotate us over time according to speed until we are in the required rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * RotationSpeed);

            sqrMagnitude = (targetPosition - transform.position).sqrMagnitude;
            if ((cpu && sqrMagnitude > 0.01f) || (!cpu && sqrMagnitude > 1))
            {
                shootBallTimer = SHOOT_BALL_TIME;

                if (movementSpeed < MAX_MOVEMEN_SPEED)
                {
                    movementSpeed += 8f * Time.deltaTime;
                }
                else
                {
                    movementSpeed = MAX_MOVEMEN_SPEED;
                }
                if (sqrMagnitude < decellMagnitude)
                {
                    movementSpeed = sqrMagnitude / decellMagnitude * MAX_MOVEMEN_SPEED;
                }

                transform.position += transform.forward * Time.deltaTime * movementSpeed;
            }
            else
            {
                if (movementSpeed > 0)
                {
                    movementSpeed -= 10f * Time.deltaTime;
                }
                else
                {
                    movementSpeed = 0;
                }

                if (shooting && holdingBall)
                {
                    shootBallTimer -= Time.deltaTime;
                    if (shootBallTimer < 0)
                    {
                        shootBall();
                        shootBallTimer = SHOOT_BALL_TIME;
                    }
                }
                else
                {
                    shootBallTimer = SHOOT_BALL_TIME;
                }
            }
        }
        else
        {
            stunTimer -= Time.deltaTime;
        }
    }

    private void shootBall()
    {
        if (cpu)
        {
            stunTimer = STUN_TIME;
        }
        holdingBall = false;
        ball.ActivateTrail(true);
        ball.GetComponent<Rigidbody>().useGravity = true;
        Transform transformRotation = transform;
        transformRotation.Rotate(Mathf.Rad2Deg * angleToBasket, 0, 0);
        if (s1.x == 0 && s1.y == 0 && s1.z == 0)
        {
            ball.GetComponent<Rigidbody>().AddForce(s0.x, s0.y, s0.z, ForceMode.Impulse);
        }
        else
        {
            ball.GetComponent<Rigidbody>().AddForce(s1.x, s1.y, s1.z, ForceMode.Impulse);
        }
        ball = null;
    }
}
