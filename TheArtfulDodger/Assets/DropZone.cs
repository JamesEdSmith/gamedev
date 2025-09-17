using System;
using System.Collections.Generic;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    public List<MeshRenderer> binWalls;
    Color startingColor;
    float timer = 0;
    GameObject picture;

    private void Start()
    {
        startingColor = binWalls[0].material.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Picture"))
        {
            foreach (MeshRenderer renderer in binWalls)
            {
                renderer.material.color = Color.green;   
            }
            picture = other.gameObject;
            timer = 2;
        }
    }

    private void Update()
    {
        if (timer >= 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                foreach (MeshRenderer renderer in binWalls)
                {
                    renderer.material.color = startingColor;
                }
                picture.SetActive(false);
            }
        }
    }

    public void reset()
    {
        gameObject.SetActive(true);
        foreach (MeshRenderer renderer in binWalls)
        {
            renderer.material.color = startingColor;
        }
    }

    internal void fail()
    {
        foreach (MeshRenderer renderer in binWalls)
        {
            renderer.material.color = Color.red;
        }
        gameObject.SetActive(false);
    }
}
