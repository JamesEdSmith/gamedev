using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpeck : MonoBehaviour
{
    private float xSpace;
    private float ySpace;
    private float zSpace;

    private float xOffset;
    private float yOffset;
    private float zOffset;

    Transform myTransform;
    Vector3 newSpot;

    // Start is called before the first frame update
    void Start()
    {
        myTransform = transform;
        xSpace = Random.Range(0.01f, 0.1f);
        ySpace = Random.Range(0.01f, 0.1f);
        zSpace = Random.Range(0.01f, 0.1f);

        xOffset = Random.Range(0, 1f);
        yOffset = Random.Range(0, 1f);
        zOffset = Random.Range(0, 1f);

        newSpot = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        newSpot = myTransform.position;
        newSpot.x += xSpace* Mathf.Sin(Time.time + xOffset);
        newSpot.y += ySpace * Mathf.Sin(Time.time + yOffset);
        newSpot.z += zSpace * Mathf.Sin(Time.time + zOffset);
        myTransform.position = newSpot;
    }
}
