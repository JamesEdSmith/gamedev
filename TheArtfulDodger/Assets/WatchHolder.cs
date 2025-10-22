using Oculus.Interaction.Input;
using UnityEngine;

public class WatchHolder : MonoBehaviour
{

    public Hand leftHand;
    public WatchMenu watchMenu;

    bool shown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (leftHand.IsConnected && !shown)
        {
            shown = true;
            foreach (Transform child in gameObject.GetComponentsInChildren<Transform>())
            {
                child.gameObject.SetActive(true);
            }
        }
        else if (!leftHand.IsConnected && shown)
        {
            shown = false;
            foreach (Transform child in gameObject.GetComponentsInChildren<Transform>())
            {
                child.gameObject.SetActive(false);
            }

        }
    }


}
