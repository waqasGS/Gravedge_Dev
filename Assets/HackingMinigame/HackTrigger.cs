using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (HackingMinigame.Instance == null)
        {
            GameObject instance = Resources.Load<GameObject>("HackingMinigame");
            Instantiate(instance, Vector3.zero, Quaternion.identity);
        }
    }
}