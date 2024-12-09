using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallInWater : MonoBehaviour
{
    Rigidbody body;
    float outOfWaterDrag;
    float outOfWaterMass;
    public float inWaterDrag;
    public float inWaterMass;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        outOfWaterDrag = body.drag;
        outOfWaterMass = body.mass;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Water" && body != null)
        {
            body.drag = inWaterDrag;
            body.mass = inWaterMass;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Water" && body != null)
        {
            body.drag = outOfWaterDrag;
            body.mass = outOfWaterMass;
        }
    }
}
