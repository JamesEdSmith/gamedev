using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMode : MonoBehaviour
{
    enum State
    {
        normal,
        follow
    }

    public Transform lureTransform;
    public Transform rodTransform;
    Transform followTransform;

    public float zoomTime;
    public float returnDistance;
    public float followDistance;
    float timer;
    Vector3 startPos;
    Quaternion startRot;

    State state;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        timer = zoomTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            state = State.follow;
            timer = 0;
        }

        if (Vector3.Distance(lureTransform.position, rodTransform.position) <= returnDistance && state != State.normal)
        {
            state = State.normal;
            timer = 0;
        }

        Vector3 followPosition = Vector3.MoveTowards(lureTransform.position, rodTransform.position, followDistance);
        Quaternion followRotation = Quaternion.LookRotation((lureTransform.position - rodTransform.position).normalized);
        timer += Time.deltaTime;
        float t = timer / zoomTime;

        switch (state)
        {
            case State.normal:
                transform.position = Vector3.Lerp(followPosition, startPos, t);
                transform.rotation = Quaternion.Slerp(followRotation, startRot, t);
                break;
            case State.follow:
                transform.position = Vector3.Lerp(startPos, followPosition, t);
                transform.rotation = Quaternion.Slerp(startRot, followRotation, t);
                break;
        }
    }
}
