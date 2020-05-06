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

    ArrayList balls;

    public GameObject ballPrefab;

    const float RANDOM_RANGE = 5;

    // Use this for initialization
    void Start()
    {
        balls = new ArrayList();
    }

    internal void removeBall(Ball ball)
    {
        balls.Remove(ball);
    }

    // Update is called once per frame
    void Update()
    {
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
            if (balls.Count < 3)
            {
                GameObject newBall = Instantiate(ballPrefab, new Vector3(UnityEngine.Random.Range(-RANDOM_RANGE, RANDOM_RANGE), 0, UnityEngine.Random.Range(-RANDOM_RANGE, RANDOM_RANGE)), Quaternion.identity);
                newBall.GetComponent<Ball>().gameController = this;
                Ball ball = newBall.GetComponent<Ball>();
                balls.Add(newBall);
            }
        }

    }
}
