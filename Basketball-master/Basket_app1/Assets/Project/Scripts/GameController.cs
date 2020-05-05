using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public Player player;
    public GameObject floor;
    public GameObject marker;


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        // create a ray from the mousePosition
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // plane.Raycast returns the distance from the ray start to the hit point
        float distance = 1000;
        if (floor.GetComponent<BoxCollider>().Raycast(ray, out RaycastHit info, distance))
        {
            var hitPoint = info.point;
            marker.transform.position = hitPoint;

        }

    }
}
