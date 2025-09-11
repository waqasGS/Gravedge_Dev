using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatingMiniGame : MonoBehaviour
{
    public int machineNumber;
    public bool useTutorial;
    public void StartMiniGame()
    {
        if (HackingMinigame.Instance == null)
        {
            GameObject instance = Resources.Load<GameObject>("HackingMinigame");
            GameObject hackObject = Instantiate(instance, Vector3.zero, Quaternion.identity);
            HackingMinigame hacking = hackObject.GetComponent<HackingMinigame>();
            hacking.useTutorial = useTutorial;
            hacking.StartMiniGame();
            hacking.onReachedEndNode += () => Tutorial1.Instance.StartNotification(machineNumber);
        }
    }
}
