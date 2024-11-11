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
    public Rigidbody lureBody;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        lureBody.drag = 0.2f;

        if (Input.GetMouseButton(0))
        {
            if ((lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position).magnitude >= 5)
            {
                if (lineTransforms[lineTransforms.Count - 3].GetComponent<HingeJoint>() == null)
                {
                    HingeJoint joint = lineTransforms[lineTransforms.Count - 3].gameObject.AddComponent<HingeJoint>();
                    joint.connectedBody = lineTransforms[lineTransforms.Count - 2].gameObject.GetComponent<Rigidbody>();
                    joint.connectedAnchor = lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position;
                    joint.anchor = Vector3.zero;

                }

                Rigidbody bod = lineTransforms[lineTransforms.Count - 2].gameObject.GetComponent<Rigidbody>();

                GameObject newline = Instantiate(lineTransforms[lineTransforms.Count - 2].gameObject, lineTransforms[lineTransforms.Count - 2].parent);

                bod.constraints = RigidbodyConstraints.None;
                lineTransforms[lineTransforms.Count - 2].parent = null;

                lineTransforms.Insert(lineTransforms.Count - 1, newline.transform);
            }
            else if (lineTransforms[lineTransforms.Count - 3].GetComponent<HingeJoint>() != null)
            {
                Destroy(lineTransforms[lineTransforms.Count - 3].GetComponent<HingeJoint>());
            }
        }
        else if (Input.GetMouseButton(1))
        {
            lureBody.drag = 60;
            if ((lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position).magnitude > 0.01 && lineTransforms.Count > 4)
            {
                HingeJoint joint = lineTransforms[lineTransforms.Count - 3].gameObject.GetComponent<HingeJoint>();
                joint.autoConfigureConnectedAnchor = false;
                Vector3 direction = (lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position);
                if (direction.magnitude <= reelSpeed * Time.deltaTime)
                    joint.connectedAnchor = Vector3.zero;
                else
                    joint.connectedAnchor -= joint.connectedAnchor.normalized * reelSpeed * Time.deltaTime;
            }
            else if (lineTransforms.Count > 4)
            {
                if (lineTransforms[lineTransforms.Count - 3].GetComponent<HingeJoint>() != null)
                {
                    Destroy(lineTransforms[lineTransforms.Count - 3].gameObject.GetComponent<HingeJoint>());
                }
                lineTransforms[lineTransforms.Count - 3].parent = lineTransforms[lineTransforms.Count - 2].parent;
                lineTransforms[lineTransforms.Count - 3].position = lineTransforms[lineTransforms.Count - 2].position;
                Rigidbody bod = lineTransforms[lineTransforms.Count - 3].gameObject.GetComponent<Rigidbody>();
                //bod.MovePosition(lineTransforms[lineTransforms.Count - 1].position);

                bod.constraints = RigidbodyConstraints.FreezePosition;
                Transform reeledT = lineTransforms[lineTransforms.Count - 2];
                lineTransforms.Remove(reeledT);
                Destroy(reeledT.gameObject);
            }
        }
        else
        {
            if ((lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position).magnitude >= 5)
            {
                if (lineTransforms[lineTransforms.Count - 3].GetComponent<HingeJoint>() == null)
                {
                    HingeJoint joint = lineTransforms[lineTransforms.Count - 3].gameObject.AddComponent<HingeJoint>();
                    joint.connectedBody = lineTransforms[lineTransforms.Count - 2].gameObject.GetComponent<Rigidbody>();
                    joint.connectedAnchor = lineTransforms[lineTransforms.Count - 2].position - lineTransforms[lineTransforms.Count - 3].position;
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
