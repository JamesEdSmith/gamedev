using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LineHandler : MonoBehaviour
{
    LineRenderer lineRenderer;
    public List<Transform> lineTransforms;
    public Transform rodEnd;
    public Transform rodHolder;
    public float reelSpeed;
    public float reelAcc;
    float currReelSpeed = 0;
    public Color OverColor;
    public Color UnderColor;
    Gradient normalGradient;
    Gradient crazyGradient;
    float decel;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        normalGradient = new Gradient();
        crazyGradient = new Gradient();
        normalGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(OverColor, 0.0f), new GradientColorKey(OverColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1, 0.0f), new GradientAlphaKey(1, 1.0f) }
        );

        crazyGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(UnderColor, 0.0f), new GradientColorKey(OverColor, 0.5f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1, 0.0f), new GradientAlphaKey(1, 1.0f) }
        );
    }

    public TMP_Text debugText;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (debugText != null)
        {
            debugText.text = "reelSpeed: " + currReelSpeed;
        }

        if (Input.GetMouseButton(0))
        {

            if ((lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position).magnitude >= 5)
            {
                if (lineTransforms[lineTransforms.Count - 3].GetComponent<CharacterJoint>() == null)
                {
                    CharacterJoint joint = lineTransforms[lineTransforms.Count - 3].gameObject.AddComponent<CharacterJoint>();
                    joint.connectedBody = lineTransforms[lineTransforms.Count - 2].gameObject.GetComponent<Rigidbody>();
                    joint.connectedAnchor = lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position;
                    joint.anchor = Vector3.zero;
                    SoftJointLimit limit = joint.lowTwistLimit;
                    limit.limit = -177;
                    joint.lowTwistLimit = limit;
                    limit.limit = 177;
                    joint.highTwistLimit = limit;
                    joint.swing1Limit = limit;
                    joint.swing2Limit = limit;
                    joint.enableProjection = true;
                    joint.projectionAngle = 0.1f;
                    joint.projectionDistance = 0.1f;

                }

                Rigidbody bod = lineTransforms[lineTransforms.Count - 2].gameObject.GetComponent<Rigidbody>();

                GameObject newline = Instantiate(lineTransforms[lineTransforms.Count - 2].gameObject, lineTransforms[lineTransforms.Count - 2].parent);

                bod.constraints = RigidbodyConstraints.None;
                lineTransforms[lineTransforms.Count - 2].parent = null;

                lineTransforms.Insert(lineTransforms.Count - 1, newline.transform);
            }
            else if (lineTransforms[lineTransforms.Count - 3].GetComponent<CharacterJoint>() != null)
            {
                Destroy(lineTransforms[lineTransforms.Count - 3].GetComponent<CharacterJoint>());
            }
        }
        else if (Input.GetMouseButton(1))
        {
            if (currReelSpeed < reelSpeed)
            {
                currReelSpeed = Mathf.Min(currReelSpeed + Time.deltaTime * reelAcc, reelSpeed);
            }
            decel = reelAcc * 8f;
        }
        else if (Input.mouseScrollDelta.magnitude > 0)
        {
            if (currReelSpeed < reelSpeed)
            {
                currReelSpeed += Input.mouseScrollDelta.magnitude * 0.03f * reelSpeed;
            }
            decel = reelAcc;
        }
        else if (Input.GetMouseButton(2))
        {
            //Vector3 direction = (rodHolder.position - lineTransforms[lineTransforms.Count - 2].position).normalized * 5000 * Time.deltaTime;
            //lineTransforms[lineTransforms.Count - 2].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            //lineTransforms[lineTransforms.Count - 2].GetComponent<Rigidbody>().MovePosition(lineTransforms[lineTransforms.Count - 2].position + direction);
            //lineTransforms[lineTransforms.Count - 2].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;

            rodHolder.Translate(0, 0, -100 * Time.deltaTime);
        }
        else
        {
            if ((lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position).magnitude >= 5)
            {
                if (lineTransforms[lineTransforms.Count - 3].GetComponent<CharacterJoint>() == null)
                {
                    CharacterJoint joint = lineTransforms[lineTransforms.Count - 3].gameObject.AddComponent<CharacterJoint>();
                    joint.connectedBody = lineTransforms[lineTransforms.Count - 2].gameObject.GetComponent<Rigidbody>();
                    joint.connectedAnchor = lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position;
                    joint.anchor = Vector3.zero;
                    SoftJointLimit limit = joint.lowTwistLimit;
                    limit.limit = -177;
                    joint.lowTwistLimit = limit;
                    limit.limit = 177;
                    joint.highTwistLimit = limit;
                    joint.swing1Limit = limit;
                    joint.swing2Limit = limit;
                }
            }
        }

        if(currReelSpeed > 0)
        {
            if (lineTransforms[lineTransforms.Count - 3].gameObject.GetComponent<CharacterJoint>() != null && lineTransforms[lineTransforms.Count - 3].gameObject.GetComponent<CharacterJoint>().connectedAnchor.magnitude > 0 && lineTransforms.Count > 4)
            {
                CharacterJoint joint = lineTransforms[lineTransforms.Count - 3].gameObject.GetComponent<CharacterJoint>();
                joint.autoConfigureConnectedAnchor = false;
                lineTransforms[lineTransforms.Count - 2].rotation = Quaternion.Slerp(lineTransforms[lineTransforms.Count - 3].rotation, lineTransforms[lineTransforms.Count - 2].rotation, 5 - joint.connectedAnchor.magnitude / 5f);
                if (joint.connectedAnchor.magnitude <= currReelSpeed * Time.deltaTime)
                    joint.connectedAnchor = Vector3.zero;
                else
                    joint.connectedAnchor -= joint.connectedAnchor.normalized * currReelSpeed * Time.deltaTime;

            }
            else if (lineTransforms.Count > 4)
            {
                if (lineTransforms[lineTransforms.Count - 3].GetComponent<CharacterJoint>() != null)
                {
                    Destroy(lineTransforms[lineTransforms.Count - 3].gameObject.GetComponent<CharacterJoint>());
                }
                lineTransforms[lineTransforms.Count - 3].parent = lineTransforms[lineTransforms.Count - 2].parent;
                lineTransforms[lineTransforms.Count - 3].position = lineTransforms[lineTransforms.Count - 2].position;
                Rigidbody bod = lineTransforms[lineTransforms.Count - 3].gameObject.GetComponent<Rigidbody>();
                bod.MovePosition(lineTransforms[lineTransforms.Count - 2].position);
                bod.constraints = RigidbodyConstraints.FreezePosition;

                Transform reeledT = lineTransforms[lineTransforms.Count - 2];
                lineTransforms.Remove(reeledT);
                Destroy(reeledT.gameObject);
            }
        }

        if (!Input.GetMouseButton(1) && Input.mouseScrollDelta.magnitude == 0)
        {
            currReelSpeed = Mathf.Max(currReelSpeed - Time.deltaTime * decel, 0);
        }

        drawlines();
    }

    int waterTally = 0;
    Vector3[] linePositions;


    private void drawlines()
    {
        if (linePositions == null || linePositions.Length != lineTransforms.Count)
        {
            linePositions = new Vector3[lineTransforms.Count];
        }

        int prevTally = waterTally;
        waterTally = 0;
        bool stopCounting = false;

        for (int i = 0; i < lineTransforms.Count; i++)
        {
            linePositions[i] = lineTransforms[i].position;
            if (linePositions[i].y < 0 && !stopCounting)
            {
                waterTally++;
            }
            else
            {
                stopCounting = true;
            }
        }

        lineRenderer.SetPositions(linePositions);
        lineRenderer.positionCount = lineTransforms.Count;

        if (waterTally > 0 && waterTally != prevTally)
        {
            float waterStart = (float)waterTally / (float)lineTransforms.Count;
            crazyGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(UnderColor, 0.0f), new GradientColorKey(OverColor, waterStart) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0.0f), new GradientAlphaKey(1, 1.0f) }
            );

            lineRenderer.colorGradient = crazyGradient;
        }
        else if (waterTally != prevTally)
        {
            lineRenderer.colorGradient = normalGradient;
        }

    }
}
