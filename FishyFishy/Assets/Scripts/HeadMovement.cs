using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadMovement : MonoBehaviour
{
    Quaternion startingRotation;
    Quaternion initialRotation;

    private Quaternion targetRotation;
    private float timer;
    public float time = 1;
    public float moveSize = 15;

    float currTime;
    float currMoveSize;

    public float speedRate = 0.25f;

    bool posTurn = true;

    // Start is called before the first frame update
    void Start()
    {
        startingRotation = initialRotation = transform.localRotation;
        currTime = time;
        currMoveSize = moveSize;
        targetRotation = initialRotation * Quaternion.Euler(moveSize, 0, 0);
        timer = time * 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Slerp(startingRotation, targetRotation, EasingFunction.EaseInOutSine(0, 1, timer / currTime));
        //transform.rotation = Quaternion.Slerp(startingRotation, targetRotation,  timer / time);
        timer += Time.deltaTime;

        if (timer > currTime)
        {
            startingRotation = transform.localRotation = targetRotation;
            posTurn = !posTurn;
            timer = 0;
            if (posTurn)
            {
                targetRotation = initialRotation * Quaternion.Euler(currMoveSize, 0, 0);
            }
            else
            {
                targetRotation = initialRotation * Quaternion.Euler(-currMoveSize, 0, 0);
            }
        }
    }

    internal void setSpeed(float currSpeed)
    {
        if (currSpeed * speedRate > 0)
        {
            currTime = time / currSpeed * speedRate;
        }
        else
        {
            currTime = time;
        }
    }
}
