using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    const float RESPAWN_BALL_TIME = 3f;

    float respawnBallTimer = RESPAWN_BALL_TIME;

    public Player player;
    public GameObject floor;
    public GameObject marker;
    public Ball[] balls;
    public GameObject[] keys;
    int keyIndex = 0;

    const float RANDOM_RANGE = 3;

    // Use this for initialization
    void Start()
    {
        if(keys[keyIndex] != null)
        {
            keys[keyIndex].GetComponent<Target>().activate(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            keys[keyIndex].GetComponent<Target>().activate(false);
            keyIndex++;
            if(keyIndex>= keys.Length)
            {
                keyIndex = 0;
            }
            keys[keyIndex].GetComponent<Target>().activate(true);
        }else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
            // create a ray from the mousePosition
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // plane.Raycast returns the distance from the ray start to the hit point
        float distance = 1000;
        if (floor.GetComponent<BoxCollider>().Raycast(ray, out RaycastHit info, distance))
        {
            var hitPoint = info.point;
            marker.transform.position = hitPoint;

        }

        respawnBallTimer -= Time.deltaTime;
        if (respawnBallTimer < 0)
        {
            respawnBallTimer = RESPAWN_BALL_TIME;
            foreach (Ball ball in balls)
            {
                if (ball.dead) {
                    ball.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    ball.gameObject.transform.position = new Vector3(UnityEngine.Random.Range(-RANDOM_RANGE, RANDOM_RANGE), 10, UnityEngine.Random.Range(-RANDOM_RANGE, RANDOM_RANGE));
                    ball.dead = false;
                }
            }
        }

    }

    internal Transform getActiveKey()
    {
        return keys[keyIndex].GetComponent<Target>().floor.transform;
    }
}
