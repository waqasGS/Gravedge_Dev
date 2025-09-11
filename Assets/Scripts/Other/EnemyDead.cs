using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDead : MonoBehaviour
{
    public float delayToDead;

    public void ToDestoryEnemy()
    {
        Debug.Log("Dead1");
        Invoke(nameof(TimeToDead), delayToDead);
    }
    private void TimeToDead()
    {

        Debug.Log("Dead1");
        Destroy(this.gameObject);
    }
}
