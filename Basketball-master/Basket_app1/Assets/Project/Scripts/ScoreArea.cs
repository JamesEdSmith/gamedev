using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreArea : MonoBehaviour
{

    public GameObject effectObject;

    void Start()
    {
        effectObject.SetActive(false);
    }

    void OnTriggerEnter(Collider otherCollider)
    {
        Ball ball = otherCollider.GetComponent<Ball>();

        if (ball != null)
        {
            ball.dead = true;
            ball.transform.position = new Vector3(0, -50, 0);
            effectObject.SetActive(true);

        }
    }


}
