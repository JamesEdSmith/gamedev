using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using UnityEngine;

public class Picture : GrabFreeTransformer, ITransformer
{
    GameObject hanger;
    Vector3 hangerOffset;
    public GameObject hangerPrefab;
    public float hangerShortenTime;
    float timer;
    Vector3 startPos;
    Quaternion startRot;
    public float width;
    public float height;
    public bool grabbed;

    public new void Initialize(IGrabbable grabbable)
    {
        base.Initialize(grabbable);
    }

    private void Start()
    {
        reset();
    }

    public void reset()
    {
        gameObject.SetActive(true);
        hangerOffset = new Vector3(0, 0.25f, 0.05f);
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

        grabbed = false;
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
        base.EndTransform();
        Rigidbody rigidbody = transform.gameObject.GetComponent<Rigidbody>();

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
        grabbed = false;

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

}