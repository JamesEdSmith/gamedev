using System;
using System.Collections.Generic;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    public List<MeshRenderer> binWalls;
    Color startingColor;
    float flashTimer = 0;
    GameObject picture;
    public Transform playerTransform;
    bool open;

    float openTimer;
    float openTime = 2.5f;

    Vector3 startingRot;
    Vector3 hingeRot;

    private void Start()
    {
        startingColor = binWalls[0].material.GetColor("_EmissionColor");
        open = false;
        openTimer = 0;
        startingRot = binWalls[1].transform.rotation.eulerAngles;
        hingeRot = startingRot + new Vector3(-90, 0, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Picture"))
        {
            foreach (MeshRenderer renderer in binWalls)
            {
                renderer.material.SetColor("_EmissionColor", Color.gold);
            }
            picture = other.gameObject;
            flashTimer = 2;
        }
    }

    private void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
            {
                foreach (MeshRenderer renderer in binWalls)
                {
                    renderer.material.SetColor("_EmissionColor", startingColor);
                }
                picture.SetActive(false);
            }
        }

        if (openTimer > 0)
        {
            openTimer -= Time.deltaTime;
            float t = (openTime - openTimer) / openTime;
            
            if (open)
            {
                float q = EasingFunction.EaseOutBack(0, 1, t);
                binWalls[1].transform.rotation = Quaternion.LerpUnclamped(Quaternion.Euler(startingRot), Quaternion.Euler(hingeRot), q);
            }
            else
            {
                float q = EasingFunction.EaseInBack(0, 1, t);
                binWalls[1].transform.rotation = Quaternion.LerpUnclamped(Quaternion.Euler(hingeRot), Quaternion.Euler(startingRot), q);
            }
        }
        else
        {
            openTimer = 0;
        }

        if (Vector3.Distance(playerTransform.position, transform.position) < 1.0f)
        {
            if (!open)
            {
                open = true;
                openTimer = openTime - openTimer;
            }
        }
        else
        {
            if (open)
            {
                open = false;
                openTimer = openTime - openTimer;
            }
        }
    }

    public void reset()
    {
        gameObject.SetActive(true);
        open = false;
        openTimer = 0;
        startingRot = binWalls[1].transform.rotation.eulerAngles;
        hingeRot = startingRot + new Vector3(-90, 0, 0);
        foreach (MeshRenderer renderer in binWalls)
        {
            renderer.material.SetColor("_EmissionColor", startingColor);
        }
    }

    internal void fail()
    {
        foreach (MeshRenderer renderer in binWalls)
        {
            renderer.material.SetColor("_EmissionColor", Color.red);
        }
        gameObject.SetActive(false);
    }
}
