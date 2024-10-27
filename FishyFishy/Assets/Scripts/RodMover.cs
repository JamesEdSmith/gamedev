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
    Transform rodTransform;

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
        meTransform.rotation = Quaternion.RotateTowards(meTransform.rotation, targetRotation, anglePerSecond * Time.deltaTime);




        targetRotation = Quaternion.LookRotation(rodEnd.position - meTransform.position);

        Vector3 rotation = targetRotation.eulerAngles - meTransform.rotation.eulerAngles;

        foreach (Transform t in rodTransforms)
        {
            t.rotation = Quaternion.identity;
        }

        float x = 9f * rotation.magnitude / 13f;

        rodTransforms[0].rotation = Quaternion.RotateTowards(meTransform.rotation, targetRotation, 180);
        rodTransforms[1].rotation = Quaternion.RotateTowards(meTransform.rotation, targetRotation, 180);
        rodTransforms[2].rotation = Quaternion.RotateTowards(meTransform.rotation, targetRotation, 180);
        
    }
}
