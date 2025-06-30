using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController.vActions;

public class SlideActionController : MonoBehaviour
{
    public bool slideStarted = false;

    private Animator playerAnimator;

    void Start()
    {
        playerAnimator = GameObject.FindObjectOfType<vGenericAction>().GetComponent<Animator>();
    }

    public void OnPlayerEnterTrigger(GameObject other)
    {
        if (!slideStarted)
        {
            slideStarted = true;
            SlideStarted();
        }
        else
        {
            slideStarted = false;
            SlideEnded();
        }
    }

    public void SlideStarted()
    {
        Debug.Log("Slide Started");
    }

    public void SlideEnded()
    {
        Debug.Log("Slide Ended");
    }
}