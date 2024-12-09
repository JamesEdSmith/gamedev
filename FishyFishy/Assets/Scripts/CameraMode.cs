using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMode : MonoBehaviour
{
    public enum State
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

    public State state;
    Camera mainCamera;

    Color aboveWaterColor = new Color(0.33f, 0.33f, 1f);
    Color belowWaterColor = new Color(0, 0.0f, 1f);

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        timer = zoomTime;
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            state = State.follow;
            timer = 0;
        }
        else if (!Input.GetMouseButton(0) && Vector3.Distance(lureTransform.position, rodTransform.position) <= returnDistance && state != State.normal)
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

        if (transform.position.y < 0)
        {
            belowWaterColor.b = (100f + transform.position.y) / 100f;
            mainCamera.backgroundColor = belowWaterColor;
        }
        else
        {
            mainCamera.backgroundColor = aboveWaterColor;
        }
    }
}
