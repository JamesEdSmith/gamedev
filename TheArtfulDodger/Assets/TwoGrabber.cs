using UnityEngine;
using Oculus.Interaction;
using TMPro;

public class TwoGrabber : GrabFreeTransformer, ITransformer
{
    bool grabbed;

    public new void BeginTransform()
    {
        grabbed = true;
        base.BeginTransform();
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;

    }

    public new void UpdateTransform()
    {
        base.UpdateTransform();
    }

    public new void EndTransform()
    {
        grabbed = false;
        base.EndTransform();
    }

}
