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

    float pictureTimer;
    float pictureTime = 1f;

    Vector3 startingRot;
    Vector3 hingeRot;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 startPosition;
    private Quaternion startRotation;
    public WatchMenu watch;
    PicturePlacer picturePlacer;

    private void Start()
    {
        picturePlacer = GameObject.Find("PicturePlacer").GetComponent<PicturePlacer>();
        startingColor = binWalls[0].material.GetColor("_EmissionColor");
        open = false;
        openTimer = 0;
        startingRot = binWalls[1].transform.rotation.eulerAngles;
        hingeRot = startingRot + new Vector3(-90, 0, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Picture") && watch.isCurrPicture(other.gameObject.GetComponent<Picture>().paintingRenderer))
        {
            foreach (MeshRenderer renderer in binWalls)
            {
                renderer.material.SetColor("_EmissionColor", Color.gold);
            }
            picture = other.gameObject;
            flashTimer = 2;
            other.GetComponent<Rigidbody>().isKinematic = true;

            startPosition = other.transform.position;
            startRotation = other.transform.rotation;
            targetPosition = transform.position + new Vector3(0, 0.035f, 0);
            targetRotation = transform.rotation * Quaternion.Euler(-90, 0, -90);

            pictureTimer = pictureTime;
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

        if (pictureTimer > 0)
        {
            pictureTimer -= Time.deltaTime;

            float t = (pictureTime - pictureTimer) / pictureTime;
            float q = EasingFunction.EaseOutCirc(0, 1, t);
            picture.GetComponent<Rigidbody>().position = Vector3.LerpUnclamped(startPosition, targetPosition, q);
            picture.GetComponent<Rigidbody>().rotation = Quaternion.LerpUnclamped(startRotation, targetRotation, q);
        }
        else
        {
            pictureTimer = 0;
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

        if (Vector3.Distance(playerTransform.position, transform.position) < 1.3f)
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
