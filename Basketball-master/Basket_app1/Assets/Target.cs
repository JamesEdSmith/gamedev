using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public bool activeKey;
    public GameObject floor;
    public GameObject board;

    public Material activeMaterial;
    public Material inactiveMaterial;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void activate(bool active)
    {
        activeKey = active;
        if (active)
        {
            floor.GetComponent<MeshRenderer>().material = activeMaterial;
            board.GetComponent<MeshRenderer>().material = activeMaterial;
        }
        else
        {
            floor.GetComponent<MeshRenderer>().material = inactiveMaterial;
            board.GetComponent<MeshRenderer>().material = inactiveMaterial;
        }
    }
}
