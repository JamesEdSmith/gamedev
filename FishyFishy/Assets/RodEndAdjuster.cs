using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodEndAdjuster : MonoBehaviour
{
    public Transform rodHolder;
    Transform myTransform;
    public float rodLength;

    // Start is called before the first frame update
    void Start()
    {
        myTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(myTransform.position, rodHolder.position)> rodLength)
        {
            Vector3 direction = (myTransform.position - rodHolder.position).normalized;
            GetComponent<Rigidbody>().MovePosition(rodHolder.position + direction * rodLength);
        }
    }
}
