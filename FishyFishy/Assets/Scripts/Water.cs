using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public Vector2 size;
    public GameObject tile;
    public Vector2 tileSize;
    // Start is called before the first frame update
    void Start()
    {
        Transform myTransform = transform;
        Vector3 pos = new Vector3();
        for (int i = 0; i < (int)size.x; i++)
        {
            for (int j = 0; j < (int)size.y; j++)
            {
                pos.x = (myTransform.position.x - size.x / 2) * tileSize.x + i * tileSize.x;
                pos.y = (myTransform.position.y - size.y / 2) * tileSize.y + j * tileSize.y;
                Instantiate(tile, pos, myTransform.rotation, myTransform);
            }
        }
        myTransform.Rotate(new Vector3(90, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
