using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterColor : MonoBehaviour
{
    public Color overWaterColor;
    public Color underWaterColor;
    Color tempColor;
    Material mat;
    public bool me;
    Transform cameraTransform;

    // Start is called before the first frame update
    void Start()
    {
        tempColor = new Color();
        mat = GetComponent<MeshRenderer>().material;
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        Transform checkTransform;
        if (me)
            checkTransform = transform;
        else
            checkTransform = cameraTransform;

        if (checkTransform.position.y > 0)
        {
            mat.color = overWaterColor;
        }
        else
        {
            tempColor.b = (100f + checkTransform.position.y) / 100f;
            tempColor.r = underWaterColor.r * tempColor.b;
            tempColor.g = underWaterColor.g * tempColor.b;
            tempColor = (underWaterColor + tempColor) / 2f;
            mat.color = tempColor;
        }
    }
}
