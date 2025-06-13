using UnityEngine;
using UnityEngine.Playables;

public class CutSceneController : MonoBehaviour
{
    public PlayableDirector _PlayableDirector;



    public void PlayCutScene()
    {
        _PlayableDirector.Play();
    }
}
