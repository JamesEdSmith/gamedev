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

    // Start is called before the first frame update
    void Start()
    {
        whammyCammy = Camera.main;
        meTransform = transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 test = Input.mousePosition;
        test.z = whammyCammy.transform.position.y + mouseTargetAdjust;
        Vector3 targetPoint = whammyCammy.ScreenToWorldPoint(test);

        var targetRotation = Quaternion.LookRotation((targetPoint - transform.position).normalized);
        var targetRotation2 = Quaternion.LookRotation(rodEnd.position - rodTransforms[2].position);

        float x = 9f * 1f / 13f;
        float dist = meTransform.InverseTransformPoint(rodEnd.position).x - meTransform.localPosition.x;

        //float angle = Mathf.Asin(dist/6) * 9f / 13f;
        //float fullAngle = Quaternion.Angle(meTransform.rotation, targetRotation2);

        float rot0 = 0;
        float rot1 = 0;
        float rot2 = 0;


        var hmm = Quaternion.LookRotation(rodEnd.position - meTransform.position);
        var boop = Quaternion.LookRotation(rodTransforms[2].position - meTransform.position);

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
