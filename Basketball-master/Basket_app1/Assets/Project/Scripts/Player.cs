using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    const float MAX_MOVEMEN_SPEED = 12;
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
    public float sqrMagnitude;
    public float movementSpeed = 0;
    public float decellMagnitude = 20;

    public float angleToBasket;
    private float gravity = Physics.gravity.y;

    public Transform basket;
    Vector3 s0;
    Vector3 s1;


    // Use this for initialization
    void Start()
    {
        ball.GetComponent<Rigidbody>().useGravity = false;
        s0 = new Vector3();
        s1 = new Vector3();

    }

    // Update is called once per frame
    void Update()
    {
        if (holdingBall)
        {
            ball.transform.position = transform.position + transform.forward * ballDistance;
            Vector3 target = basket.TransformPoint(Vector3.zero);
            float y = target.y - transform.position.y;
            target.y = 0;
            float x = (target - transform.position).sqrMagnitude;

            fts.solve_ballistic_arc(ball.transform.position, ballThrowingForce, basket.TransformPoint(Vector3.zero), Physics.gravity.y * -1, out s0, out s1);

            
            //clic mouse

            if (Input.GetMouseButtonDown(0))
            {
                holdingBall = false;
                ball.ActivateTrail();
                ball.GetComponent<Rigidbody>().useGravity = true;
                Transform transformRotation = transform;
                transformRotation.Rotate(Mathf.Rad2Deg * angleToBasket, 0, 0);
                ball.GetComponent<Rigidbody>().AddForce(s1.x,s1.y,s1.z,ForceMode.Impulse);
                //Quaternion.AngleAxis(Mathf.Rad2Deg * angleToBasket, Vector3.left) 
            }

        }

        if (Input.GetMouseButtonDown(1))
        {
            ball.GetComponent<Rigidbody>().useGravity = false;
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            holdingBall = true;
        }

        //find the vector pointing from our position to the target
        Vector3 targetPosition = target.position;
        targetPosition.y = transform.position.y;
        _direction = (targetPosition - transform.position).normalized;

        //create the rotation we need to be in to look at the target
        _lookRotation = Quaternion.LookRotation(_direction);

        //rotate us over time according to speed until we are in the required rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * RotationSpeed);

        sqrMagnitude = (targetPosition - transform.position).sqrMagnitude;
        if (sqrMagnitude > 1)
        {

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
        }

    }


}
