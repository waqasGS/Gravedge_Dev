using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXController : MonoBehaviour
{
    public GameObject[] vfx;
    public Vector2 delay = new Vector2(1f, 3f);

    private void Start()
    {
        for (int i = 0; i < vfx.Length; i++)
        {
            StartCoroutine(PlayVFXCoroutine(i));
        }
    }

    IEnumerator PlayVFXCoroutine(int index)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(delay.x, delay.y));
            vfx[index].SetActive(true);
        }
    }
}