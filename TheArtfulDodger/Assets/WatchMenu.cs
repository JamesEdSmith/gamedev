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

    public SpriteRenderer watchRenderer;
    public SpriteRenderer bgRenderer;
    public GameObject[] pages;
    int currPage = 0;

    int currPicture = 0;

    public List<Sprite> images;

    private void Start()
    {
        watchRenderer.sprite = images[currPicture];
        foreach(GameObject page in pages)
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

    public void Reset()
    {
        currPicture = 0;
        watchRenderer.sprite = images[currPicture];
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

    internal bool isCurrPicture(GameObject picture)
    {
        if (picture.GetComponent<Picture>().index == currPicture)
        {
            currPicture++;
            if (currPicture < images.Count)
            {
                watchRenderer.sprite = images[currPicture];
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
