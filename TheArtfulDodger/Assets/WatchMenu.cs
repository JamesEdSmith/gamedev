using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

public class WatchMenu : MonoBehaviour
{
    [SerializeField]
    private Transform leftHand; // Assign HandDataLeft in the Inspector

    [SerializeField]
    public Transform centerEyeTransform;

    public MeshRenderer watchRenderer;
    public SpriteRenderer bgRenderer;
    public GameObject[] pages;
    int currPage = 0;

    int currPicture = 0;

    List<Material> images;

    public PicturePlacer placer;

    private void Start()
    {
        images = new List<Material>();
        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }
        pages[0].SetActive(true);
    }

    public void swapMenu()
    {
        pages[currPage].SetActive(false);

        currPage++;
        if (currPage >= pages.Length)
        {
            currPage = 0;
        }
        pages[currPage].SetActive(true);
    }

    public void reset()
    {
        currPicture = 0;
        changePicture();
    }

    private void changePicture()
    {
        float width = images[currPicture].GetTexture("_BaseMap").width;
        float height = images[currPicture].GetTexture("_BaseMap").height;

        watchRenderer.transform.parent.localScale = new Vector3(width / (width + height), watchRenderer.transform.parent.localScale.y, height / (width + height));

        watchRenderer.material = images[currPicture];
    }

    public void setImages(List<Material> images)
    {
        this.images = images;

        float width = images[currPicture].GetTexture("_BaseMap").width;
        float height = images[currPicture].GetTexture("_BaseMap").height;

        float w, h;
        if (width / (width + height) > height / (width + height))
        {
            w = 1;
            h = (height / (width + height)) / (width / (width + height));
        }
        else
        {
            h = 1;
            w = (width / (width + height)) / (height / (width + height));
        }

        watchRenderer.transform.parent.localScale = new Vector3(w, watchRenderer.transform.parent.localScale.y, h);
        watchRenderer.material = this.images[currPicture];
    }

    void Update()
    {
        float dist = Vector3.Distance(leftHand.position, centerEyeTransform.position);

        if (Vector3.Distance(leftHand.position + leftHand.up * dist, centerEyeTransform.position) < dist * 0.5f)
        {
            bgRenderer.gameObject.SetActive(true);
            transform.position = leftHand.transform.position + leftHand.transform.right * -0.15f + leftHand.transform.up * 0.02f + leftHand.transform.forward * 0.075f;
            transform.rotation = leftHand.transform.rotation;
        }
        else
        {
            bgRenderer.gameObject.SetActive(false);
        }
    }

    internal bool isCurrPicture(MeshRenderer picture)
    {
        if (picture.material.GetTexture("_BaseMap") == images[currPicture].GetTexture("_BaseMap"))
        {
            currPicture++;
            if (currPicture < images.Count)
            {
                changePicture();
            }
            else
            {
                placer.reset();
                reset();
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
