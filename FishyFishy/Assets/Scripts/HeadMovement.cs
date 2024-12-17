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
    bool posTurn = true;

    // Start is called before the first frame update
    void Start()
    {
        startingRotation = initialRotation = transform.rotation;
        targetRotation = initialRotation * Quaternion.Euler(moveSize, 0, 0);
        timer = time * 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Slerp(startingRotation, targetRotation, EasingFunction.EaseInOutSine(0, 1, timer / time));
        //transform.rotation = Quaternion.Slerp(startingRotation, targetRotation,  timer / time);
        timer += Time.deltaTime;

        if (timer > time)
        {
            transform.rotation = targetRotation;
            posTurn = !posTurn;
            timer = 0;
            startingRotation = transform.rotation;
            if (posTurn)
            {
                targetRotation = initialRotation * Quaternion.Euler(moveSize, 0, 0);
            }
            else
            {
                targetRotation = initialRotation * Quaternion.Euler(-moveSize, 0, 0);
            }
        }
    }
}
