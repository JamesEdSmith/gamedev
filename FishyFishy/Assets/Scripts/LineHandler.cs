using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineHandler : MonoBehaviour
{
    LineRenderer lineRenderer;
    public List<Transform> lineTransforms;
    public Transform rodEnd;
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
            if ((lineTransforms[lineTransforms.Count - 1].position - lineTransforms[lineTransforms.Count - 2].position).magnitude >= 5)
            {
                if (lineTransforms[lineTransforms.Count - 2].GetComponent<HingeJoint>() == null)
                {
                    HingeJoint joint = lineTransforms[lineTransforms.Count - 2].gameObject.AddComponent<HingeJoint>();
                    joint.connectedBody = lineTransforms[lineTransforms.Count - 1].gameObject.GetComponent<Rigidbody>();
                    joint.connectedAnchor = lineTransforms[lineTransforms.Count - 1].position - lineTransforms[lineTransforms.Count - 2].position;
                    joint.anchor = Vector3.zero;

                }

                Rigidbody bod = lineTransforms[lineTransforms.Count - 1].gameObject.GetComponent<Rigidbody>();

                GameObject newline = Instantiate(lineTransforms[lineTransforms.Count - 1].gameObject, lineTransforms[lineTransforms.Count - 1].parent);

                bod.constraints = RigidbodyConstraints.None;
                lineTransforms[lineTransforms.Count - 1].parent = null;

                lineTransforms.Add(newline.transform);
            }
            else if (lineTransforms[lineTransforms.Count - 2].GetComponent<HingeJoint>() != null)
            {
                Destroy(lineTransforms[lineTransforms.Count - 2].GetComponent<HingeJoint>());
            }
        }
        else if (Input.GetMouseButton(1))
        {
            if ((lineTransforms[lineTransforms.Count - 1].position - lineTransforms[lineTransforms.Count - 2].position).magnitude > 0.01 && lineTransforms.Count > 3)
            {
                HingeJoint joint = lineTransforms[lineTransforms.Count - 2].gameObject.GetComponent<HingeJoint>();
                joint.autoConfigureConnectedAnchor = false;
                Vector3 direction = (lineTransforms[lineTransforms.Count - 1].position - lineTransforms[lineTransforms.Count - 2].position);
                if (direction.magnitude <= reelSpeed * Time.deltaTime)
                    joint.connectedAnchor = Vector3.zero;
                else
                    joint.connectedAnchor -= joint.connectedAnchor.normalized * reelSpeed * Time.deltaTime;
            }
            else if (lineTransforms.Count > 3)
            {
                if (lineTransforms[lineTransforms.Count - 2].GetComponent<HingeJoint>() != null)
                {
                    Destroy(lineTransforms[lineTransforms.Count - 2].gameObject.GetComponent<HingeJoint>());
                }
                lineTransforms[lineTransforms.Count - 2].parent = lineTransforms[lineTransforms.Count - 1].parent;
                lineTransforms[lineTransforms.Count - 2].position = lineTransforms[lineTransforms.Count - 1].position;
                Rigidbody bod = lineTransforms[lineTransforms.Count - 2].gameObject.GetComponent<Rigidbody>();
                //bod.MovePosition(lineTransforms[lineTransforms.Count - 1].position);

                bod.constraints = RigidbodyConstraints.FreezePosition;
                Transform reeledT = lineTransforms[lineTransforms.Count - 1];
                lineTransforms.Remove(reeledT);
                Destroy(reeledT.gameObject);
            }
        }
        else
        {
            if ((lineTransforms[lineTransforms.Count - 1].position - lineTransforms[lineTransforms.Count - 2].position).magnitude >= 5)
            {
                if (lineTransforms[lineTransforms.Count - 2].GetComponent<HingeJoint>() == null)
                {
                    HingeJoint joint = lineTransforms[lineTransforms.Count - 2].gameObject.AddComponent<HingeJoint>();
                    joint.connectedBody = lineTransforms[lineTransforms.Count - 1].gameObject.GetComponent<Rigidbody>();
                    joint.connectedAnchor = lineTransforms[lineTransforms.Count - 1].position - lineTransforms[lineTransforms.Count - 2].position;
                    joint.anchor = Vector3.zero;
                    //joint.autoConfigureConnectedAnchor = false;
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
