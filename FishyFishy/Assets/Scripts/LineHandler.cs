using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineHandler : MonoBehaviour
{
    LineRenderer lineRenderer;
    public List<Transform> lineTransforms;
    public Transform rodEnd;
    public Transform rodHolder;
    public float reelSpeed;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
            if ((lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position).magnitude > 0.01 && lineTransforms.Count > 4)
            {
                CharacterJoint joint = lineTransforms[lineTransforms.Count - 3].gameObject.GetComponent<CharacterJoint>();
                joint.autoConfigureConnectedAnchor = false;
                Vector3 direction = (lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position);
                lineTransforms[lineTransforms.Count - 2].rotation = Quaternion.Slerp(lineTransforms[lineTransforms.Count - 3].rotation, lineTransforms[lineTransforms.Count - 2].rotation, 5-direction.magnitude / 5f);
                if (direction.magnitude <= reelSpeed * Time.deltaTime)
                    joint.connectedAnchor = Vector3.zero;
                else
                    joint.connectedAnchor -= joint.connectedAnchor.normalized * reelSpeed * Time.deltaTime;

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
        }else if (Input.GetMouseButton(2))
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

        drawlines();
    }

    private void drawlines()
    {
        Vector3[] linePositions = new Vector3[lineTransforms.Count];

        for (int i = 0; i < lineTransforms.Count; i++)
        {
            linePositions[i] = lineTransforms[i].position;
        }

        lineRenderer.SetPositions(linePositions);
        lineRenderer.positionCount = lineTransforms.Count;
    }
}
