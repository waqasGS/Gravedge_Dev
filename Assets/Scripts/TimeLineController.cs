using UnityEngine;
using UnityEngine.Playables;

public class TimelineControl : MonoBehaviour
{
    public PlayableDirector timeline;

    void Update()
    {
        // Example input to stop
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopTimeline();
        }

        // Example input to play again
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayTimeline();
        }
    }

    public void StopTimeline()
    {
        if (timeline != null)
        {
            timeline.Pause();  // Pauses at current time
            // Or use timeline.Stop(); if you want to reset to time = 0
        }
    }

    public void PlayTimeline()
    {
        if (timeline != null)
        {
            timeline.Play();
        }
    }
}
