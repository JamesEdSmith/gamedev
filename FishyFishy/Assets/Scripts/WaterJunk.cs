using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterJunk : MonoBehaviour
{
    public GameObject prefab;
    private List<GameObject> specks;


    Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        specks = new List<GameObject>(25);
        Vector3 zOffset = Vector3.zero;
        int size = 10;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                zOffset.x = -200f + (400f * ((float)i / ((float)size-1f))) + UnityEngine.Random.Range(-10f, 10f);
                zOffset.y = 100 - (200f * ((float)j / ((float)size - 1f))) + UnityEngine.Random.Range(-10f, 10f);
                zOffset.z = 100 + UnityEngine.Random.Range(-50, 1000);

                GameObject temp = Instantiate(prefab, mainCamera.transform);
                specks.Add(temp);
                temp.transform.localPosition = zOffset;

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 zOffset = Vector3.zero;
        foreach (GameObject speck in specks)
        {
            bool visible = speck.GetComponent<Renderer>().isVisible;
            if (speck.transform.position.y >= 0 || (speck.transform.position.y > mainCamera.transform.position.y && !visible))
            {
                speck.transform.Translate(0, -100, 0);
            }
            else if ((speck.transform.position - mainCamera.transform.position).magnitude > 425)
            {
                zOffset.x = -200f + UnityEngine.Random.Range(0, 400f);
                zOffset.y = 100 - UnityEngine.Random.Range(0f, 200f);
                zOffset.z =  UnityEngine.Random.Range(-25, 0);

                speck.transform.parent = mainCamera.transform;
                speck.transform.localPosition = zOffset;
                speck.transform.parent = null;

            }

            speck.transform.LookAt(mainCamera.transform);

        }
    }

    internal void underWaterOn()
    {
        foreach (GameObject speck in specks)
        {
            speck.SetActive(true);
            speck.transform.parent = null;
        }
    }

    internal void underWaterOff()
    {
        Vector3 zOffset = Vector3.zero;
        int counter = 0;
        int size = 10;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                zOffset.x = -200f + (400f * ((float)i / ((float)size - 1f))) + UnityEngine.Random.Range(-10f, 10f);
                zOffset.y = 100 - (200f * ((float)j / ((float)size - 1f))) + UnityEngine.Random.Range(-10f, 10f);
                zOffset.z = 100 + UnityEngine.Random.Range(-50, 1000);

                specks[counter].transform.parent = mainCamera.transform;
                specks[counter].transform.localPosition = zOffset;
                specks[counter].SetActive(false);

                counter++;
            }
        }
    }
}
