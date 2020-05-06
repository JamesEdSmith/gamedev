using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    public GameObject trailObject;
    public GameController gameController;

    // Use this for initialization
    void Start()
    {
        trailObject.SetActive(false);
    }

    // Update is called once per frame
    public void ActivateTrail(bool activate)
    {
        trailObject.SetActive(activate);
    }

    private void Update()
    {
        if (transform.position.y < 4)
        {
            gameController.removeBall(this);
            Destroy(this);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if ( player != null)//&& !player.holdingBall && player.canCatchBall
        {
            player.catchBall(this);
        }
    }
}
