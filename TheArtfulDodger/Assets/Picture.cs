using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

public class Picture : GrabFreeTransformer, ITransformer
{
    public GameObject hanger;
    Vector3 hangerOffset;
    public GameObject hangerPrefab;
    public float hangerShortenTime;
    float timer;
    Vector3 startPos;
    Quaternion startRot;
    public float width;
    public float height;

    float colorTimer;
    Color colorSet;

    public int index;
    IGrabbable grabbable;

    public TextMeshPro textPro;
    public MeshRenderer paintingRenderer;
    public bool grabbed;

    public new void Initialize(IGrabbable grabbable)
    {
        base.Initialize(grabbable);
        this.grabbable = grabbable;
        //textPro.text = grabbable.GrabPoints.Count.ToString();
    }

    private void Start()
    {
        reset();
    }

    public void reset()
    {
        gameObject.SetActive(true);
        hangerOffset = new Vector3(0, 0.2f, 0.05f);
        if (hanger == null)
        {
            hanger = Instantiate(hangerPrefab, transform.position + hangerOffset, transform.rotation);
        }
        else
        {
            hanger.GetComponent<Rigidbody>().position = transform.position + hangerOffset;
            hanger.GetComponent<Rigidbody>().rotation = transform.rotation;
        }
        hanger.SetActive(false);
    }

    private void FixedUpdate()
    {

        if (timer >= 0 && hanger != null)
        {
            timer -= Time.deltaTime;
            float t = (hangerShortenTime - timer) / hangerShortenTime;
            float q = EasingFunction.EaseInOutBack(0, 1, t);
            transform.position = Vector3.LerpUnclamped(startPos, (hanger.transform.position - new Vector3(hangerOffset.x, hangerOffset.y, 0)) + hanger.transform.rotation * Vector3.forward * hangerOffset.z, q);
            transform.rotation = Quaternion.LerpUnclamped(startRot, hanger.transform.rotation, q);
        }

        if (colorTimer >= 0)
        {
            colorTimer -= Time.deltaTime;
            float t = 0.5f - timer / 0.5f;
            //GetComponent<MeshRenderer>().material.color = Color.Lerp(colorSet, Color.white, t);
        }

        //if (GetComponent<Rigidbody>().isKinematic)
        //{
        //    GetComponent<MeshRenderer>().material.color = Color.red;
        //}
        //else
        //{
        //    GetComponent<MeshRenderer>().material.color = Color.green;
        //}
    }

    public new void BeginTransform()
    {
        base.BeginTransform();
        grabbed = true;

        if (!hanger.activeSelf)
        {
            hanger.SetActive(true);
            hanger.transform.position = transform.position + new Vector3(hangerOffset.x, hangerOffset.y, 0) + transform.rotation * Vector3.forward * -hangerOffset.z;
            hanger.transform.rotation = transform.rotation;
        }
    }

    public new void UpdateTransform()
    {
        base.UpdateTransform();
    }

    public new void EndTransform()
    {
        grabbed = false;
        Rigidbody rigidbody = GetComponent<Rigidbody>();

        float dist = Vector3.Distance(transform.position
            + transform.rotation * Vector3.up * hangerOffset.y
            + transform.rotation * Vector3.forward * -hangerOffset.z, hanger.transform.position);
        if (dist < 0.3f)
        {
            timer = hangerShortenTime;
            startPos = transform.position;
            startRot = transform.rotation;
            rigidbody.isKinematic = true;
        }
        else
        {
            rigidbody.isKinematic = false;
            rigidbody.position = transform.position;
            rigidbody.rotation = transform.rotation;
        }

        base.EndTransform();


        //RaycastHit hit;
        //if (Physics.Raycast(transform.position, -transform.forward, out hit, 100))
        //{
        //    Transform myParent = hit.transform.parent;
        //    MRUKAnchor anchor = null;
        //    if (myParent != null)
        //    {
        //        anchor = myParent.gameObject.GetComponent<MRUKAnchor>();
        //    }
        //    float dist = Vector3.Distance(transform.position, hit.point);
        //    if (anchor != null && dist < 0.5f)
        //    {
        //        transform.position = hit.point;
        //        transform.rotation = Quaternion.LookRotation(hit.normal);
        //        transform.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        //        //Debug.Log("Hello, Joe");
        //    }
        //    else
        //    {
        //        transform.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        //    }
        //}
        //else
        //{
        //    transform.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        //}
    }

    public void SetMaterials(List<Material> paintingMat)
    {
        List<Transform> children = new List<Transform>(GetComponentsInChildren<Transform>(true));
        List<Transform> frames = children.FindAll(x => x.gameObject.name.Contains("FrameSelect"));

        foreach(Transform frame in frames)
        {
            frame.gameObject.SetActive(false);
        }

        frames[UnityEngine.Random.Range(0, frames.Count)].gameObject.SetActive(true);
        paintingRenderer.SetMaterials(paintingMat);
    }
}