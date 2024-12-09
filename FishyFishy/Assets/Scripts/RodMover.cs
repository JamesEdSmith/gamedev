using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodMover : MonoBehaviour
{
    Camera whammyCammy;
    Transform meTransform;
    public float mouseTargetAdjust;
    public int anglePerSecond;

    public List<Transform> rodTransforms;
    public Transform rodEnd;
    public Vector3 originalPos;
    public CameraMode cameraMode;

    public Transform testPos;


    // Start is called before the first frame update
    void Start()
    {
        whammyCammy = Camera.main;
        originalPos = whammyCammy.transform.position;
        meTransform = transform;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = originalPos.y + mouseTargetAdjust;
        Vector3 targetPoint = whammyCammy.ScreenToWorldPoint(mousePos);


        if (cameraMode.state == CameraMode.State.follow)
        {
            if (mousePos.x < Screen.width / 2)
            {
                mousePos.x -= (Screen.width / 2 - mousePos.x) * 5f;
            }
            else
            {
                mousePos.x += (mousePos.x - Screen.width / 2) * 5f;
            }

            if (mousePos.y < Screen.height / 2)
            {
                mousePos.y -= (Screen.height / 2 - mousePos.y) * 5f;
            }
            else
            {
                mousePos.y += (mousePos.y - Screen.height / 2) * 5f;
            }

            Vector3 cameraPos = whammyCammy.transform.position;
            Quaternion cameraRot = whammyCammy.transform.rotation;

            whammyCammy.transform.position = transform.position;
            Vector3 lurePosition = cameraMode.lureTransform.position;
            lurePosition.y = 10;
            whammyCammy.transform.LookAt(lurePosition);
            mousePos.z = 5;
            targetPoint = whammyCammy.ScreenToWorldPoint(mousePos);
            testPos.position = targetPoint;
            whammyCammy.transform.position = cameraPos;
            whammyCammy.transform.rotation = cameraRot;
        }

        Quaternion targetRotation = Quaternion.LookRotation((targetPoint - transform.position).normalized);
        Quaternion targetRotation2 = Quaternion.LookRotation(rodEnd.position - rodTransforms[2].position);

        //float angle = Mathf.Asin(dist/6) * 9f / 13f;
        //float fullAngle = Quaternion.Angle(meTransform.rotation, targetRotation2);

        float rot0 = 0;
        float rot1 = 0;
        float rot2 = 0;

        rodTransforms[2].rotation = meTransform.rotation;
        rodTransforms[1].rotation = meTransform.rotation;
        rodTransforms[0].rotation = meTransform.rotation;

        float magnitude = (rodTransforms[2].position - rodEnd.position).magnitude;

        while (rot0 < 100)
        {

            //if (rot0 < 360)
            //{
            rot0 += 3;
            rot1 += 1;
            rot2 += 0.3f;
            //}

            rodTransforms[2].rotation = Quaternion.Slerp(meTransform.rotation, targetRotation2, rot0 / 100f);
            rodTransforms[1].rotation = Quaternion.Slerp(meTransform.rotation, targetRotation2, rot1 / 100f);
            rodTransforms[0].rotation = Quaternion.Slerp(meTransform.rotation, targetRotation2, rot2 / 100f);

            if (magnitude > (rodTransforms[2].position - rodEnd.position).magnitude)
            {
                magnitude = (rodTransforms[2].position - rodEnd.position).magnitude;
            }
            else
            {
                rodTransforms[2].rotation = Quaternion.Slerp(meTransform.rotation, targetRotation2, (rot0 - 3) / 100f);
                rodTransforms[1].rotation = Quaternion.Slerp(meTransform.rotation, targetRotation2, (rot1 - 1) / 100f);
                rodTransforms[0].rotation = Quaternion.Slerp(meTransform.rotation, targetRotation2, (rot2 - 0.3f) / 100f);
                break;
            }
        }

        meTransform.rotation = Quaternion.RotateTowards(meTransform.rotation, targetRotation, anglePerSecond * Time.deltaTime);
    }
}
